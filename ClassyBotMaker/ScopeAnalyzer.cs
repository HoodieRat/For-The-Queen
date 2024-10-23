using ClassyBotMaker;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassyBotMaker
{
    public class ScopeAnalyzer
    {
        private readonly string baseDirectory;
        private readonly List<string> projectFiles;

        public ScopeAnalyzer(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
            projectFiles = new List<string>(Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories));
        }

        public List<string> GetImpactedFiles(string className)
        {
            var impactedFiles = new List<string>();
            foreach (var file in projectFiles)
            {
                string fileContent = File.ReadAllText(file);
                if (Regex.IsMatch(fileContent, $"\b{className}\b"))
                {
                    impactedFiles.Add(file);
                }
            }
            return impactedFiles;
        }

        public List<string> GetUnusedMethods()
        {
            var unusedMethods = new List<string>();
            foreach (var file in projectFiles)
            {
                string fileContent = File.ReadAllText(file);
                var methodMatches = Regex.Matches(fileContent, @"public|private|protected|internal void (\w+)\(");
                foreach (Match match in methodMatches)
                {
                    string methodName = match.Groups[1].Value;
                    if (!Regex.IsMatch(fileContent, $"\b{methodName}\b"))
                    {
                        unusedMethods.Add(methodName);
                    }
                }
            }
            return unusedMethods;
        }
    }

    public class AIProcessor
    {
        private readonly OpenAiHandler openAiHandler;
        private readonly ScopeAnalyzer scopeAnalyzer;
        private readonly ToDoListManager toDoListManager;
        private readonly ValidationManager validationManager;
        private readonly string baseDirectory;

        public AIProcessor(OpenAiHandler openAiHandler, ScopeAnalyzer scopeAnalyzer, ToDoListManager toDoListManager, ValidationManager validationManager, string baseDirectory)
        {
            this.openAiHandler = openAiHandler;
            this.scopeAnalyzer = scopeAnalyzer;
            this.toDoListManager = toDoListManager;
            this.validationManager = validationManager;
            this.baseDirectory = baseDirectory;
        }
        public async Task BatchProcessTaskAsync(string className, string aiResponse)
        {
            var impactedFiles = scopeAnalyzer.GetImpactedFiles(className);
            foreach (var file in impactedFiles)
            {
                string originalContent = File.ReadAllText(file);
                await UpdateFileContent(file, aiResponse, originalContent);
            }
        }
        public async Task ProcessToDoTaskAsync(string taskName)
        {
            var toDoItem = toDoListManager.GetTask(taskName);
            if (toDoItem == null)
            {
                Console.WriteLine($"Task '{taskName}' not found in the to-do list.");
                return;
            }

            toDoListManager.MarkTaskAsInProgress(taskName);
            Console.WriteLine($"Processing task: {taskName}");

            // Generate prompt for AI
            string prompt = GenerateTaskPrompt(toDoItem);
            string systemMessage = "You are an expert C# developer. Provide complete and production-quality code updates.";

            string aiResponse = await openAiHandler.SendOpenAiApiRequestAsync(prompt, systemMessage);
            if (!string.IsNullOrEmpty(aiResponse))
            {
                var impactedFiles = scopeAnalyzer.GetImpactedFiles(toDoItem.ClassName);
                foreach (var file in impactedFiles)
                {
                    BackupFile(file);
                    string originalContent = File.ReadAllText(file);  // Read the original file content
                    UpdateFileContent(file, aiResponse, originalContent);  // Pass originalContent as the third parameter
                }

                if (validationManager.RunValidationTests(toDoItem.ClassName))
                {
                    Console.WriteLine($"Task '{taskName}' completed successfully.");
                    toDoListManager.MarkTaskAsCompleted(taskName);
                    LogTaskCompletion(taskName);
                }
                else
                {
                    Console.WriteLine($"Validation failed for task '{taskName}'. Reverting changes.");
                    RevertBackupFiles(impactedFiles);
                    toDoListManager.MarkTaskAsFailed(taskName);
                }
            }
            else
            {
                Console.WriteLine($"Failed to get AI response for task '{taskName}'.");
                toDoListManager.MarkTaskAsFailed(taskName);
            }
        }

        private void BackupFile(string filePath)
        {
            string backupFilePath = filePath + $".backup_{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(filePath, backupFilePath);
            Console.WriteLine($"Backup created for file: {filePath}");
        }

        private async Task UpdateFileContent(string filePath, string newContent, string originalContent)
        {
            if (newContent.Length < originalContent.Length)
            {
                string prompt = $"The AI returned a shorter code snippet for {filePath}. Please explain why code was removed and ensure all functionality remains.";
                string aiResponse = await openAiHandler.SendOpenAiApiRequestAsync(prompt, "Explain code removal");
                newContent = aiResponse ?? newContent;  // Use updated AI response if available
            }

            File.WriteAllText(filePath, newContent);
            Console.WriteLine($"File updated: {filePath}");
        }


        private void RevertBackupFiles(List<string> impactedFiles)
        {
            foreach (var file in impactedFiles)
            {
                string backupFilePath = file + ".backup_*";
                string latestBackup = Directory.GetFiles(baseDirectory, backupFilePath).OrderByDescending(f => f).FirstOrDefault();
                if (latestBackup != null)
                {
                    File.Copy(latestBackup, file, true);
                    Console.WriteLine($"Reverted to backup for file: {file}");
                }
            }
        }

        private void LogTaskCompletion(string taskName)
        {
            string finishedTasksPath = Path.Combine(baseDirectory, "data", "finishedtasks.txt");
            File.AppendAllText(finishedTasksPath, $"{taskName} - Completed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        }

        private string GenerateTaskPrompt(ToDoItem task)
        {
            return $"Task: {task.Task}\nDescription: {task.Description}\nEnsure to keep all methods intact, improve performance, and add any enhancements suggested.";
        }
    }

}

