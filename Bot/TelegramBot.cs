
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Concentus.Oggfile;
using Concentus.Structs;
using Kursova;
using MongoDB.Driver;
using NAudio.Lame;
using NAudio.Wave;
using Newtonsoft.Json;
namespace Bot;

public class TelegramBot
{
    public static string connectionString = "mongodb+srv://berezinv7930:root@cluster0.yklzpio.mongodb.net/?retryWrites=true&w=majority";
    public static string databaseName = "speechAndTextBot";
    public static string collectionName = "BotData";
    public static string RecognizedText { get; set; }
    public static string TranslatedText { get; set; }
    public static string ImageUrl { get; set; }
    public static string Null = "null";
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
            
           

            if (message.Type == MessageType.Voice)
            {

                await botClient.SendTextMessageAsync(message.Chat.Id, "Processing...");
                var fileId = update.Message.Voice.FileId;
              
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;


                    const string destinationFolder = @"C:\Users\Влад\RiderProjects\Kursova\Bot\Voices";
                    var fileExtension = filePath.Split('.').Last();
                    var destinationFilePath = $"{destinationFolder}\\newVoice.{fileExtension}";
                    
                try
                {
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
                        await botClient.SendTextMessageAsync(message.Chat.Id, RecognizedText);
                        using (StreamWriter streamWriter =
                               new StreamWriter(@"C:\Users\Влад\RiderProjects\Kursova\Bot\Text\responce.txt"))
                        {
                            streamWriter.WriteLine(RecognizedText);
                        }

                     //   Console.WriteLine(RecognizedText);


                        var responce2 = await httpClient.GetAsync("/TextToText");
                        responce2.EnsureSuccessStatusCode();
                        TranslatedText = responce2.Content.ReadAsStringAsync().Result;
                        await botClient.SendTextMessageAsync(message.Chat.Id, TranslatedText);
                        using (StreamWriter streamWriter =
                               new StreamWriter(@"C:\Users\Влад\RiderProjects\Kursova\Bot\Text\responceEng.txt"))
                        {
                            streamWriter.WriteLine(TranslatedText);
                        }

                     //   Console.WriteLine(TranslatedText);



                        await botClient.SendTextMessageAsync(message.Chat.Id, "Processing image...");
                        var responce4 = await httpClient.GetAsync("/TextToImage");
                        responce4.EnsureSuccessStatusCode();
                        string imageUrl = responce4.Content.ReadAsStringAsync().Result;
                        if (imageUrl != "")
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, imageUrl);
                            ImageUrl = imageUrl;
                        }
                        else
                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                "Some issues happened while generating image. Try again");

                        
                        TextToSpeechClient toSpeech = new TextToSpeechClient(); 
                        await  toSpeech.SynthesizeAudioAsync(TranslatedText);
                        await ConvertWavToMp3(@"C:\Users\Влад\RiderProjects\Kursova\Bot\Voices\synthesizedVoice.wav",
                            @"C:\Users\Влад\RiderProjects\Kursova\Bot\Voices\synthesizedVoice.mp3");
                        
                        InputFile audioFile = new InputFile(new FileStream(
                            @"C:\Users\Влад\RiderProjects\Kursova\Bot\Voices\synthesizedVoice.mp3", FileMode.Open));
                        await botClient.SendAudioAsync(message.Chat.Id, audioFile);
                        
                        
                        
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "What do you you want to be saved in database? Write options in one message" +
                            "(ukrText, engText, image, username, all). To skip dont write anything");
                        

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
                    var text = message.Text;
                    switch (text.ToLower())
                            {
                                
                                case "all":
                                    if (ImageUrl != "")
                                    {
                                        var responce5 =
                                            await httpClient.PostAsync(
                                                $"/db/dbPost?ukrText={RecognizedText}&engText={TranslatedText}&image={ImageUrl}&username={message.Chat.Username}",
                                                null);
                                        responce5.EnsureSuccessStatusCode();
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                    }
                                    else
                                    {
                                        var responce5 =
                                            await httpClient.PostAsync(
                                                $"/db/dbPost?ukrText={RecognizedText}&engText={TranslatedText}&image={Null}&username={message.Chat.Username}",
                                                null);
                                        responce5.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                    }
                                    break;
                                case "ukrtext":   var responce6 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={RecognizedText}&engText={Null}&image={Null}&username={Null}",
                                            null);
                                    responce6.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                    break;
                                case "engtext": var responce7 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={TranslatedText}&image={Null}&username={Null}",
                                            null);
                                    responce7.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                    break;
                                case "image":
                                    if (ImageUrl != "")
                                    {
                                        var responce8 =
                                            await httpClient.PostAsync(
                                                $"/db/dbPost?ukrText={Null}&engText={Null}&image={ImageUrl}&username={Null}",
                                                null);
                                        responce8.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                    }
                                    else
                                    {
                                        var responce8 =
                                            await httpClient.PostAsync(
                                                $"/db/dbPost?ukrText={Null}&engText={Null}&image={Null}&username={Null}",
                                                null);
                                        responce8.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                    }

                                    break;
                                case "username":var responce9 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={Null}&image={Null}&username={message.Chat.Username}",
                                            null);
                                    responce9.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                    break;
                            }

                            if (text.ToLower().Contains("ukrtext") && text.ToLower().Contains("engtext"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={RecognizedText}&engText={TranslatedText}&image={Null}&username={Null}",
                                        null);
                                responce10.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                            }
                            if (text.ToLower().Contains("ukrtext") && text.ToLower().Contains("image"))
                            {
                                if (ImageUrl != "")
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={RecognizedText}&engText={Null}&image={ImageUrl}&username={Null}",
                                            null);
                                    responce10.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                }
                                else
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={RecognizedText}&engText={Null}&image={Null}&username={Null}",
                                            null);
                                    responce10.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                }
                            }
                            if (text.ToLower().Contains("ukrtext") && text.ToLower().Contains("username"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={RecognizedText}&engText={Null}&image={Null}&username={message.Chat.Username}",
                                        null);
                                responce10.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                            }
                            if (text.ToLower().Contains("engtext") && text.ToLower().Contains("image"))
                            {
                                if (ImageUrl != "")
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={TranslatedText}&image={ImageUrl}&username={Null}",
                                            null);
                                    responce10.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                }
                                else
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={TranslatedText}&image={Null}&username={Null}",
                                            null);
                                    responce10.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                }
                            }
                            if (text.ToLower().Contains("engtext") && text.ToLower().Contains("username"))
                            {
                                var responce10 =
                                    await httpClient.PostAsync(
                                        $"/db/dbPost?ukrText={Null}&engText={TranslatedText}&image={Null}&username={message.Chat.Username}",
                                        null);
                                responce10.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                            }
                            if (text.ToLower().Contains("image") && text.ToLower().Contains("username"))
                            {
                                if (ImageUrl != "")
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={Null}&image={ImageUrl}&username={message.Chat.Username}",
                                            null);
                                    responce10.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                }
                                else
                                {
                                    var responce10 =
                                        await httpClient.PostAsync(
                                            $"/db/dbPost?ukrText={Null}&engText={Null}&image={Null}&username={message.Chat.Username}",
                                            null);
                                    responce10.EnsureSuccessStatusCode();   await botClient.SendTextMessageAsync(message.Chat.Id, "Data was successfuly added in database");
                                }
                            }

                            var id = "644ec8ab603dab69388a5647";
                            
                            var responce = await httpClient.DeleteAsync($"/db/dbDelete?id={id}");
                            responce.EnsureSuccessStatusCode();
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
            using (var reader =  new WaveFileReader(wavFilePath))
            using (var writer =  new LameMP3FileWriter(mp3FilePath, reader.WaveFormat, LAMEPreset.STANDARD))
            {
              
                await reader.CopyToAsync(writer);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public async static Task Convert()
  {
      var filePath = $@"C:\Users\Влад\RiderProjects\Kursova\Bot\Voices\";
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