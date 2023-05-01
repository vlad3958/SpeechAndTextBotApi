using System.Text;
using Newtonsoft.Json;

namespace Kursova;

public class TextToImageClient
{
    //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
    public async Task<string> Image(string prompt)
    {
        // Your API key from the OpenAI dashboard
        var apiKey = "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";

        // The DALL-E API endpoint
        var apiUrl = "https://api.openai.com/v1/images/generations";

        // The text to generate an image for
      //  var prompt = "a happy little tree in a meadow";

        // Create an HttpClient instance
        using (var client = new HttpClient())
        {
            // Set the API key in the Authorization header
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            // Create a JSON request body with the prompt
            var requestBody = new
            {
                model = "image-alpha-001",
                prompt = prompt,
                num_images = 1,
                size = "512x512",
                response_format = "url"
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send the request and read the response
            var response = await client.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            try
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(responseContent);
                string url = jsonObj.data[0].url;
              //  Console.WriteLine($"Generated image URL: {url}");
                return url;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        return null;
    }
}