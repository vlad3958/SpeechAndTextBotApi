
using System.Diagnostics;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Concentus.Oggfile;
using Concentus.Structs;
using Kursova;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NAudio.Lame;
using NAudio.Wave;
using Newtonsoft.Json;
using File = System.IO.File;


namespace Bot;

public class TelegramBot
{
    public static string RecognizedText { get; set; }
    public static string TranslatedText { get; set; }
    public static string ImageUrl { get; set; }
    public static string Null = "null";
    public static int Kostil= 0;
  
   public static void Main()
    {
        var client =  new TelegramBotClient("5901078154:AAGNK4zeF5imBfVrKOld5he_GWx8N06qqzw");
        
        client.StartReceiving(Update1, Error);
        Console.ReadLine();
    }

    public static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        throw new NotImplementedException();
    }

    public async static Task Update1(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        
        var message = update.Message;
        if (message != null)
        {
            Kostil++;
            
            if (message.Type == MessageType.Voice)
            {
                try
                {
                    
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Processing...");
                var fileId = update.Message.Voice.FileId;
              
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;


                    const string destinationFolder = @"C:\Users\Влад\RiderProjects\KursovaApiAndBot\Bot\Voices";
                    var fileExtension = filePath.Split('.').Last();
                    var destinationFilePath = $"{destinationFolder}\\newVoice.{fileExtension}";
                    
               
                        await using Stream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                        await botClient.DownloadFileAsync(
                            filePath: filePath,
                            destination: fileStream,
                            cancellationToken: token);
                        await fileStream.DisposeAsync();

                    
                    await Convert();

                    HttpClientHandler clientHandler = new HttpClientHandler();
                    clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                    HttpClient httpClient = new HttpClient(clientHandler);
                    httpClient.BaseAddress = new Uri(Values.address);
                    var responce = await httpClient.GetAsync("/SpeechToText");
                    responce.EnsureSuccessStatusCode();

                    RecognizedText = responce.Content.ReadAsStringAsync().Result;
                    if (RecognizedText != "")
                    {
                   //   await botClient.SendTextMessageAsync(message.Chat.Id, RecognizedText);
                        using (StreamWriter streamWriter =
                               new StreamWriter(@"C:\Users\Влад\RiderProjects\KursovaApiAndBot\Bot\Text\responce.txt"))
                        {
                            streamWriter.WriteLine(RecognizedText);
                        }
                        

                        var responce2 = await httpClient.GetAsync("/TextToText");
                        responce2.EnsureSuccessStatusCode();
                        TranslatedText = responce2.Content.ReadAsStringAsync().Result;
                     //   await botClient.SendTextMessageAsync(message.Chat.Id, TranslatedText);
                        using (StreamWriter streamWriter =
                               new StreamWriter(@"C:\Users\Влад\RiderProjects\KursovaApiAndBot\Bot\Text\responceEng.txt"))
                        {
                            streamWriter.WriteLine(TranslatedText);
                        }


                        await botClient.SendTextMessageAsync(message.Chat.Id, "Processing image...");
                        var responce4 = await httpClient.GetAsync("/TextToImage");
                        responce4.EnsureSuccessStatusCode();
                      string imageUrl = responce4.Content.ReadAsStringAsync().Result;
                     if (imageUrl != "")
                    {
                       //   await botClient.SendTextMessageAsync(message.Chat.Id, imageUrl);
                          ImageUrl = imageUrl;
                      }
                       else
                          await botClient.SendTextMessageAsync(message.Chat.Id,
                             "Some issues happened while generating image. Try again");
                     
                       
                 await botClient.SendTextMessageAsync(message.Chat.Id,
                   "What do you you want to be shown? (Select commands in menu)");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Speech could not be recognized.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
         if (message.Type == MessageType.Text)
         {
             HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                
                HttpClient httpClient = new HttpClient(clientHandler);
                httpClient.BaseAddress = new Uri(Values.address);
                try
                {
                        var text1 = message.Text;
                        var text = text1.Replace(" ", String.Empty);

                        if (text.ToLower() == "/command1")
                        {
                            if (RecognizedText != null)
                                await botClient.SendTextMessageAsync(message.Chat.Id, RecognizedText);
                            else
                                await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                        }

                        if (text.ToLower() == "/command2")
                        {
                            if (TranslatedText != null)
                            await botClient.SendTextMessageAsync(message.Chat.Id, TranslatedText);
                            else
                                await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                        }

                        if (text.ToLower() == "/command3")
                        {
                            if(ImageUrl!=null)
                            await botClient.SendTextMessageAsync(message.Chat.Id, ImageUrl);
                            else
                                await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something...");return;
                        }

                        if (text.ToLower() == "/command4")
                        {
                            if (TranslatedText != null)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Processing...");
                                await SynthesizeAudioAsync(TranslatedText);
                                await ConvertWavToMp3(
                                    @$"C:\Users\Влад\RiderProjects\KursovaApiAndBot\Bot\Voices\synthesizedVoice{Kostil.ToString()}.wav",
                                    @$"C:\Users\Влад\RiderProjects\KursovaApiAndBot\Bot\Voices\synthesizedVoice{Kostil.ToString()}.mp3");
                                InputFile audioInput = new InputFile(new FileStream(
                                    $@"C:\Users\Влад\RiderProjects\KursovaApiAndBot\Bot\Voices\synthesizedVoice{Kostil.ToString()}.mp3",
                                    FileMode.Open));

                                await botClient.SendAudioAsync(message.Chat.Id, audioInput);
                            }else
                                await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                        }

                        if (text.ToLower().Contains("/comm"))
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                "What do you you want to be saved in database? Write options in one message" +
                                "(ukrText, engText, image, username, all). To skip dont write anything");
                        }

                        if (text.ToLower() == ("ukrtextengtext") || text.ToLower() == ("engtextukrtext"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={RecognizedText}&engText={TranslatedText}&image={Null}&username={Null}",
                                        null);
                                if (responce10.StatusCode == HttpStatusCode.OK)
                                    await botClient.SendTextMessageAsync(message.Chat.Id,
                                    "Data was successfuly added in database");
                                else
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                             
                            }
                        if (text.ToLower() == ("ukrtextimage") || text.ToLower() == ("imageukrtext"))
                            {
                                if (ImageUrl != "")
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={RecognizedText}&engText={Null}&image={ImageUrl}&username={Null}",
                                            null);
                                    if (responce10.StatusCode == HttpStatusCode.OK)
                                        await botClient.SendTextMessageAsync(message.Chat.Id,
                                            "Data was successfuly added in database");
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                }
                                else
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={RecognizedText}&engText={Null}&image={Null}&username={Null}",
                                            null);
                                    if (responce10.StatusCode == HttpStatusCode.OK)
                                        await botClient.SendTextMessageAsync(message.Chat.Id,
                                            "Data was successfuly added in database");
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                }
                            }
                        if (text.ToLower() == ("ukrtextusername") || text.ToLower() == ("usernameukrtext"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={RecognizedText}&engText={Null}&image={Null}&username={message.Chat.Username}",
                                        null);
                                if (responce10.StatusCode == HttpStatusCode.OK)
                                    await botClient.SendTextMessageAsync(message.Chat.Id,
                                        "Data was successfuly added in database");
                                else
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                            }
                        if (text.ToLower() == ("engtextimage") || text.ToLower() == ("imageengtext"))
                            {
                                if (ImageUrl != "")
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={TranslatedText}&image={ImageUrl}&username={Null}",
                                            null);
                                    if (responce10.StatusCode == HttpStatusCode.OK)
                                        await botClient.SendTextMessageAsync(message.Chat.Id,
                                            "Data was successfuly added in database");
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                }
                                else
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={TranslatedText}&image={Null}&username={Null}",
                                            null);
                                    if (responce10.StatusCode == HttpStatusCode.OK)
                                        await botClient.SendTextMessageAsync(message.Chat.Id,
                                            "Data was successfuly added in database");
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                }
                            }
                        if (text.ToLower() == ("engtextusername") || text.ToLower() == ("usernameengtext"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={Null}&engText={TranslatedText}&image={Null}&username={message.Chat.Username}",
                                        null);
                                if (responce10.StatusCode == HttpStatusCode.OK)
                                    await botClient.SendTextMessageAsync(message.Chat.Id,
                                        "Data was successfuly added in database");
                                else
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                            }
                        if (text.ToLower() == ("imageusername") || text.ToLower() == ("usernameimage"))
                            {
                                if (ImageUrl != "")
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={Null}&image={ImageUrl}&username={message.Chat.Username}",
                                            null);
                                    if (responce10.StatusCode == HttpStatusCode.OK)
                                        await botClient.SendTextMessageAsync(message.Chat.Id,
                                            "Data was successfuly added in database");
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                }
                                else
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={Null}&image={Null}&username={message.Chat.Username}",
                                            null);
                                    if (responce10.StatusCode == HttpStatusCode.OK)
                                        await botClient.SendTextMessageAsync(message.Chat.Id,
                                            "Data was successfuly added in database");
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                }
                            }
                        if (text1.ToLower().Contains("ukrtext") && text1.ToLower().Contains("engtext") && text1.ToLower().Contains("image"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={RecognizedText}&engText={TranslatedText}&image={ImageUrl}&username={Null}",
                                        null);
                                if (responce10.StatusCode == HttpStatusCode.OK)
                                    await botClient.SendTextMessageAsync(message.Chat.Id,
                                        "Data was successfuly added in database");
                                else
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                            }
                        if (text1.ToLower().Contains("username") && text1.ToLower().Contains("engtext") && text1.ToLower().Contains("image"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={Null}&engText={TranslatedText}&image={ImageUrl}&username={message.Chat.Username}",
                                        null);
                                if (responce10.StatusCode == HttpStatusCode.OK)
                                    await botClient.SendTextMessageAsync(message.Chat.Id,
                                        "Data was successfuly added in database");
                                else
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                            }
                        if (text1.ToLower().Contains("ukrtext") && text1.ToLower().Contains("engtext") && text1.ToLower().Contains("username"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={RecognizedText}&engText={TranslatedText}&image={Null}&username={message.Chat.Username}",
                                        null);
                                if (responce10.StatusCode == HttpStatusCode.OK)
                                    await botClient.SendTextMessageAsync(message.Chat.Id,
                                        "Data was successfuly added in database");
                                else
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                            }
                        if (text1.ToLower().Contains("ukrtext") && text1.ToLower().Contains("image") && text1.ToLower().Contains("username"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={RecognizedText}&engText={Null}&image={ImageUrl}&username={message.Chat.Username}",
                                        null);
                                if (responce10.StatusCode == HttpStatusCode.OK)
                                    await botClient.SendTextMessageAsync(message.Chat.Id,
                                        "Data was successfuly added in database");
                                else
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                            }

                            switch (text.ToLower())
                            {

                                case "all":
                                    if (ImageUrl != "")
                                    {
                                        var responce5 =
                                            await httpClient.PostAsync(
                                                $"/db/dbPost?ukrText={RecognizedText}&engText={TranslatedText}&image={ImageUrl}&username={message.Chat.Username}",
                                                null);
                                        if (responce5.StatusCode == HttpStatusCode.OK)
                                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                "Data was successfuly added in database");
                                        else
                                            await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                    }
                                    else
                                    {
                                        var responce5 =
                                            await httpClient.PostAsync(
                                                $"/db/dbPost?ukrText={RecognizedText}&engText={TranslatedText}&image={Null}&username={message.Chat.Username}",
                                                null);
                                        if (responce5.StatusCode == HttpStatusCode.OK)
                                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                "Data was successfuly added in database");
                                        else
                                            await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                    }

                                    break;
                                case "ukrtext":
                                    var responce6 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={RecognizedText}&engText={Null}&image={Null}&username={Null}",
                                            null);
                                    if (responce6.StatusCode == HttpStatusCode.OK)
                                        await botClient.SendTextMessageAsync(message.Chat.Id,
                                            "Data was successfuly added in database");
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                    break;
                                case "engtext":
                                    var responce7 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={TranslatedText}&image={Null}&username={Null}",
                                            null);
                                    if (responce7.StatusCode == HttpStatusCode.OK)
                                        await botClient.SendTextMessageAsync(message.Chat.Id,
                                            "Data was successfuly added in database");
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                    break;
                                case "image":
                                    if (ImageUrl != "")
                                    {
                                        var responce8 =
                                            await httpClient.PostAsync(
                                                $"/db/dbPost?ukrText={Null}&engText={Null}&image={ImageUrl}&username={Null}",
                                                null);
                                        if (responce8.StatusCode == HttpStatusCode.OK)
                                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                "Data was successfuly added in database");
                                        else
                                            await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                    }
                                    else
                                    {
                                        var responce8 =
                                            await httpClient.PostAsync(
                                                $"/db/dbPost?ukrText={Null}&engText={Null}&image={Null}&username={Null}",
                                                null);
                                        if (responce8.StatusCode == HttpStatusCode.OK)
                                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                                "Data was successfuly added in database");
                                        else
                                            await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                    }

                                    break;
                                case "username":
                                    var responce9 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={Null}&image={Null}&username={message.Chat.Username}",
                                            null);
                                    if (responce9.StatusCode == HttpStatusCode.OK)
                                        await botClient.SendTextMessageAsync(message.Chat.Id,
                                            "Data was successfuly added in database");
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "First you need to record something..."); return;
                                    break;
                            }
                            
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }
    }

    public static async Task ConvertWavToMp3(string wavFilePath, string mp3FilePath)
    {
        try
        {
             using (var reader = new WaveFileReader(wavFilePath))
            {
                 using (var writer = new LameMP3FileWriter(mp3FilePath, reader.WaveFormat, LAMEPreset.STANDARD))
                {
                    reader.CopyTo(writer);
        
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public static async Task SynthesizeAudioAsync(string text)
    {
        try
        {
            var speechConfig = SpeechConfig.FromSubscription("6bc60295e8494ad3bd6f6b867ea7ea5a", "eastus");
            using var audioConfig =  
                AudioConfig.FromWavFileOutput(@$"C:\Users\Влад\RiderProjects\KursovaApiAndBot\Bot\Voices\synthesizedVoice{Kostil.ToString()}.wav");
            using var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
            await speechSynthesizer.SpeakTextAsync(text);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    
    public async static Task Convert()
  {
      var filePath = $@"C:\Users\Влад\RiderProjects\KursovaApiAndBot\Bot\Voices\";
      var fileOgg = "newVoice.oga";
      var fileWav = "newVoice.wav";

      using (FileStream fileIn = new FileStream($"{filePath}{fileOgg}", FileMode.Open))
      using (MemoryStream pcmStream = new MemoryStream())
      {
          OpusDecoder decoder = OpusDecoder.Create(48000, 1);
          OpusOggReadStream oggIn = new OpusOggReadStream(decoder, fileIn);
          while (oggIn.HasNextPacket)
          {
              short[] packet = oggIn.DecodeNextPacket();
              if (packet != null)
              {
                  for (int i = 0; i < packet.Length; i++)
                  {
                      var bytes = BitConverter.GetBytes(packet[i]);
                      pcmStream.Write(bytes, 0, bytes.Length);
                  }
              }
          }
          pcmStream.Position = 0;
          var wavStream = new RawSourceWaveStream(pcmStream, new WaveFormat(48000, 1));
          var sampleProvider = wavStream.ToSampleProvider(); 
          WaveFileWriter.CreateWaveFile16($"{filePath}{fileWav}", sampleProvider);
    
      }
     
  }
 
}