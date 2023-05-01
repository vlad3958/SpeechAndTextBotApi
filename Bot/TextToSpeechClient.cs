
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Kursova;

public class TextToSpeechClient
{

    public async Task SynthesizeAudioAsync(string text)
    {
        try
        {
            var speechConfig = SpeechConfig.FromSubscription("6bc60295e8494ad3bd6f6b867ea7ea5a", "eastus");
            using var audioConfig =  
                AudioConfig.FromWavFileOutput(@"C:\Users\Влад\RiderProjects\Kursova\Bot\Voices\synthesizedVoice.wav");
            using var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
            await speechSynthesizer.SpeakTextAsync(text);
   
        //  speechSynthesizer.Dispose();
        //  audioConfig.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
          
        }
    }
}