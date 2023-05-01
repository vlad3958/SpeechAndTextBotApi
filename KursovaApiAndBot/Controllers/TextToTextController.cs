
using Microsoft.AspNetCore.Mvc;
namespace Kursova.Controllers;

[ApiController]
[Route("[controller]")]
public class TextToTextController : ControllerBase
{

    private readonly ILogger<TextToTextController> _logger;

    public TextToTextController(ILogger<TextToTextController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "TextToText")]
    public string TextToText()
    {
        string fileContent;
        using (StreamReader streamReader =
               new StreamReader(@"C:\Users\Влад\RiderProjects\Kursova\Bot\Text\responce.txt"))
        {
            fileContent = streamReader.ReadToEnd();
        }
        TextToTextClient textClient = new TextToTextClient();
        return textClient.Main(fileContent).Result;
     
    }
}