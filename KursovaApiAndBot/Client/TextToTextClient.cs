using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kursova;

public class TextToTextClient
{
    public async Task<string> Main(string textToTranslate)
    {
        if (textToTranslate==null)
            return null;
        
        string apiKey = "1d6b608944a94eef9aff409700990687";
        string endpoint = "https://api.cognitive.microsofttranslator.com";
        string targetLanguage = "en";

       
        string route = "/translate?api-version=3.0&from=uk&to=en";
      
        object[] body = new object[] { new { Text = textToTranslate } };
        var requestBody = JsonConvert.SerializeObject(body);

        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage())
        {
            // Build the request.
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            // location required if you're using a multi-service or regional (not global) resource.
            request.Headers.Add("Ocp-Apim-Subscription-Region", "eastus");

            // Send the request and get response.
            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            // Read response as a string.
            string result = await response.Content.ReadAsStringAsync();
           // Console.WriteLine(result);
           
            JArray jArray = JArray.Parse( result);
            string text = jArray[0]["translations"][0]["text"].ToString();
            return text;
        }
    }
}