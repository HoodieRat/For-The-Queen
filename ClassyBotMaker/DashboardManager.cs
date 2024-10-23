using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassyBotMaker
{
    public class DashboardManager
    {
        private readonly string baseDirectory;
        private readonly LoggingManager loggingManager;

        public DashboardManager(string baseDirectory, LoggingManager loggingManager)
        {
            this.baseDirectory = baseDirectory;
            this.loggingManager = loggingManager;
        }
        private readonly string dashboardDirectory;
        private readonly string dashboardFilePath;

        public DashboardManager(string baseDirectory)
        {
            dashboardDirectory = Path.Combine(baseDirectory, "dashboard");
            Directory.CreateDirectory(dashboardDirectory);
            dashboardFilePath = Path.Combine(dashboardDirectory, "bot_dashboard.html");
        }

        // Method to update the dashboard with progress details for a given component
        public void UpdateDashboardWithProgress(string componentName, string progressDetails, string status = "info")
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"[Dashboard] Component: {componentName} - {progressDetails} (Status: {status.ToUpper()})");

            StringBuilder dashboardContent;

            if (File.Exists(dashboardFilePath))
            {
                // Read the existing dashboard content
                dashboardContent = new StringBuilder(File.ReadAllText(dashboardFilePath));

                // Find or create the status section
                int sectionIndex = dashboardContent.ToString().LastIndexOf($"<ul id=\"{status}\">");
                if (sectionIndex == -1)
                {
                    dashboardContent.AppendLine($"<h2>{status.ToUpper()} Updates</h2>");
                    dashboardContent.AppendLine($"<ul id=\"{status}\"></ul>");
                    sectionIndex = dashboardContent.ToString().LastIndexOf($"<ul id=\"{status}\">");
                }

                // Insert the new progress details
                int ulEndIndex = dashboardContent.ToString().IndexOf("</ul>", sectionIndex);
                if (ulEndIndex != -1)
                {
                    dashboardContent.Insert(ulEndIndex, $"<li><strong>{componentName}:</strong> {progressDetails} <em>({timestamp})</em></li>");
                }
            }
            else
            {
                // Create a new dashboard if it doesn't exist
                dashboardContent = CreateNewDashboardContent(componentName, progressDetails, status, timestamp);
            }

            // Save the updated dashboard content
            File.WriteAllText(dashboardFilePath, dashboardContent.ToString());
        }

        // Method to log general info related to dashboard progress
        public void LogInfo(string message)
        {
            Console.WriteLine($"[Dashboard - INFO] {message}");
        }

        // Method to log errors to the dashboard for better error visibility
        public void LogError(string message)
        {
            Console.WriteLine($"[Dashboard - ERROR] {message}");
        }

        // Method to clear the dashboard information before starting a new process
        public void ClearDashboard()
        {
            Console.WriteLine("[Dashboard] Clearing dashboard for a new operation...");
            if (File.Exists(dashboardFilePath))
            {
                File.Delete(dashboardFilePath);
            }
        }

        // Method to generate a comprehensive bot dashboard
        public void GenerateBotDashboard(List<string> botFiles)
        {
            var dashboardContent = new StringBuilder();
            dashboardContent.AppendLine("<html><head><title>Bot Dashboard</title></head><body>");
            dashboardContent.AppendLine("<h1>Bot Dashboard</h1>");
            dashboardContent.AppendLine("<ul>");

            foreach (var botFile in botFiles)
            {
                string botName = Path.GetFileNameWithoutExtension(botFile);
                dashboardContent.AppendLine($"<li><strong>{botName}</strong>: <a href=\"{botName}_stats.html\">View Stats</a></li>");

                // Generate individual bot stats file
                GenerateBotStats(botName);
            }

            dashboardContent.AppendLine("</ul></body></html>");
            File.WriteAllText(dashboardFilePath, dashboardContent.ToString());
            Console.WriteLine($"Dashboard generated at {dashboardFilePath}");
        }

        // Method to generate statistics for individual bots
        private void GenerateBotStats(string botName)
        {
            string botStatsPath = Path.Combine(dashboardDirectory, $"{botName}_stats.html");
            var botStatsContent = new StringBuilder();

            botStatsContent.AppendLine("<html><head><title>Bot Stats</title></head><body>");
            botStatsContent.AppendLine($"<h1>{botName} Statistics</h1>");
            botStatsContent.AppendLine("<p>Placeholder for bot stats...</p>");
            botStatsContent.AppendLine("<ul>");
            botStatsContent.AppendLine("<li>Learning Progress: <strong>70%</strong></li>");
            botStatsContent.AppendLine("<li>Inventory Status: <strong>Stone, Wood, Tools</strong></li>");
            botStatsContent.AppendLine("<li>Tasks Completed: <strong>15</strong></li>");
            botStatsContent.AppendLine("<li>Current Task: <strong>Mining Resources</strong></li>");
            botStatsContent.AppendLine("</ul>");
            botStatsContent.AppendLine("</body></html>");

            File.WriteAllText(botStatsPath, botStatsContent.ToString());
        }

        // Method to create a new dashboard content when it doesn't exist
        private StringBuilder CreateNewDashboardContent(string componentName, string progressDetails, string status, string timestamp)
        {
            var dashboardContent = new StringBuilder();
            dashboardContent.AppendLine("<html><head><title>Bot Dashboard</title></head><body>");
            dashboardContent.AppendLine("<h1>Bot Dashboard</h1>");
            dashboardContent.AppendLine($"<h2>{status.ToUpper()} Updates</h2>");
            dashboardContent.AppendLine($"<ul id=\"{status}\">");
            dashboardContent.AppendLine($"<li><strong>{componentName}:</strong> {progressDetails} <em>({timestamp})</em></li>");
            dashboardContent.AppendLine("</ul>");
            dashboardContent.AppendLine("</body></html>");

            return dashboardContent;
        }

        // Method to generate a summary report
        public void GenerateSummaryReport(string summary)
        {
            Console.WriteLine($"[Dashboard - Summary Report] {summary}");
            UpdateDashboardWithProgress("Summary Report", summary, "summary");
        }

        // Method to clean up old bot statistics files
        public void CleanupOldBotStats(int daysToKeep = 7)
        {
            var logFiles = Directory.GetFiles(dashboardDirectory, "*_stats.html");

            foreach (var file in logFiles)
            {
                if (File.GetCreationTime(file) < DateTime.Now.AddDays(-daysToKeep))
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"Old bot stats file deleted: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete old bot stats file '{file}': {ex.Message}");
                    }
                }
            }
        }
    }
}
