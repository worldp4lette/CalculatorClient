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
        static async Task Main(string[] args)
        {
            var simpleCalculateUrl = "http://localhost:5281/api/calculator";
            var complexCalculateUrl = "http://localhost:5281/api/calculator/ComplexExpression";

            while (true)
            {
                string expression = Console.ReadLine();
                string url;
                bool isComplex = (expression[0] == 'c');

                try
                {
                    if (isComplex)
                    {
                        url = complexCalculateUrl;
                        expression = expression[1..];
                    }
                    else
                    {
                        url = simpleCalculateUrl;
                    }
                    var result = await PostExpressionAsync(url, expression);
                    if (isComplex)
                    {
                        Console.WriteLine($"The result of the previous complex calculation\n {expression} is: {result}");
                    }
                    else
                    {
                        Console.WriteLine($"The result of {expression} is: {result}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

            }
        }

        static async Task<string> PostExpressionAsync(String apiUrl, String expression)
        {
            using (var client = new HttpClient())
            {
                var payload = new StringContent($"{{\"expression\": \"{expression}\"}}",
                    Encoding.UTF8,
                    "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
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