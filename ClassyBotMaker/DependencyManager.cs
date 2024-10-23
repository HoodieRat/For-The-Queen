using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClassyBotMaker
{
    public class DependencyManager
    {
        private readonly string baseDirectory;

        public DependencyManager(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
        }

        /// <summary>
        /// Extracts all direct and indirect dependencies for a given class by searching for all references.
        /// </summary>
        /// <param name="baseDirectory">The base directory to search within.</param>
        /// <param name="className">The name of the class to extract dependencies for.</param>
        /// <returns>A dictionary containing the file paths and their contents of the dependent classes.</returns>
        public static Dictionary<string, string> ExtractDependencies(string baseDirectory, string className)
        {
            var dependencies = new Dictionary<string, string>();
            string[] allFiles = Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories);

            foreach (string file in allFiles)
            {
                string fileContent;
                try
                {
                    fileContent = File.ReadAllText(file);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Failed to read file {file}: {ex.Message}");
                    continue;
                }

                // Identify direct dependencies by checking if the file references the class
                if (Regex.IsMatch(fileContent, $"\b{className}\b") && !file.EndsWith($"{className}.cs"))
                {
                    dependencies[file] = fileContent;
                }
            }

            // Identify indirect dependencies by traversing recursively through all related classes
            var indirectDependencies = new Dictionary<string, string>();
            foreach (var dependency in dependencies)
            {
                FindIndirectDependencies(baseDirectory, dependency.Key, dependencies, indirectDependencies);
            }

            // Merge indirect dependencies to complete the dependency map
            foreach (var indirectDependency in indirectDependencies)
            {
                if (!dependencies.ContainsKey(indirectDependency.Key))
                {
                    dependencies[indirectDependency.Key] = indirectDependency.Value;
                }
            }

            return dependencies;
        }

        /// <summary>
        /// Recursively identifies indirect dependencies for a given class file.
        /// </summary>
        /// <param name="baseDirectory">The base directory to search within.</param>
        /// <param name="classFilePath">The file path of the class to find dependencies for.</param>
        /// <param name="existingDependencies">The existing direct dependencies.</param>
        /// <param name="indirectDependencies">The dictionary to store indirect dependencies.</param>
        private static void FindIndirectDependencies(string baseDirectory, string classFilePath, Dictionary<string, string> existingDependencies, Dictionary<string, string> indirectDependencies)
        {
            string className = Path.GetFileNameWithoutExtension(classFilePath);
            string[] allFiles = Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories);

            foreach (string file in allFiles)
            {
                if (existingDependencies.ContainsKey(file) || indirectDependencies.ContainsKey(file) || file == classFilePath)
                {
                    continue;
                }

                string fileContent;
                try
                {
                    fileContent = File.ReadAllText(file);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Failed to read file {file}: {ex.Message}");
                    continue;
                }

                // Identify indirect dependencies by checking if the file references the given class
                if (Regex.IsMatch(fileContent, $"\b{className}\b"))
                {
                    indirectDependencies[file] = fileContent;
                    FindIndirectDependencies(baseDirectory, file, existingDependencies, indirectDependencies);
                }
            }
        }

        /// <summary>
        /// Gets all classes that depend on the specified class.
        /// </summary>
        /// <param name="className">The name of the class to find dependents for.</param>
        /// <returns>A list of file paths of the classes that depend on the specified class.</returns>
        public List<string> GetDependentClasses(string className)
        {
            var dependentClasses = new List<string>();
            string[] allFiles = Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories);

            foreach (string file in allFiles)
            {
                string fileContent;
                try
                {
                    fileContent = File.ReadAllText(file);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Failed to read file {file}: {ex.Message}");
                    continue;
                }

                if (Regex.IsMatch(fileContent, $"\b{className}\b") && !file.EndsWith($"{className}.cs"))
                {
                    dependentClasses.Add(file);
                }
            }

            return dependentClasses;
        }
    }
}
