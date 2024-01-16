using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CalculatorClient
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly Channel<string> requestChannel = Channel.CreateUnbounded<string>();

        static async Task Main(string[] args)
        {
            var simpleCalculateUrl = "http://localhost:5281/api/calculator";
            var complexCalculateUrl = "http://localhost:5281/api/calculator/ComplexExpression";

            var userInputTask = Task.Run(async () =>
            {
                while (true)
                {
                    string expression = Console.ReadLine();
                    await requestChannel.Writer.WriteAsync(expression);
                }
            });

            var requestHandlerTask = Task.Run(async () =>
            {
                await foreach (var expression in requestChannel.Reader.ReadAllAsync())
                {
                    string url = (expression[0] == 'c') ? complexCalculateUrl : simpleCalculateUrl;
                    if (expression[0] == 'c')
                    {
                        var result = PostExpressionAsync(url, expression[1..], true);
                    }
                    else
                    {
                        var result = PostExpressionAsync(url, expression, false);
                        
                    }
                }
            });

            await Task.WhenAll(userInputTask, requestHandlerTask);
        }

        static async Task<string> PostExpressionAsync(String apiUrl, String expression, bool isComplex)
        {
            using (var client = new HttpClient())
            {
                var payload = new StringContent($"{{\"expression\": \"{expression}\"}}",
                    Encoding.UTF8,
                    "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (isComplex)
                    {
                        Console.WriteLine($"The result of the previous complex calculation\n {expression} is: {result}");
                    }
                    else
                    {
                        Console.WriteLine($"The result of {expression} is: {result}");
                    }
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Status code: {response.StatusCode}, Error: {errorContent}");
                }
            }
        }
    }
}