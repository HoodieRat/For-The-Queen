using System;
using System.Collections.Generic;
using System.IO;

namespace ClassyBotMaker
{
    public class FileManager
    {
        private  string baseDirectory;
        private  LoggingManager loggingManager;

        // Constructor that initializes baseDirectory and loggingManager
        public FileManager(string baseDirectory, LoggingManager loggingManager)
        {
            this.baseDirectory = baseDirectory;
            this.loggingManager = loggingManager;
        }

        // Method to get the base directory
        public string GetBaseDirectory()
        {
            return baseDirectory;
        }

        // Create directories based on the input list of directory paths
        public void CreateDirectories(string[] directories)
        {
            foreach (var dir in directories)
            {
                var fullPath = Path.Combine(baseDirectory, dir);
                Directory.CreateDirectory(fullPath);
                loggingManager.LogInfo($"Directory created or already exists: {fullPath}");
            }
        }

        // Method to backup files from sourceDir to backupDir
        public static void BackupFiles(string sourceDir, string backupDir)
        {
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(backupDir, fileName);
                File.Copy(file, destFile, true);
            }
            Console.WriteLine($"Backup completed from {sourceDir} to {backupDir}");
        }

        public void BackupAllFiles(string sourceDirectory, string backupDirectory)
        {
            // Ensure backup directory exists
            Directory.CreateDirectory(backupDirectory);

            // Copy all files from the source to the backup directory
            foreach (string filePath in Directory.GetFiles(sourceDirectory))
            {
                string fileName = Path.GetFileName(filePath);
                string backupFilePath = Path.Combine(backupDirectory, $"{fileName}.backup_{DateTime.Now:yyyyMMddHHmmss}");

                File.Copy(filePath, backupFilePath, true);  // 'true' to overwrite any existing files
                loggingManager.LogInfo($"Backed up {fileName} to {backupFilePath}");
            }
        }

        // Save content to a specified file
        public void SaveFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
                loggingManager.LogInfo($"File saved: {filePath}");
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Failed to save file: {filePath}. Error: {ex.Message}");
            }
        }

        // Read content from a specified file
        public string ReadFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }
                else
                {
                    loggingManager.LogWarning($"File not found: {filePath}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Error reading file: {filePath}. Error: {ex.Message}");
                return null;
            }
        }

        // Check if a file exists
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        // Allow the user to enter the root directory via the console
        public void SelectRootDirectory()
        {
            Console.WriteLine("Please enter the root directory path or press Enter to use the default:");
            string userInput = Console.ReadLine();

            if (!string.IsNullOrEmpty(userInput) && Directory.Exists(userInput))
            {
                baseDirectory = userInput;
                loggingManager.LogInfo($"Root directory set to: {baseDirectory}");
            }
            else if (string.IsNullOrEmpty(userInput))
            {
                loggingManager.LogInfo($"Using the default directory: {baseDirectory}");
            }
            else
            {
                loggingManager.LogWarning($"Invalid directory: {userInput}. Using the default directory: {baseDirectory}");
            }
        }
        // Method to backup a file by copying it to a backup file
        public void BackupFile(string filePath)
        {
            try
            {
                string backupFilePath = filePath + $".backup_{DateTime.Now:yyyyMMddHHmmss}";
                File.Copy(filePath, backupFilePath, true);
                loggingManager.LogInfo($"Backed up file: {filePath} to {backupFilePath}");
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Failed to backup file: {filePath}. Error: {ex.Message}");
            }
        }
        // Optionally scan directories and display numerical options to choose from
        public void SelectRootDirectoryFromList(string parentDirectory)
        {
            try
            {
                Console.WriteLine("Available directories:");
                var directories = Directory.GetDirectories(parentDirectory);
                for (int i = 0; i < directories.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {directories[i]}");
                }

                Console.WriteLine("Please select a directory number, or press Enter to use the default:");
                string choice = Console.ReadLine();

                if (int.TryParse(choice, out int selectedNumber) && selectedNumber > 0 && selectedNumber <= directories.Length)
                {
                    baseDirectory = directories[selectedNumber - 1];
                    loggingManager.LogInfo($"Root directory set to: {baseDirectory}");
                }
                else if (string.IsNullOrEmpty(choice))
                {
                    loggingManager.LogInfo($"Using the default directory: {baseDirectory}");
                }
                else
                {
                    loggingManager.LogWarning("Invalid selection. Using the default directory.");
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Error selecting directory from list. Error: {ex.Message}");
            }
        }

        // Method to retrieve all bot files in the bots directory
        public List<string> GetBotFiles()
        {
            try
            {
                string botsDirectory = Path.Combine(baseDirectory, "bots");
                if (Directory.Exists(botsDirectory))
                {
                    return new List<string>(Directory.GetFiles(botsDirectory, "*.js")); // Assuming bot files are JavaScript files
                }
                else
                {
                    loggingManager.LogWarning($"Bots directory not found: {botsDirectory}");
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Error retrieving bot files. Error: {ex.Message}");
                return new List<string>();
            }
        }

        // Restore a backup by extracting it to the base directory
        public void RestoreBackup(string backupFilePath)
        {
            try
            {
                if (File.Exists(backupFilePath))
                {
                    string restoreDirectory = Path.Combine(baseDirectory, "restored");
                    if (!Directory.Exists(restoreDirectory))
                    {
                        Directory.CreateDirectory(restoreDirectory);
                    }
                    System.IO.Compression.ZipFile.ExtractToDirectory(backupFilePath, restoreDirectory);
                    loggingManager.LogInfo($"Backup restored to {restoreDirectory}");
                }
                else
                {
                    loggingManager.LogWarning($"Backup file not found: {backupFilePath}");
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Failed to restore backup from {backupFilePath}. Error: {ex.Message}");
            }
        }

        // Delete a specified file
        public void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    loggingManager.LogInfo($"File deleted: {filePath}");
                }
                else
                {
                    loggingManager.LogWarning($"File not found: {filePath}");
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Failed to delete file: {filePath}. Error: {ex.Message}");
            }
        }

        // Get all files in a directory with a specified extension
        public List<string> GetFilesWithExtension(string directory, string extension)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    return new List<string>(Directory.GetFiles(directory, $"*.{extension}"));
                }
                else
                {
                    loggingManager.LogWarning($"Directory not found: {directory}");
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Error retrieving files with extension {extension} from {directory}. Error: {ex.Message}");
                return new List<string>();
            }
        }
    }
}
