using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace DomainAvailabilityChecker
{
    internal static class Helpers
    {
        private static int OffsetTop;

        static Helpers()
        {
            OffsetTop = Console.CursorTop;
        }
        internal static string BuildOutputFileName(string relativePath)
        {
            return
                $"{relativePath}/output-file-{DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace('/', '_')}.txt";
        }

        internal static void CreateOutputFolderIfNeeded(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        internal static void ReadFileLine(string filePath, Action<string> executor,
            Action onFinish)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                executor(reader.ReadLine());
            }

            onFinish();
        }

        internal static bool IsValidDomain(string domain)
        {
            const string pattern = "^([a-z0-9]+(-[a-z0-9]+)*\\.)+[a-z]{2,}$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.IsMatch(domain);
        }

        internal static void WriteTextTo(int left, int top, string text)
        {
            top += OffsetTop;
            Console.SetCursorPosition(left, top);
            Console.Write(text);
            SetCursorToEnd();
        }

        internal static void LogTestedDomain(string domain)
        {
            Console.SetCursorPosition(12, OffsetTop + 2);
            for (var i = 0; i < 100; i++)
                Console.Write(" ");
            WriteTextTo(12, OffsetTop + 1, domain);
        }

        internal static void LogFreeDomainCount(int count)
        {
            WriteTextTo(5, OffsetTop + 3, $"Total Free Domains found  : {count}");
        }

        internal static void LogTakenDomainCount(int count)
        {
            WriteTextTo(5, OffsetTop + 5, $"Total Taken Domains found : {count}");
        }

        internal static void LogTotalDomainsTested(int count)
        {
            WriteTextTo(5, OffsetTop + 7, $"Total Tested Domains count: {count}");
        }

        internal static void LogInvalidDomainCount(int count)
        {
            WriteTextTo(5, OffsetTop + 9, $"Total invalid domain count: {count}");
        }
        
        internal static void LogAccProgressTestedDomains(CheckStatus status, int totalCount)
        {
            Func<string> getCharacter = () =>
            {
                return status switch
                {
                    CheckStatus.Free => ".",
                    CheckStatus.Taken => "T",
                    CheckStatus.Unknown => "?",
                    CheckStatus.Invalid => "F",
                    _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
                };
            };
            WriteTextTo(22 + totalCount, OffsetTop + 11, getCharacter());
        }

        internal static void LogTestCompleted()
        {
            SetCursorToEnd("Test completed");
        }

        private static void SetCursorToEnd(string message = "Test in progress: ")
        {
            Console.SetCursorPosition(5, OffsetTop + 12);
            Console.Write(message);
            Console.CursorVisible = false;
        }
    }
}