using ClassyBotMaker.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ClassyBotMaker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Ensure all directories exist before proceeding
            //DirectoryUtils.EnsureDirectoriesExist();
            LoggingManager loggingManager = new LoggingManager(LogLevel.Info, @"C:\AiAIBot\logs");

            // Log a startup message
            loggingManager.LogInfo("ClassyBotMaker program has started.");

            // Set the default base directory.
            string defaultBaseDirectory = @"C:\AiAIBot";
            //string baseDirectory = defaultBaseDirectory;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory; //normally you'd use the variable above for the bot default folder... 
            // Prompt the user to use the default base directory or enter a new one.
            Console.WriteLine($"The default base directory is set to: {defaultBaseDirectory}");
            Console.WriteLine("Would you like to use the default directory? (y/n)");

            string response = Console.ReadLine()?.Trim().ToLower();
            if (response == "n")
            {
                Console.WriteLine("Please enter a new base directory path:");
                string newDirectory = Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(newDirectory))
                {
                    // Update the base directory to the user's input.
                    baseDirectory = newDirectory;
                }
            }

            // Make sure the base directory exists; if not, create it.
            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
                Console.WriteLine($"Created new base directory: {baseDirectory}");
            }

            // Initialize the FileManager with the base directory.
            FileManager fileManager = new FileManager(baseDirectory, loggingManager);

            // Initialize BotManager with the base directory and the fileManager instance.
            BotManager botManager = new BotManager(baseDirectory, fileManager, loggingManager);

            // Load dependencies from a file in the data directory.
         
            string dependenciesFilePath = Path.Combine(baseDirectory, "data", "dependencies.txt");
            string[] dependencies = File.Exists(dependenciesFilePath)
                ? File.ReadAllLines(dependenciesFilePath)
                : new string[0]; // Empty array if the file is not found

            // Initialize the PackageDependencyManager
            PackageDependencyManager packageDependencyManager = new PackageDependencyManager(baseDirectory, dependencies, loggingManager);

            // Check and install dependencies before proceeding
            await packageDependencyManager.CheckDependenciesAsync();  // Ensure dependencies are installed

            // Initialize the DashboardManager for managing dashboard creation.
            DashboardManager dashboardManager = new DashboardManager(baseDirectory, loggingManager);

            // Initialize the OpenAiHandler for managing AI requests, with a max retry count of 3
            OpenAiHandler openAiHandler = new OpenAiHandler(3);  // Pass maxRetryCount as an integer

            // Initialize the MenuManager with all other managers.
            MenuManager menuManager = new MenuManager(botManager, packageDependencyManager, fileManager, openAiHandler, dashboardManager, loggingManager);

            // Show the menu and run the program.
            await menuManager.ShowMenuAsync();

            // Log the program end
            loggingManager.LogInfo("ClassyBotMaker program has ended.");
        }

    }
}
