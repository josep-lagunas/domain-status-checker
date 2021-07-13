using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace DomainAvailabilityChecker
{
    internal static class Helpers
    {
        private const int ScrollOffsetLines = 13;
        private static readonly int OffsetTop;
        private static string _previousCheckedDomain;

        static Helpers()
        {
            OffsetTop = Console.CursorTop;
            _previousCheckedDomain = string.Empty;
            //AddScrollOffset();
        }

        private static void AddScrollOffset()
        {
            for (var i = 0; i < ScrollOffsetLines; i++)
                Console.WriteLine();
        }

        internal static string BuildOutputFileName(string relativePath)
        {
            var strTimeStamp =
                DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace('/', '_');

            return $"{relativePath}/output-file-{strTimeStamp}.txt";
        }

        internal static void CreateOutputFolderIfNeeded(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        internal static void ReadFileLine(string filePath, Action<string> executor,
            Action onFinish)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
                executor(reader.ReadLine());

            onFinish();
        }

        internal static bool IsValidDomain(string domain)
        {
            const string pattern = "^([a-z0-9]+(-[a-z0-9]+)*\\.)+[a-z]{2,}$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.IsMatch(domain);
        }

        internal static void WriteTextTo(int left, int top, string text, ConsoleColor color=default)
        {
            Console.CursorTop = OffsetTop;
            top += OffsetTop;
            Console.ForegroundColor = color;
            Console.SetCursorPosition(left, top);
            Console.Write(text);
            Console.ResetColor();
            SetCursorToEnd();
        }

        internal static void LogTestedDomain(string domain)
        {
            var clearStr = new string(' ', _previousCheckedDomain.Length);
            WriteTextTo(15, 1, clearStr);
            _previousCheckedDomain = domain;
            WriteTextTo(15, 1, domain, ConsoleColor.DarkYellow);
        }

        internal static void LogFreeDomainCount(int count)
        {
            WriteTextTo(5, 3, $"Total Free Domains found  : {count}", ConsoleColor.DarkGreen);
        }

        internal static void LogTakenDomainCount(int count)
        {
            WriteTextTo(5, 5, $"Total Taken Domains found : {count}", ConsoleColor.Red);
        }

        internal static void LogTotalDomainsTested(int count)
        {
            WriteTextTo(5, 7, $"Total Tested Domains count: {count}", ConsoleColor.Yellow);
        }

        internal static void LogInvalidDomainCount(int count)
        {
            WriteTextTo(5, 9, $"Total invalid domain count: {count}", ConsoleColor.Red);
        }

        internal static void LogAccProgressTestedDomains(CheckStatus status, int totalCount)
        {
            Func<Tuple<string, ConsoleColor>> parseState = () =>
            {
                return status switch
                {
                    CheckStatus.Free => Tuple.Create(".", ConsoleColor.Green),
                    CheckStatus.Taken => Tuple.Create("T", ConsoleColor.Red),
                    CheckStatus.Unknown => Tuple.Create("?", ConsoleColor.Yellow),
                    CheckStatus.Invalid => Tuple.Create("F", ConsoleColor.Red),
                    _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
                };
            };
            var (item1, item2) = parseState();
            WriteTextTo(22 + totalCount, 11, item1, item2);
        }

        internal static void LogTestCompleted()
        {
            SetCursorToEnd("Test completed");
        }

        private static void SetCursorToEnd(string message = "Test in progress: ")
        {
            Console.SetCursorPosition(5, OffsetTop + 11);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(message);
            Console.CursorVisible = false;
        }
    }
}