using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassyBotMaker
{
    public class ToDoListManager
    {
        private readonly string baseDirectory;
        private readonly string toDoListFilePath;
        private List<ToDoItem> toDoList;

        private readonly OpenAiHandler openAiHandler;
        private readonly DashboardManager dashboardManager;

        public ToDoListManager(string baseDirectory, OpenAiHandler openAiHandler, DashboardManager dashboardManager)
        {
            this.baseDirectory = baseDirectory;
            this.openAiHandler = openAiHandler;
            this.dashboardManager = dashboardManager;

            toDoListFilePath = Path.Combine(baseDirectory, "data", "todoList.json");
            LoadToDoList();
        }

        // Load to-do list from file
        private void LoadToDoList()
        {
            if (File.Exists(toDoListFilePath))
            {
                string json = File.ReadAllText(toDoListFilePath);
                toDoList = JsonConvert.DeserializeObject<List<ToDoItem>>(json) ?? new List<ToDoItem>();
            }
            else
            {
                toDoList = new List<ToDoItem>();
            }
        }

        // Save the to-do list back to file
        private void SaveToDoList()
        {
            File.WriteAllText(toDoListFilePath, JsonConvert.SerializeObject(toDoList, Formatting.Indented));
        }

        // Update the status of a specific task in the to-do list
        public void UpdateToDoList(string taskName, string status)
        {
            var toDoItem = toDoList.Find(t => t.Task.Equals(taskName, StringComparison.OrdinalIgnoreCase));
            if (toDoItem != null)
            {
                toDoItem.Status = status;
                SaveToDoList();
                dashboardManager.UpdateDashboardWithProgress(taskName, $"Task status updated to: {status}");
            }
            else
            {
                Console.WriteLine($"Task '{taskName}' not found in the to-do list.");
            }
        }

        // Retrieve all tasks
        public List<ToDoItem> GetAllTasks()
        {
            return new List<ToDoItem>(toDoList);
        }

        // Retrieve the next pending task
        public ToDoItem GetNextTask()
        {
            return toDoList.Find(t => t.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
        }

        // Send a task to AI and process the response
        public async Task ProcessNextTask()
        {
            var nextTask = GetNextTask();
            if (nextTask != null)
            {
                UpdateToDoList(nextTask.Task, "In Progress");
                bool taskCompleted = await SendTaskToAI(nextTask);

                if (taskCompleted)
                {
                    UpdateToDoList(nextTask.Task, "Completed");
                }
                else
                {
                    UpdateToDoList(nextTask.Task, "Failed");
                }
            }
            else
            {
                Console.WriteLine("No pending tasks available.");
            }
        }

        // Send task details to AI for processing
        public async Task<bool> SendTaskToAI(ToDoItem task)
        {
            Console.WriteLine($"Sending task to AI: {task.Task}");

            // Construct a detailed prompt
            string prompt = $"You are a top-tier C# programmer with expertise in AI-driven bot development. " +
                            $"Here is the next task:\n\n{task.Description}\n\n" +
                            $"Ensure that all code is fully implemented, functional, and efficient. " +
                            $"Provide production-quality updates without placeholders or incomplete methods. " +
                            $"If changes impact other parts of the project, please include necessary updates.";

            string systemMessage = "You are an expert C# developer specializing in production-quality code. Provide complete solutions without placeholders.";

            // Send request to OpenAI
            string updatedCode = await openAiHandler.SendOpenAiApiRequestAsync(prompt, systemMessage);

            if (!string.IsNullOrEmpty(updatedCode))
            {
                string classFilePath = Path.Combine(baseDirectory, $"{task.Task}.cs");

                try
                {
                    // Backup the existing file before updating
                    string backupFilePath = classFilePath.Replace(".cs", $"_backup_{DateTime.Now:yyyyMMddHHmmss}.cs");
                    File.Copy(classFilePath, backupFilePath, overwrite: true);

                    // Update the class with new code from AI
                    File.WriteAllText(classFilePath, updatedCode);
                    Console.WriteLine($"Successfully updated task: {task.Task}");

                    // Run validation tests after updating
                    bool validationPassed = RunValidationTests(task.Task);

                    if (validationPassed)
                    {
                        Console.WriteLine($"Validation successful for task: {task.Task}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Validation failed for task: {task.Task}. Reverting changes.");
                        File.Copy(backupFilePath, classFilePath, overwrite: true);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while processing task '{task.Task}': {ex.Message}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"Failed to get a response from AI for task: {task.Task}");
                return false;
            }
        }




     

        // Retrieve a specific task
        public ToDoItem GetTask(string taskName)
        {
            return toDoList.Find(t => t.Task.Equals(taskName, StringComparison.OrdinalIgnoreCase));
        }



        // Mark a task as in progress
        public void MarkTaskAsInProgress(string taskName)
        {
            UpdateToDoList(taskName, "In Progress");
        }

        // Mark a task as completed
        public void MarkTaskAsCompleted(string taskName)
        {
            UpdateToDoList(taskName, "Completed");
        }

        // Mark a task as failed
        public void MarkTaskAsFailed(string taskName)
        {
            UpdateToDoList(taskName, "Failed");
        }

  

        // Retrieve the next pending task

        // Run validation tests on the updated class
        private bool RunValidationTests(string className)
        {
            Console.WriteLine($"Running validation tests for class: {className}");

            try
            {
                // Define the path to the NUnit console runner and the assembly containing the unit tests.
                string nunitConsolePath = @"C:\NUnit\nunit3-console.exe"; // Adjust path to NUnit console runner accordingly.
                string testAssemblyPath = Path.Combine(baseDirectory, "ClassyBotMaker.Tests.dll"); // Assuming your tests are compiled in this assembly.

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
                    Console.WriteLine("Test Output:");
                    Console.WriteLine(output);

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Test Errors:");
                        Console.WriteLine(error);
                    }

                    // Determine the success of the tests based on the process exit code
                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"All tests passed for class: {className}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Some tests failed for class: {className}. Please review the errors.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while running validation tests for class {className}: {ex.Message}");
                return false;
            }
        }
    }

    // ToDoItem class to define the structure of a task in the to-do list
    public class ToDoItem
    {
        public string Task { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string ClassName { get; set; }  // New property
        public string ClassCode { get; set; }  // Add this property
    }
}
