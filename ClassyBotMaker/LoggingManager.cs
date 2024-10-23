using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClassyBotMaker
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    public class LoggingManager
    {
        private readonly string logFilePath;
        private readonly LogLevel minLogLevel;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public LoggingManager(LogLevel logLevel = LogLevel.Info, string logDirectory = "logs")
        {
            this.minLogLevel = minLogLevel;

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            logFilePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
        }

        // Method to log messages based on the log level
        public void Log(LogLevel level, string message)
        {
            if (level >= minLogLevel)
            {
                string formattedMessage = $"{level.ToString().ToUpper()}: {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                WriteToConsoleAndFile(formattedMessage);
            }
        }

        // Method to write to console and file asynchronously
        private void WriteToConsoleAndFile(string message)
        {
            Console.WriteLine(message);
            _ = WriteToFileAsync(message); // Fire-and-forget async file writing
        }

        // Asynchronous method to write to the log file
        private async Task WriteToFileAsync(string message)
        {
            await semaphore.WaitAsync();
            try
            {
                using (StreamWriter sw = new StreamWriter(logFilePath, true))
                {
                    await sw.WriteLineAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }

        // Method to clean up old log files older than specified days
        private void CleanupOldLogs(int daysToKeep = 7)
        {
            var directory = Path.GetDirectoryName(logFilePath);
            var logFiles = Directory.GetFiles(directory, "log_*.txt");

            foreach (var file in logFiles)
            {
                if (File.GetCreationTime(file) < DateTime.Now.AddDays(-daysToKeep))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete old log file '{file}': {ex.Message}");
                    }
                }
            }
        }

        // Convenience methods for logging at specific levels
        public void LogDebug(string message) => Log(LogLevel.Debug, message);
        public void LogInfo(string message) => Log(LogLevel.Info, message);
        public void LogWarning(string message) => Log(LogLevel.Warning, message);
        public void LogError(string message) => Log(LogLevel.Error, message);
        public void LogCritical(string message) => Log(LogLevel.Critical, message);
    }
}
