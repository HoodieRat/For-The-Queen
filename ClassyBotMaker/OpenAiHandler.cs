using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassyBotMaker
{
    public class OpenAiHandler
    {
        private readonly string openAiApiKey;
        private readonly string OpenAIEndpoint = "https://api.openai.com/v1/chat/completions";
        private readonly int _maxRetryCount;
        private readonly HttpClient _httpClient;

        // Constructor initializes the API key internally and HttpClient
        public OpenAiHandler(int maxRetryCount = 3)
        {
            // Set the API key directly in the handler
            openAiApiKey = "your-api-key"; // Replace with your actual key
            _maxRetryCount = maxRetryCount;
            _httpClient = new HttpClient();
        }

        // Method to send requests to the OpenAI API
        public async Task<string> SendOpenAiApiRequestAsync(string prompt, string systemMessage, int maxTokens = 6000, float temperature = 0.0f)
        {
            if (string.IsNullOrEmpty(openAiApiKey))
            {
                Console.WriteLine("OpenAI API key not found.");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

            var messages = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "role", "system" }, { "content", systemMessage } },
                new Dictionary<string, string> { { "role", "user" }, { "content", prompt } }
            };

            var requestBody = new
            {
                model = "gpt-4",
                messages = messages,
                max_tokens = maxTokens,
                temperature = temperature,
                top_p = 1.0,
                frequency_penalty = 0.2,
                presence_penalty = 0.0
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(OpenAIEndpoint, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);

                if (jsonResponse != null && jsonResponse.choices != null && jsonResponse.choices.Count > 0)
                {
                    return jsonResponse.choices[0].message.content.ToString().Trim();
                }
                else
                {
                    Console.WriteLine("No responses returned from OpenAI API.");
                    return null;
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Error: {httpEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching OpenAI response: {ex.Message}");
                return null;
            }
        }

        public async Task<string> RetryOpenAiRequestAsync(string prompt, string systemMessage)
        {
            string response = null;

            for (int attempt = 1; attempt <= _maxRetryCount; attempt++)
            {
                response = await SendOpenAiApiRequestAsync(prompt, systemMessage);
                if (!string.IsNullOrEmpty(response))
                {
                    return response;
                }

                Console.WriteLine($"Attempt {attempt} failed to get a response from AI. Retrying...");
            }

            return response;
        }

        // Implementation of MakeApiRequest to interact with OpenAI API
        private async Task<string> MakeApiRequest(string prompt, string systemMessage)
        {
            var requestUri = "https://api.openai.com/v1/completions";
            var requestBody = new
            {
                model = "text-davinci-003",
                prompt = $"{systemMessage}\n{prompt}",
                max_tokens = 1000,
                temperature = 0.7
            };

            var jsonBody = JsonConvert.SerializeObject(requestBody);
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(requestUri, httpContent);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonConvert.DeserializeObject<OpenAiResponse>(responseBody);

                if (openAiResponse != null && openAiResponse.Choices.Count > 0)
                {
                    return openAiResponse.Choices[0].Text.Trim();
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
            }

            return null; // Return null in case of failure
        }
    }

    // Response model for OpenAI API
    public class OpenAiResponse
    {
        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
