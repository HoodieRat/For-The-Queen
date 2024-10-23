using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ClassyBotMaker
{
    public class BotManager
    {
        private readonly string baseDirectory;
        private readonly FileManager fileManager;
        private readonly LoggingManager loggingManager;

        public BotManager(string baseDirectory, FileManager fileManager, LoggingManager loggingManager)
        {
            this.baseDirectory = baseDirectory;
            this.fileManager = fileManager;
            this.loggingManager = loggingManager;
        }

        public BotManager(string baseDirectory, FileManager fileManager)
        {
            this.baseDirectory = baseDirectory;
            this.fileManager = fileManager;
        }

        // Method to update bots using AI and OpenAiHandler
        public async Task UpdateBotsUsingAI(OpenAiHandler openAiHandler)
        {
            Console.WriteLine("Requesting AI to generate updates for bots...");

            foreach (var botFile in Directory.GetFiles(Path.Combine(baseDirectory, "bots"), "*.js"))
            {
                string botCode = File.ReadAllText(botFile);
                string systemMessage = "You are an expert in Minecraft bot development and JavaScript programming. Improve the functionality of the provided bot code, adding new features and optimizing existing ones.";

                // Corrected method name
                string updatedCode = await openAiHandler.SendOpenAiApiRequestAsync(botCode, systemMessage);

                if (!string.IsNullOrEmpty(updatedCode))
                {
                    string newFileName = GetNextVersionedFileName(botFile);
                    File.WriteAllText(newFileName, updatedCode);
                    Console.WriteLine($"Bot updated and saved as {newFileName}");
                }
                else
                {
                    Console.WriteLine($"Failed to update bot: {Path.GetFileName(botFile)}");
                }
            }
        }

        // Helper method to generate versioned filenames
        private string GetNextVersionedFileName(string originalFilePath)
        {
            string directory = Path.GetDirectoryName(originalFilePath);
            string fileName = Path.GetFileNameWithoutExtension(originalFilePath);
            string extension = Path.GetExtension(originalFilePath);

            var versionMatch = Regex.Match(fileName, @"_v(\d+)$");
            int currentVersion = 1;

            if (versionMatch.Success)
            {
                currentVersion = int.Parse(versionMatch.Groups[1].Value);
                fileName = fileName.Substring(0, versionMatch.Index);
            }

            int newVersion = currentVersion + 1;
            string newFileName = $"{fileName}_v{newVersion}{extension}";

            return Path.Combine(directory, newFileName);
        }
    }
}
