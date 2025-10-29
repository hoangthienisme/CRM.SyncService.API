using RestSharp;
using Newtonsoft.Json;
using CRM.SyncService.API;

public static class ClickUpService
{
    private static string ApiToken = "pk_294795597_MWJJI41L9W1WK2CJFBBFRX6DYG5MTWY8";

    private static string ListId = "901812735003";

    private static string CustomPhoneId = "687bb217-8df3-4e70-8b2f-c2b43e323b9c";
    private static string CustomEmailId = "53c65b2d-ac3e-4682-990c-2e85d8d5b610";
    private static string CustomStatusId = "def3ca78-0082-46b9-8525-409341eac624";
    private static string CustomSourceId = "abcff42b-f1cf-4f4f-8ed7-e9a37919d3de";
    public static string FormatPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;

        phone = phone.Trim(); 
        if (phone.StartsWith("0"))
            return "+84" + phone.Substring(1);
        if (phone.StartsWith("+"))
            return phone;
        return "+" + phone;
    }



    public static async Task PushContactAsync(ContactDto contact)
    {
        var client = new RestClient("https://api.clickup.com/api/v2");
        var request = new RestRequest($"/list/{ListId}/task", Method.Post);

        request.AddHeader("Authorization", ApiToken);
        request.AddHeader("Content-Type", "application/json");
        var phone = FormatPhone(contact.Phone);
        var body = new
        {
            name = contact.Contact_Name,
            custom_fields = new[]
    {
        new { id = CustomPhoneId, value = phone },
        new { id = CustomEmailId, value = contact.Email },
        //new { id = CustomStatusId, value = "option_uuid_lead" }, 
        //new { id = CustomSourceId, value = "option_uuid_affiliate" }
    }
        };


        request.AddStringBody(JsonConvert.SerializeObject(body), DataFormat.Json);

        var response = await client.ExecuteAsync(request);
        if (!response.IsSuccessful)
            throw new Exception($"ClickUp API error (StatusCode: {response.StatusCode}): {response.Content}");

    }

}
