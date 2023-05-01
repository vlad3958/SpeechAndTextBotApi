
using Microsoft.CognitiveServices.Speech;

using Microsoft.CognitiveServices.Speech.Audio;

namespace Kursova;

public class SpeechToTextClient
{

    public async  Task<string>  Main()
    { 
       
     string subscriptionKey = "1d6b608944a94eef9aff409700990687";
     string region = "eastus"; // e.g. "eastus"
  
     string outputFilePath = @"C:\Users\Влад\RiderProjects\Kursova\Bot\Voices\newVoice.wav";
    
     SpeechConfig speechConfig = SpeechConfig.FromSubscription("1d6b608944a94eef9aff409700990687", "eastus");
     speechConfig.SpeechRecognitionLanguage = "uk-UA";
    
     AudioConfig audioConfig = AudioConfig.FromWavFileInput(outputFilePath);

     SpeechRecognizer recognizer = new SpeechRecognizer(speechConfig,audioConfig);
     SpeechRecognitionResult result = await recognizer.RecognizeOnceAsync();
     if (result.Reason == ResultReason.RecognizedSpeech)
     {
       Console.WriteLine($"Transcription: {result.Text}");
       recognizer.Dispose();
       audioConfig.Dispose();
        return result.Text;
     }
     else if (result.Reason == ResultReason.NoMatch)
     {
         Console.WriteLine($"Speech could not be recognized.");
     }
     
     return null;
    }
}