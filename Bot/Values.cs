using Telegram.Bot.Types;

namespace Bot;

public class Values
{
    public static string TranslatedText { get; set; }
    public static string RecognizedText { get; set; }
    public static string Responce { get; set; }

    public static string address = "https://localhost:7226";
    public static InputFile TranslatedVoice { get; set; }
}