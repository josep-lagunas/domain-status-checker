using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DomainAvailabilityChecker
{
    internal static partial class Program
    {
        private static string _outputFilePath;
        private const string InputFileRelativePrefix = "../../..";
        private const string OutputResultsFileRelativePrefix = "../../../output_files";
        private const string InputFileName = "domain-names.txt";
        private const bool LogAnyStatus = true;
        private static Stream _stream;
        private static StreamWriter _writer;
        private static int _count;
        private static bool _flag;
        
        private static Queue<string> _domainsQueue;
        private static object _flagLock;
         private static bool _readyToFinish;

        private static void Main(string[] args)
        {
            Initialize();

            EnableFlag();

            //start reading input file and queueing
            Helpers.ReadFileLine(
                $"{InputFileRelativePrefix}/{InputFileName}",
                _domainsQueue.Enqueue,
                () =>
                {
                    _readyToFinish = true;
                    Helpers.LogTestCompleted();
                });

            //start processing queued candidates
            var queueProcessorThread = new Thread(ProcessDomainsQueue);
            queueProcessorThread.Start();
        }

        private static void Initialize()
        {
            _readyToFinish = false;
            Helpers.CreateOutputFolderIfNeeded(OutputResultsFileRelativePrefix);
            _outputFilePath = Helpers.BuildOutputFileName(OutputResultsFileRelativePrefix);

            _count = 0;
            _flagLock = new object();

            _domainsQueue = new Queue<string>();

            _stream = File.Open(_outputFilePath, FileMode.OpenOrCreate, FileAccess.Write,
                FileShare.Read);
            _writer = new StreamWriter(_stream) {AutoFlush = true};
            
        }

        private static void ProcessDomainsQueue()
        {
            var freeDomainsCount = 0;
            var takenDomainsCount = 0;
            var invalidDomainCount = 0;
            Helpers.LogInvalidDomainCount(invalidDomainCount);
            Helpers.LogFreeDomainCount(freeDomainsCount);
            Helpers.LogTakenDomainCount(takenDomainsCount);
            Helpers.LogTotalDomainsTested(0);
            Helpers.WriteTextTo(3, 2, "Testing:");
            while (_domainsQueue.Count > 0)
            {
                while (!_flag || _domainsQueue.Count == 0)
                {
                    //avoiding being banned
                    Thread.Sleep(20);
                }

                lock (_domainsQueue)
                {
                    var domain = _domainsQueue.Dequeue();

                    int totalProcessed;
                    if (!Helpers.IsValidDomain(domain))
                    {
                        Helpers.LogInvalidDomainCount(++invalidDomainCount); 
                        totalProcessed = freeDomainsCount + takenDomainsCount + invalidDomainCount;

                        Helpers.LogAccProgressTestedDomains(CheckStatus.Invalid, totalProcessed);
                        continue;
                    }

                    Helpers.LogTestedDomain(domain);
                    var result = GetDomainInfo(domain);
                    _count++;

                    var contentResult = $"{_count:00000}; {domain}; {result.Status}";
                    if (result.Status == CheckStatus.Free)
                    {
                        contentResult +=
                            $" {result.FirstDomainPrice}{result.Currency}; {result.RealDomainPrice}{result.Currency}";
                        Helpers.LogFreeDomainCount(++freeDomainsCount);
                    }
                    else
                        Helpers.LogTakenDomainCount(++takenDomainsCount);

                    totalProcessed = freeDomainsCount + takenDomainsCount + invalidDomainCount;
                    Helpers.LogAccProgressTestedDomains(result.Status, totalProcessed);
                    Helpers.LogTotalDomainsTested(totalProcessed);

                    if (result.Status == CheckStatus.Free || LogAnyStatus)
                        LogToFile(contentResult);
                }
            }
        }

        private static void LogToFile(string contentResult)
        {
            _writer.WriteLine(contentResult);
        }

        private static DomainInfoResponse GetDomainInfo(string domain)
        {
            DisableFlag();
            var success = HttpHelpers.DoRequest(domain, out var json);
            EnableFlag();

            if (!success)
                return DomainInfoResponse.Create(domain, CheckStatus.Unknown, 0, 0, "");

            try
            {
                var isAvailable = (bool) json["ExactMatchDomain"]["IsAvailable"];
                var price = (decimal) json["Products"][0]["PriceInfo"]["CurrentPrice"];
                var renewPrice = (decimal) json["Products"][0]["PriceInfo"]["AdditionalYearsPrice"];
                var currencySymbol = (string) json["ClientRequestIn"]["CurrencySymbol"];
                var status = isAvailable ? CheckStatus.Free : CheckStatus.Taken;

                return DomainInfoResponse.Create(domain, status, price, renewPrice, currencySymbol);
            }
            catch (Exception ex)
            {
                return DomainInfoResponse.Create(domain, CheckStatus.Unknown, 0, 0, "");
            }
        }

        private static void EnableFlag()
        {
            lock (_flagLock)
            {
                {
                    if (_flag) return;
                    _flag = true;
                }
            }
        }

        private static void DisableFlag()
        {
            lock (_flagLock)
            {
                if (!_flag) return;
                _flag = false;
            }
        }
    }
}