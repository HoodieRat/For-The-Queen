using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassyBotMaker
{
    public class MenuManager
    {
        private readonly FileManager fileManager;
        private readonly BotManager botManager;
        private readonly PackageDependencyManager dependencyManager;
        private readonly DashboardManager dashboardManager;
        private readonly ToDoListManager toDoListManager;
        private readonly PackageDependencyManager packageDependencyManager;
        private readonly OpenAiHandler openAiHandler;
        private readonly LoggingManager loggingManager;

        public MenuManager(BotManager botManager, PackageDependencyManager packageDependencyManager, FileManager fileManager, OpenAiHandler openAiHandler, DashboardManager dashboardManager, LoggingManager loggingManager)
        {
            this.botManager = botManager;
            this.packageDependencyManager = packageDependencyManager;
            this.fileManager = fileManager;
            this.openAiHandler = openAiHandler;
            this.dashboardManager = dashboardManager;
            this.loggingManager = loggingManager;
        }

        public MenuManager(
            BotManager botManager,
            PackageDependencyManager dependencyManager,
            FileManager fileManager,
            OpenAiHandler openAiHandler,
            DashboardManager dashboardManager)
        {
            this.fileManager = fileManager;
            this.botManager = botManager;
            this.dependencyManager = dependencyManager;
            this.dashboardManager = dashboardManager;
            this.toDoListManager = new ToDoListManager(fileManager.GetBaseDirectory(), openAiHandler, dashboardManager);
        }

        public async Task ShowMenuAsync()
        {
            while (true)
            {
                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Backup existing files");
                Console.WriteLine("2. Check and Install Dependencies");
                Console.WriteLine("3. Generate Dependency Feedback Files");
                Console.WriteLine("4. Generate Base Bot Files with Advanced Features");
                Console.WriteLine("5. Update Existing Bot Files using AI");
                Console.WriteLine("6. Generate Personalities for Bots");
                Console.WriteLine("7. Test and Correct Bot Files");
                Console.WriteLine("8. Generate Dashboard");
                Console.WriteLine("9. Start Minecraft Bot on Server");
                Console.WriteLine("10. Show To-Do List");
                Console.WriteLine("11. Process Next To-Do Task");
                Console.WriteLine("12. Exit");

                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        BackupFiles();
                        break;
                    case "2":
                        await CheckAndInstallDependencies();
                        break;
                    case "3":
                        // Generate Dependency Feedback Files logic here...
                        break;
                    case "4":
                        // Generate Base Bot Files logic here...
                        break;
                    case "5":
                        // Update Existing Bot Files using AI logic here...
                        await botManager.UpdateBotsUsingAI(new OpenAiHandler());
                        break;
                    case "6":
                        // Generate Personalities for Bots logic here...
                        break;
                    case "7":
                        // Test and Correct Bot Files logic here...
                        break;
                    case "8":
                        List<string> botFiles = fileManager.GetBotFiles();
                        dashboardManager.GenerateBotDashboard(botFiles);
                        break;
                    case "9":
                        // Start Minecraft Bot on Server logic here...
                        break;
                    case "10":
                        ShowToDoList();
                        break;
                    case "11":
                        await toDoListManager.ProcessNextTask();
                        break;
                    case "12":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }
        private void BackupFiles()
        {
            Console.WriteLine("Backing up existing files...");
            string baseDirectory = @"c:\aiaibots\backups";
            // Ensure that baseDirectory is correctly initialized
            if (string.IsNullOrEmpty(baseDirectory))
            {
                Console.WriteLine("Base directory is not set. Cannot proceed with backup.");
                return;
            }

            // Set the actual source directory (the base directory of your bot project)
            string sourceDirectory = baseDirectory;  // This is the base directory, e.g., C:\AiAIBot

            // Set the backup directory to a "backups" folder inside the base directory
            string backupDirectory = Path.Combine(baseDirectory, "backups");

            // Ensure the backup directory exists
            Directory.CreateDirectory(backupDirectory);

            // Call the FileManager to backup all files
            fileManager.BackupAllFiles(sourceDirectory, backupDirectory);

            Console.WriteLine("Backup completed.");
        }

        // Method to check and install dependencies using PackageDependencyManager
        private async Task CheckAndInstallDependencies()
        {
            Console.WriteLine("Checking and installing dependencies...");
            await dependencyManager.CheckDependenciesAsync();
            Console.WriteLine("Dependencies check and installation complete.");
        }

        // Method to show the current to-do list
        private void ShowToDoList()
        {
            var tasks = toDoListManager.GetAllTasks();
            Console.WriteLine("\nCurrent To-Do List:");

            foreach (var task in tasks)
            {
                Console.WriteLine($"{task.Task} - Status: {task.Status}");
            }
        }
    }
}

