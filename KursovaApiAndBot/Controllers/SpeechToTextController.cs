

using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;

namespace Kursova.Controllers;

[ApiController]
[Route("[controller]")]
public class SpeechToTextController : ControllerBase
{
  
    private readonly ILogger<SpeechToTextController> _logger;

    public SpeechToTextController(ILogger<SpeechToTextController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet]
    public string SpeechToText()
    {
        SpeechToTextClient speechToTextClient = new SpeechToTextClient();
        return speechToTextClient.Main().Result;
    }
}