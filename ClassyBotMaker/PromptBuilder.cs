using System;
using System.Collections.Generic;
using System.Text;

namespace ClassyBotMaker
{
    public class PromptBuilder
    {
        // Method to build a prompt for upgrading a specific class using AI
        public string BuildUpgradeClassPrompt(string className, string classCode, Dictionary<string, string> dependencies)
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine($"Class Upgrade Request for '{className}'");
            promptBuilder.AppendLine("The following class is part of a C# console application that manages an AI-driven bot system.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Objective:");
            promptBuilder.AppendLine("1. Improve the class code.");
            promptBuilder.AppendLine("2. Refactor where necessary for better readability and modularity.");
            promptBuilder.AppendLine("3. Add any missing features, optimize performance, and enhance overall quality.");
            promptBuilder.AppendLine("4. Ensure all changes are production-ready and align with industry best practices.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Class Code:");
            promptBuilder.AppendLine(classCode);
            promptBuilder.AppendLine();

            if (dependencies != null && dependencies.Count > 0)
            {
                promptBuilder.AppendLine("Dependencies:");
                foreach (var dependency in dependencies)
                {
                    promptBuilder.AppendLine($"\nDependency File: {dependency.Key}\nDependency Code:\n{dependency.Value}\n");
                }
            }

            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Focus Areas:");
            promptBuilder.AppendLine("1. Performance improvements.");
            promptBuilder.AppendLine("2. Code modularity and maintainability.");
            promptBuilder.AppendLine("3. Identifying and fixing any potential issues or bad practices.");
            promptBuilder.AppendLine("4. If the class interacts with other classes or APIs, ensure proper handling and error resilience.");

            return promptBuilder.ToString();
        }

        // Method to build a prompt for batch updating dependent classes after a core class is updated
        public string BuildBatchUpdatePrompt(string updatedClassName, string updatedClassCode, Dictionary<string, string> dependencies)
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine($"Batch Update Request for Dependencies of Class: '{updatedClassName}'");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("The core class has been updated. Below is the new version:");
            promptBuilder.AppendLine("Updated Class Code:");
            promptBuilder.AppendLine(updatedClassCode);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("The following dependent classes need to be updated to ensure compatibility:");
            promptBuilder.AppendLine();

            foreach (var dependency in dependencies)
            {
                promptBuilder.AppendLine($"Dependent Class: {dependency.Key}");
                promptBuilder.AppendLine($"Dependency Code:\n{dependency.Value}");
                promptBuilder.AppendLine();
            }

            promptBuilder.AppendLine("Ensure the following while making updates:");
            promptBuilder.AppendLine("1. Consistency with the updated core class.");
            promptBuilder.AppendLine("2. Maintaining functionality without introducing errors.");
            promptBuilder.AppendLine("3. Comprehensive refactoring and improvements if needed.");
            promptBuilder.AppendLine("4. Production readiness of all dependent classes.");

            return promptBuilder.ToString();
        }

        // Method to build a prompt for fixing issues found in AI-generated code
        public string BuildFixFailedUpdatePrompt(string className, string originalClassCode, string failedUpdatedCode)
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine($"Fix Request for Failed Update of Class: '{className}'");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("The class was updated, but validation tests failed for the updated version. Your task is to fix the issues, refactor if needed, and ensure that all improvements are functional and optimized.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Original Class Code:");
            promptBuilder.AppendLine(originalClassCode);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Updated (Failed) Class Code:");
            promptBuilder.AppendLine(failedUpdatedCode);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Focus on ensuring the following:");
            promptBuilder.AppendLine("1. The updated class code is functional, with all errors fixed.");
            promptBuilder.AppendLine("2. Performance and modularity improvements are retained.");
            promptBuilder.AppendLine("3. The updated code is production-ready and aligns with the application's architecture.");

            return promptBuilder.ToString();
        }

        // Method to build a prompt to ask if shortened code is more advanced
        public string BuildClarificationPrompt(string className, string originalCode, string updatedCode)
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine($"Clarification Request: Shortened Code for '{className}'");
            promptBuilder.AppendLine("The AI has provided a shorter version of the method. Is this new code more advanced?");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Original Method:");
            promptBuilder.AppendLine(originalCode);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Shortened Method:");
            promptBuilder.AppendLine(updatedCode);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("If the new code is more advanced, please further enhance it.");

            return promptBuilder.ToString();
        }

        // Method to build a prompt for enhancing an entire project with focus areas
        public string BuildEnhanceProjectPrompt(string projectOverview, List<string> classNames, Dictionary<string, string> allClasses)
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine("Project Enhancement Request");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Overview:");
            promptBuilder.AppendLine(projectOverview);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Objective:");
            promptBuilder.AppendLine("1. Improve the quality, performance, and modularity of the entire project.");
            promptBuilder.AppendLine("2. Refactor and restructure classes to make the project more maintainable.");
            promptBuilder.AppendLine("3. Add any missing features that enhance the usability or functionality of the project.");
            promptBuilder.AppendLine();

            promptBuilder.AppendLine("Classes Involved:");
            foreach (var className in classNames)
            {
                promptBuilder.AppendLine($"- {className}");
            }
            promptBuilder.AppendLine();

            promptBuilder.AppendLine("Class Details:");
            foreach (var classInfo in allClasses)
            {
                promptBuilder.AppendLine($"Class Name: {classInfo.Key}");
                promptBuilder.AppendLine($"Class Code:\n{classInfo.Value}\n");
            }

            promptBuilder.AppendLine("Focus on:");
            promptBuilder.AppendLine("1. Comprehensive improvements across all classes.");
            promptBuilder.AppendLine("2. Performance enhancements, fixing potential issues, and ensuring all changes are production-ready.");
            promptBuilder.AppendLine("3. Modularization and improving code quality.");

            return promptBuilder.ToString();
        }
        public string BuildErrorPrompt(string className, string originalClassCode, int errorLine)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Error Report for Class: '{className}'");
            promptBuilder.AppendLine("The following method caused an error during validation:");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Original Class Code:");
            promptBuilder.AppendLine(originalClassCode);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"Error occurred at line {errorLine}.");
            promptBuilder.AppendLine("Please fix the error and ensure the class is optimized and fully functional.");

            return promptBuilder.ToString();
        }

    }
}

