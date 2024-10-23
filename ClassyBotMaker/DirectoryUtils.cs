using System;
using System.IO;

namespace ClassyBotMaker.Utils
{
    public static class DirectoryUtils
    {
        public static void EnsureDirectoriesExist()
        {
            string botProjectBase = @"C:\AiAIBot";
            string botLogsDir = Path.Combine(botProjectBase, "logs");
            string botBackupDir = Path.Combine(botProjectBase, "backups");

            string programBase = @"C:\Users\frien\source\repos\ClassyBotMaker\ClassyBotMaker";
            string programLogsDir = Path.Combine(programBase, "logs");
            string programBackupDir = Path.Combine(programBase, "backups");

            // Ensure directories exist
            Directory.CreateDirectory(botLogsDir);
            Directory.CreateDirectory(botBackupDir);
            Directory.CreateDirectory(programLogsDir);
            Directory.CreateDirectory(programBackupDir);

            Console.WriteLine("All necessary directories are set up.");
        }
    }
}
