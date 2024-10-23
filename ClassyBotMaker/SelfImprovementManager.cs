using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ClassyBotMaker
{
    public class SelfImprovementManager
    {
        private readonly DependencyManager dependencyManager;
        private readonly BatchUpdateManager batchUpdateManager;
        private readonly OpenAiHandler openAiHandler;
        private readonly ValidationManager validationManager;
        private readonly DashboardManager dashboardManager;
        private readonly PromptBuilder promptBuilder;
        private readonly LoggingManager loggingManager;
        private readonly FileManager fileManager;

        public SelfImprovementManager(
            DependencyManager dependencyManager,
            BatchUpdateManager batchUpdateManager,
            OpenAiHandler openAiHandler,
            ValidationManager validationManager,
            DashboardManager dashboardManager,
            PromptBuilder promptBuilder,
            LoggingManager loggingManager,
            FileManager fileManager)
        {
            this.dependencyManager = dependencyManager;
            this.batchUpdateManager = batchUpdateManager;
            this.openAiHandler = openAiHandler;
            this.validationManager = validationManager;
            this.dashboardManager = dashboardManager;
            this.promptBuilder = promptBuilder;
            this.loggingManager = loggingManager;
            this.fileManager = fileManager;
        }

        // Method to upgrade a class using AI
        public async Task UpgradeClassUsingAI(string className)
        {
            string baseDirectory = fileManager.GetBaseDirectory();
            loggingManager.LogInfo($"Requesting AI to upgrade the {className} class...");

            try
            {
                // In the UpgradeClassUsingAI method
                var dependencies = DependencyManager.ExtractDependencies(baseDirectory, className);

                string classFilePath = Path.Combine(baseDirectory, $"{className}.cs");

                // Ensure the class file exists
                if (!File.Exists(classFilePath))
                {
                    LogAndReportError($"Class file {classFilePath} not found.");
                    return;
                }

                // Read the current class code
                string classCode = ReadClassCode(classFilePath);
                if (string.IsNullOrEmpty(classCode))
                {
                    return; // Error is already logged in ReadClassCode method
                }

                // Build prompt using PromptBuilder
                string prompt = promptBuilder.BuildUpgradeClassPrompt(className, classCode, dependencies);
                string systemMessage = "You are an expert C# developer specializing in modular, object-oriented programming. Provide complete and functional code updates.";

                // Retry logic for AI request
                string updatedClassCode = await openAiHandler.RetryOpenAiRequestAsync(prompt, systemMessage);

                if (!string.IsNullOrEmpty(updatedClassCode))
                {
                    await SaveAndValidateUpdatedClass(className, classFilePath, classCode, updatedClassCode);
                }
                else
                {
                    LogAndReportError($"Failed to get a complete upgrade for class: {className}. Please retry.");
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"An unexpected error occurred during the upgrade process for {className}: {ex.Message}");
            }
        }

        // Helper method to save and validate updated class
        private async Task SaveAndValidateUpdatedClass(string className, string classFilePath, string originalClassCode, string updatedClassCode)
        {
            // Save the updated class code to a new versioned file
            string backupFilePath = UtilityManager.GetNextVersionedFileName(classFilePath);
            UtilityManager.SaveBackup(backupFilePath, originalClassCode);

            try
            {
                UtilityManager.SaveUpdatedClass(classFilePath, updatedClassCode);
                loggingManager.LogInfo($"Successfully updated class: {classFilePath}");

                // Run validation tests
                bool isValid = validationManager.RunValidationTests(className);
                if (isValid)
                {
                    loggingManager.LogInfo("All tests passed. Update is successful.");
                    batchUpdateManager.BatchUpdateDependentClasses(className, updatedClassCode);
                    UpdateDashboardWithProgress(className, $"Successfully updated and validated class with new improvements. Version: {backupFilePath}", "success");
                }
                else
                {
                    loggingManager.LogError("Validation failed. Reverting to the previous version.");
                    UtilityManager.RevertToBackup(classFilePath, backupFilePath);
                    UpdateDashboardWithProgress(className, $"Failed to validate updates. Reverted to the previous version. Version: {backupFilePath}", "error");

                    // Re-prompt AI with the working version and failed version to improve it
                    await HandleFailedUpdate(className, originalClassCode, updatedClassCode);
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Failed to save updated class file {classFilePath}: {ex.Message}");
            }
        }

        // Helper method to read class code
        private string ReadClassCode(string classFilePath)
        {
            try
            {
                return File.ReadAllText(classFilePath);
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Failed to read class file {classFilePath}: {ex.Message}");
                return null;
            }
        }

        // Handle failed updates by re-prompting OpenAI
        private async Task HandleFailedUpdate(string className, string oldClassCode, string newClassCode)
        {
            loggingManager.LogInfo("Re-prompting AI to correct the issues in the updated code.");

            string prompt = promptBuilder.BuildFixFailedUpdatePrompt(className, oldClassCode, newClassCode);
            string systemMessage = "You are an expert C# developer specializing in code debugging, optimization, and refactoring. Correct the issues in the updated code.";

            string correctedClassCode = await openAiHandler.RetryOpenAiRequestAsync(prompt, systemMessage);

            if (!string.IsNullOrEmpty(correctedClassCode))
            {
                UtilityManager.SaveUpdatedClass(Path.Combine(fileManager.GetBaseDirectory(), $"{className}.cs"), correctedClassCode);
                bool isValid = validationManager.RunValidationTests(className);

                if (isValid)
                {
                    loggingManager.LogInfo($"All tests passed for {className} after correction. Update is successful.");
                }
                else
                {
                    loggingManager.LogError($"Validation still failed for {className} after correction. Manual intervention is required.");
                }
            }
            else
            {
                loggingManager.LogError($"Failed to get corrected code from AI for {className}.");
            }
        }

        private void UpdateDashboardWithProgress(string componentName, string progressDetails, string status = "info")
        {
            dashboardManager.UpdateDashboardWithProgress(componentName, progressDetails, status);
        }

        // Helper method to log and report an error to the dashboard
        private void LogAndReportError(string message)
        {
            loggingManager.LogError(message);
            UpdateDashboardWithProgress("SelfImprovementManager", message, "error");
        }
    }
}
