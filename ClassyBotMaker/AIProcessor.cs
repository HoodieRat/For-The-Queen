using ClassyBotMaker;

public class AIProcessor
{
    private readonly OpenAiHandler openAiHandler;
    private readonly ScopeAnalyzer scopeAnalyzer;
    private readonly ToDoListManager toDoListManager;
    private readonly ValidationManager validationManager;
    private readonly PromptBuilder promptBuilder;
    private readonly LoggingManager loggingManager;  // Add LoggingManager
    private readonly string baseDirectory;

    public AIProcessor(OpenAiHandler openAiHandler, ScopeAnalyzer scopeAnalyzer, ToDoListManager toDoListManager, ValidationManager validationManager, PromptBuilder promptBuilder, LoggingManager loggingManager, string baseDirectory)
    {
        this.openAiHandler = openAiHandler;
        this.scopeAnalyzer = scopeAnalyzer;
        this.toDoListManager = toDoListManager;
        this.validationManager = validationManager;
        this.promptBuilder = promptBuilder;
        this.loggingManager = loggingManager;  // Assign LoggingManager
        this.baseDirectory = baseDirectory;
    }

    public async Task ProcessToDoTaskAsync(string taskName)
    {
        var toDoItem = toDoListManager.GetTask(taskName);
        if (toDoItem == null)
        {
            Console.WriteLine($"Task '{taskName}' not found in the to-do list.");
            return;
        }

        // Mark the task as in progress
        toDoListManager.MarkTaskAsInProgress(taskName);
        loggingManager.LogInfo($"Processing task: {taskName}");

        // Send the task to AI for processing
        toDoListManager.SendTaskToAI(toDoItem);

        // Generate prompt for AI using PromptBuilder
        string prompt = promptBuilder.BuildUpgradeClassPrompt(toDoItem.ClassName, toDoItem.ClassCode, null);  // Pass dependencies if needed
        string systemMessage = "You are an expert C# developer. Provide complete and production-quality code updates.";

        // Retry the initial request to ensure task processing starts
        string aiResponse = await openAiHandler.RetryOpenAiRequestAsync(prompt, systemMessage);
        if (!string.IsNullOrEmpty(aiResponse))
        {
            var impactedFiles = scopeAnalyzer.GetImpactedFiles(toDoItem.ClassName);
            foreach (var file in impactedFiles)
            {
                // Pass the LoggingManager to FileManager
                FileManager fileManager = new FileManager(baseDirectory, loggingManager);  // Pass both arguments

                // Backup the file
                fileManager.BackupFile(file);

                string originalContent = File.ReadAllText(file);  // Read the original file content
                string newContent = aiResponse;

                // Check if the AI removed any code
                if (newContent.Length < originalContent.Length)
                {
                    // Use PromptBuilder to ask AI if the new method is more advanced
                    string clarificationPrompt = promptBuilder.BuildClarificationPrompt(file, originalContent, newContent);
                    string advancedResponse = await openAiHandler.SendOpenAiApiRequestAsync(clarificationPrompt, "Is the new method more advanced?");
                    newContent = advancedResponse ?? newContent;  // Use updated AI response if available
                }

                // Implement the most recent code
                UpdateFileContent(file, newContent, originalContent);  // Update the file with the new content

                // Validate the new code
                if (validationManager.RunValidationTests(toDoItem.ClassName))
                {
                    loggingManager.LogInfo($"Task '{taskName}' completed successfully.");
                    toDoListManager.MarkTaskAsCompleted(taskName);
                    LogTaskCompletion(taskName);
                }
                else
                {
                    // If validation fails, use PromptBuilder to alert the AI with the full method and error
                    loggingManager.LogError($"Validation failed for task '{taskName}'. Reverting changes.");
                    RevertBackupFiles(impactedFiles);

                    // Add this section to log the error and build the error prompt
                    string errorLine = GetErrorLineFromLogs();  // Implement this to retrieve the line causing the error
                    loggingManager.LogError($"Error found in file '{file}' at line {errorLine}. Requesting AI to fix the method.");

                    string errorPrompt = promptBuilder.BuildErrorPrompt(file, originalContent, int.Parse(errorLine));
                    await openAiHandler.SendOpenAiApiRequestAsync(errorPrompt, "Fix the error in the method");

                    toDoListManager.MarkTaskAsFailed(taskName);
                }
            }
        }
        else
        {
            loggingManager.LogError($"Failed to get AI response for task '{taskName}'.");
            toDoListManager.MarkTaskAsFailed(taskName);
        }
    }

        private void BackupFile(string filePath)
    {
        string backupFilePath = $"{filePath}.backup_{DateTime.Now:yyyyMMddHHmmss}";
        FileManager.BackupFiles(filePath, backupFilePath);  // Call the FileManager's backup method
        loggingManager.LogInfo($"Backup created for file: {filePath}");
    }

    private void UpdateFileContent(string filePath, string newContent, string originalContent)
    {
        File.WriteAllText(filePath, newContent);
        loggingManager.LogInfo($"File updated: {filePath}");
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
                loggingManager.LogInfo($"Reverted to backup for file: {file}");
            }
        }
    }

    private void LogTaskCompletion(string taskName)
    {
        string finishedTasksPath = Path.Combine(baseDirectory, "data", "finishedtasks.txt");
        File.AppendAllText(finishedTasksPath, $"{taskName} - Completed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        loggingManager.LogInfo($"Task '{taskName}' marked as completed.");
    }

    private string GetErrorLineFromLogs()
    {
        // Implement logic to extract error line from logs
        return "42";  // Example, replace with actual implementation
    }
}
