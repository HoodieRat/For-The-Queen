using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ClassyBotMaker
{
    public static class UtilityManager
    {
        // Method to run validation tests after updating a class
        public static bool RunValidationTests(string baseDirectory, string className)
        {
            Console.WriteLine($"Running validation tests for class: {className}");

            try
            {
                string nunitConsolePath = @"C:\NUnit\nunit3-console.exe"; // Adjust path to NUnit console runner accordingly.
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
    
        public static void SaveUpdatedClass(string classFilePath, string updatedClassCode)
        {
            File.WriteAllText(classFilePath, updatedClassCode);
            Console.WriteLine($"Class updated: {classFilePath}");
        }

        // Method to save a backup of the current class code (text)
        public static void SaveBackup(string filePath, string classCode)
        {
            // Generate the next versioned file name for the backup
            string backupPath = GetNextVersionedFileName(filePath);
            File.WriteAllText(backupPath, classCode);
            Console.WriteLine($"Backup created: {backupPath}");
        }

        public static void RevertToBackup(string classFilePath, string backupDirectory)
        {
            // Generate the expected backup file pattern
            string latestBackupPath = GetLatestBackupFilePath(Path.Combine(backupDirectory, Path.GetFileName(classFilePath)));

            if (!string.IsNullOrEmpty(latestBackupPath))
            {
                try
                {
                    // Copy the latest backup version back to the original class file
                    File.Copy(latestBackupPath, classFilePath, overwrite: true);
                    Console.WriteLine($"Successfully reverted {Path.GetFileName(classFilePath)} to the latest backup version: {latestBackupPath}");
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine($"IO Error during revert: {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error during revert: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"No backup found to revert for class: {Path.GetFileName(classFilePath)}.");
            }
        }

        // Helper method to generate versioned filenames
        public static string GetNextVersionedFileName(string originalFilePath)
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

        private static string GetLatestBackupFilePath(string backupFilePattern)
        {
            string directory = Path.GetDirectoryName(backupFilePattern);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(backupFilePattern);
            string extension = Path.GetExtension(backupFilePattern);

            // Get all files in the directory that match the backup pattern
            string[] backupFiles = Directory.GetFiles(directory, $"{fileNameWithoutExtension}_v*{extension}");

            if (backupFiles.Length == 0)
            {
                Console.WriteLine("No backup files found.");
                return null;
            }

            // Sort the files based on version number in descending order
            Array.Sort(backupFiles, (a, b) =>
            {
                int versionA = ExtractVersionNumber(a);
                int versionB = ExtractVersionNumber(b);
                return versionB.CompareTo(versionA);
            });

            // Return the latest version
            return backupFiles[0];
        }

        // Helper method to extract version number from the file name
        private static int ExtractVersionNumber(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            var match = Regex.Match(fileName, @"_v(\d+)$");

            if (match.Success && int.TryParse(match.Groups[1].Value, out int version))
            {
                return version;
            }

            return 0; // Default to version 0 if no version number found
        }
    }
}
