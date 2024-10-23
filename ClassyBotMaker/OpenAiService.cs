using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace ClassyBotMaker
{


        public class OpenAIService
        {
            private static readonly string OpenAIEndpoint = "https://api.openai.com/v1/chat/completions";
            private readonly string apiKey;

            public OpenAIService(string apiKey)
            {
                this.apiKey = apiKey;
            }

            // Sends a prompt to the OpenAI API
            public async Task<string> SendOpenAiApiRequestAsync(string prompt, string systemMessage, int maxTokens = 6000, float temperature = 0.0f)
            {
                if (string.IsNullOrEmpty(apiKey))
                {
                    Console.WriteLine("OpenAI API key not found.");
                    return null;
                }

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var messages = new[]
                    {
                    new { role = "system", content = systemMessage },
                    new { role = "user", content = prompt }
                };

                    var requestBody = new
                    {
                        model = "gpt-4",
                        messages = messages,
                        max_tokens = maxTokens,
                        temperature = temperature
                    };

                    var jsonContent = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(OpenAIEndpoint, content);
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);

                    return jsonResponse?.choices[0]?.message?.content?.ToString()?.Trim();
                }
            }
        }
    }