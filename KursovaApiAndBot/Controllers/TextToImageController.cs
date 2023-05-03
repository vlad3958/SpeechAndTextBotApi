
using Microsoft.AspNetCore.Mvc;
namespace Kursova.Controllers;

[ApiController]
[Route("[controller]")]
public class TextToImageController : ControllerBase
{
    private readonly ILogger<TextToImageController> _logger;

    public TextToImageController(ILogger<TextToImageController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "TextToImage")]
    public string TextToImage()
    {
        string fileContent;
        using (StreamReader streamReader =
               new StreamReader(@"C:\Users\Влад\RiderProjects\KursovaApiAndBot\Bot\Text\responceEng.txt"))
        {
            fileContent = streamReader.ReadToEnd();
        }
        TextToImageClient toImage = new TextToImageClient();
         return toImage.Image(fileContent).Result;
    }
}