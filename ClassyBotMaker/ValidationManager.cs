using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ClassyBotMaker
{
    public class ValidationManager
    {
        private readonly string baseDirectory;
        private readonly LoggingManager loggingManager;

        public ValidationManager(string baseDirectory, LoggingManager loggingManager)
        {
            this.baseDirectory = baseDirectory;
            this.loggingManager = loggingManager;
        }

        // Method to run validation tests for a single class
        public bool RunValidationTests(string className)
        {
            loggingManager.LogInfo($"Running validation tests for class: {className}");

            try
            {
                string nunitConsolePath = @"C:\NUnit\nunit3-console.exe"; // Should be configurable
                string testAssemblyPath = Path.Combine(baseDirectory, "ClassyBotMaker.Tests.dll");

                if (!File.Exists(nunitConsolePath))
                {
                    loggingManager.LogError("NUnit Console Runner not found. Please verify the installation path.");
                    return false;
                }

                if (!File.Exists(testAssemblyPath))
                {
                    loggingManager.LogError("Test assembly not found. Ensure that the test project is compiled and available.");
                    return false;
                }

                // Create the NUnit process to run the tests
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

                    // Read and capture the standard output and error
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    // Log the output and error
                    loggingManager.LogInfo("Test Output:");
                    loggingManager.LogInfo(output);

                    if (!string.IsNullOrEmpty(error))
                    {
                        loggingManager.LogError("Test Errors:");
                        loggingManager.LogError(error);
                    }

                    // Determine the success of the tests based on the process exit code
                    if (process.ExitCode == 0)
                    {
                        loggingManager.LogInfo($"All tests passed for class: {className}");
                        return true;
                    }
                    else
                    {
                        loggingManager.LogError($"Some tests failed for class: {className}. Please review the errors.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Exception while running validation tests for class {className}: {ex.Message}");
                return false;
            }
        }

        // Method to run validation tests for multiple classes concurrently
        public async Task<Dictionary<string, bool>> RunBatchValidationAsync(IEnumerable<string> classNames)
        {
            var tasks = classNames.Select(async className =>
            {
                bool result = await Task.Run(() => RunValidationTests(className));
                return new { className, result };
            });

            var results = await Task.WhenAll(tasks);
            return results.ToDictionary(r => r.className, r => r.result);
        }
    }
}
