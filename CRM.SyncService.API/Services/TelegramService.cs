using RestSharp;

public static class TelegramService
{
    private static string BotToken = "YOUR_BOT_TOKEN";
    private static string ChatId = "YOUR_CHAT_ID";

    public static async Task SendLogAsync(string message)
    {
        var client = new RestClient($"https://api.telegram.org/bot{BotToken}/");

        var request = new RestRequest("sendMessage", Method.Post);
        request.AddParameter("chat_id", ChatId);
        request.AddParameter("text", message);

        var response = await client.ExecuteAsync(request);
        if (!response.IsSuccessful)
            throw new Exception($"Telegram API error: {response.Content}");
    }
}
