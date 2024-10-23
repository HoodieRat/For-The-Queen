using ClassyBotMaker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClassyBotMaker
{
    public class BatchUpdateManager
    {
        private readonly string baseDirectory;
        private readonly OpenAiHandler openAiHandler;
        private readonly DashboardManager dashboardManager;
        private DependencyManager dependencyManager;
        LoggingManager loggingManager;  
        public BatchUpdateManager(string baseDirectory, OpenAiHandler openAiHandler, DashboardManager dashboardManager)
        {
            this.baseDirectory = baseDirectory;
            this.openAiHandler = openAiHandler;
            this.dashboardManager = dashboardManager;
        }

        // Method to extract all dependencies, including indirect references, to create a full dependency map.
        public Dictionary<string, string> ExtractAllDependencies(string className)
        {
            Console.WriteLine($"Extracting dependencies for class: {className}");

            var dependencies = new Dictionary<string, string>();
            string[] allFiles = Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories);

            foreach (string file in allFiles)
            {
                string fileContent = File.ReadAllText(file);

                // Identify direct dependencies based on class reference
                if (Regex.IsMatch(fileContent, $@"\b{className}\b") && !file.EndsWith($"{className}.cs"))
                {
                    dependencies[file] = fileContent;
                }

                // Identify indirect dependencies by checking references to other dependencies
                foreach (var dependency in dependencies)
                {
                    if (Regex.IsMatch(fileContent, $@"\b{Path.GetFileNameWithoutExtension(dependency.Key)}\b") && !dependencies.ContainsKey(file))
                    {
                        dependencies[file] = fileContent;
                    }
                }
            }

            return dependencies;
        }

        // Method to prepare and send a batch update request to AI
        public async Task<string> PrepareBatchUpdatePrompt(string className, string updatedClassCode)
        {
            Console.WriteLine($"Preparing batch update prompt for dependencies of class: {className}");

            var dependencies = ExtractAllDependencies(className);
            if (dependencies.Count == 0)
            {
                Console.WriteLine("No dependent classes found to update.");
                return null;
            }

            // Construct prompt for batch updating all dependencies
            StringBuilder promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"The following class named '{className}' was updated. Here is the new version:\n{updatedClassCode}\n");
            promptBuilder.AppendLine("The following dependent classes need to be updated to be compatible with the changes:");

            foreach (var dependency in dependencies)
            {
                promptBuilder.AppendLine($"Class Name: {Path.GetFileNameWithoutExtension(dependency.Key)}\nCode:\n{dependency.Value}\n");
            }

            promptBuilder.AppendLine("Ensure all changes are consistent, complete, and functional.");

            string systemMessage = "You are an expert C# developer specializing in code integration and refactoring.";

            return await openAiHandler.SendOpenAiApiRequestAsync(promptBuilder.ToString(), systemMessage);
        }

        // Method to process the response from AI and apply the updates to all dependent files
        public void ProcessBatchUpdateResponse(string updatedBatchCode)
        {
            Console.WriteLine("Processing AI response for batch update...");

            if (string.IsNullOrEmpty(updatedBatchCode))
            {
                Console.WriteLine("Failed to receive a valid response for batch update of dependencies.");
                return;
            }

            // Split the response into individual class updates
            var updatedClasses = SplitUpdatedBatchCode(updatedBatchCode);

            foreach (var updatedClass in updatedClasses)
            {
                string classFilePath = updatedClass.Key;
                string classCode = updatedClass.Value;

                File.WriteAllText(classFilePath, classCode);
                Console.WriteLine($"Updated dependent file: {classFilePath}");
                UpdateDashboardWithProgress(classFilePath, "Updated dependent class to maintain consistency.");
            }
        }

        // Helper method to split the batch updated code into individual class updates
        private Dictionary<string, string> SplitUpdatedBatchCode(string updatedBatchCode)
        {
            var updatedClasses = new Dictionary<string, string>();
            string[] splitUpdates = Regex.Split(updatedBatchCode, "(?<=\n)Class Name: (.+?)\n");

            for (int i = 1; i < splitUpdates.Length; i += 2)
            {
                string className = splitUpdates[i].Trim();
                string classCode = splitUpdates[i + 1].Trim();

                string classFilePath = Path.Combine(baseDirectory, $"{className}.cs");
                if (!updatedClasses.ContainsKey(classFilePath))
                {
                    updatedClasses[classFilePath] = classCode;
                }
            }

            return updatedClasses;
        }

        // Method to re-prompt AI to fix failed batch updates
        public async Task HandleBatchUpdateFailures(Dictionary<string, string> failedClasses)
        {
            loggingManager.LogInfo("Handling batch update failures...");

            foreach (var failedClass in failedClasses)
            {
                string className = Path.GetFileNameWithoutExtension(failedClass.Key);
                string failedCode = failedClass.Value;

                string prompt = $"The following class named '{className}' was updated but failed validation. Please fix the issues and ensure all changes are functional:\n\n" +
                                $"Failed Class Code:\n{failedCode}\n\nEnsure all changes are consistent, complete, and production-ready.";

                string systemMessage = "You are an expert C# developer specializing in code debugging and refactoring.";

                string correctedCode = await openAiHandler.SendOpenAiApiRequestAsync(prompt, systemMessage);

                if (!string.IsNullOrEmpty(correctedCode))
                {
                    try
                    {
                        UtilityManager.SaveBackup(failedClass.Key, failedCode);
                        File.WriteAllText(failedClass.Key, correctedCode);
                        loggingManager.LogInfo($"Successfully corrected class: {failedClass.Key}");
                        dashboardManager.UpdateDashboardWithProgress(failedClass.Key, "Corrected and validated the failed class update.");
                    }
                    catch (Exception ex)
                    {
                        loggingManager.LogError($"Failed to save corrected class {failedClass.Key}: {ex.Message}");
                    }
                }
                else
                {
                    loggingManager.LogError($"Failed to correct the class: {failedClass.Key}. Manual intervention required.");
                    dashboardManager.UpdateDashboardWithProgress(failedClass.Key, "Failed correction. Manual intervention required.");
                }
            }
        }


        // Method to update the dashboard with progress details
        private void UpdateDashboardWithProgress(string componentName, string progressDetails)
        {
            dashboardManager.UpdateDashboardWithProgress(componentName, progressDetails);
        }

        // Method to run validation tests after updating a class
        public bool RunValidationTests(string className)
        {
            Console.WriteLine($"Running validation tests for class: {className}");

            try
            {
                string nunitConsolePath = @"C:\NUnit\nunit3-console.exe";
                string testAssemblyPath = Path.Combine(baseDirectory, "ClassyBotMaker.Tests.dll");

                if (!File.Exists(nunitConsolePath))
                {
                    Console.WriteLine("NUnit Console Runner not found. Please verify the installation path.");
                    return false;
                }

                if (!File.Exists(testAssemblyPath))
                {
                    Console.WriteLine("Test assembly not found. Ensure that the test project is compiled and available.");
                    return false;
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = nunitConsolePath,
                    Arguments = $"\"{testAssemblyPath}\" --where \"class =~ {className}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    Console.WriteLine("Test Output:");
                    Console.WriteLine(output);

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Test Errors:");
                        Console.WriteLine(error);
                    }

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while running validation tests for class {className}: {ex.Message}");
                return false;
            }
        }
        // Method to update dependent classes based on changes in a core class
        public void BatchUpdateDependentClasses(string updatedClassName, string updatedClassCode)
        {
            loggingManager.LogInfo($"Batch updating dependencies for {updatedClassName}...");

            var dependencies = DependencyManager.ExtractDependencies(baseDirectory, updatedClassName);

            foreach (var dependency in dependencies)
            {
                string dependencyFilePath = dependency.Key;
                string dependencyCode = dependency.Value;

                try
                {
                    // Backup the original dependent class code before modifying
                    UtilityManager.SaveBackup(dependencyFilePath, dependencyCode);

                    // Use Regex to find precise instances of the class reference and update them
                    string updatedDependencyCode = Regex.Replace(
                        dependencyCode,
                        $@"(?<![A-Za-z0-9_]){updatedClassName}(?![A-Za-z0-9_])",
                        updatedClassCode,
                        RegexOptions.Multiline
                    );

                    // Save the updated dependent class code back to the file
                    File.WriteAllText(dependencyFilePath, updatedDependencyCode);

                    loggingManager.LogInfo($"Updated dependent class: {dependencyFilePath}");
                }
                catch (Exception ex)
                {
                    loggingManager.LogError($"Failed to update dependent class {dependencyFilePath}: {ex.Message}");
                }
            }
        }

    }
}
