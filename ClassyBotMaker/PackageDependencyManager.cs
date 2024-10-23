using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ClassyBotMaker
{

    public class PackageDependencyManager
    {
        private string baseDirectory;
        private List<string> dependencies;
        private readonly LoggingManager loggingManager;

        // Constructor for the PackageDependencyManager
        public PackageDependencyManager(string baseDirectory, IEnumerable<string> initialDependencies, LoggingManager loggingManager)
        {
            this.baseDirectory = baseDirectory;
            this.dependencies = new List<string>(initialDependencies ?? new List<string>());
            this.loggingManager = loggingManager;
        }

        // Method to check and install dependencies asynchronously
        public async Task CheckDependenciesAsync()
        {
            loggingManager.LogInfo("Checking for dependencies...");

            foreach (string dependency in dependencies)
            {
                if (!IsDependencyInstalled(dependency))
                {
                    loggingManager.LogInfo($"Installing dependency: {dependency}");
                    await InstallDependencyAsync(dependency);
                }
                else
                {
                    loggingManager.LogInfo($"Dependency {dependency} is already installed.");
                }
            }

            loggingManager.LogInfo("Dependency check complete.");
        }

        // Method to add a dependency
        public void AddDependency(string dependency)
        {
            if (!dependencies.Contains(dependency))
            {
                dependencies.Add(dependency);
                loggingManager.LogInfo($"Added dependency: {dependency}");
            }
        }

        // Method to remove a dependency
        public void RemoveDependency(string dependency)
        {
            if (dependencies.Contains(dependency))
            {
                dependencies.Remove(dependency);
                loggingManager.LogInfo($"Removed dependency: {dependency}");
            }
        }

        // Method to list all dependencies
        public List<string> ListDependencies()
        {
            return new List<string>(dependencies);
        }

        // Private helper method to check if a dependency is installed
        private bool IsDependencyInstalled(string dependency)
        {
            string nodeModulesPath = Path.Combine(baseDirectory, "node_modules", dependency);
            return Directory.Exists(nodeModulesPath);
        }

        // Private helper method to install a dependency asynchronously
        private async Task InstallDependencyAsync(string dependency)
        {
            try
            {
                loggingManager.LogInfo($"Starting installation for {dependency}...");

                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", $"/c npm install {dependency} --save")
                {
                    WorkingDirectory = baseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = processInfo;
                    process.Start();

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        loggingManager.LogInfo($"{dependency} installed successfully.\n{output}");
                    }
                    else
                    {
                        loggingManager.LogError($"Error installing {dependency}:\n{error}");
                    }
                }
            }
            catch (Exception ex)
            {
                loggingManager.LogError($"Exception occurred while installing {dependency}: {ex.Message}");
            }
        }
    }
}