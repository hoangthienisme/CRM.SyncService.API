using CRM.SyncService.API;
using Newtonsoft.Json;
using RestSharp;
using System.Text;
using System.Text.RegularExpressions;

public static class ClickUpService
{
    private static string ApiToken = "pk_294795597_MWJJI41L9W1WK2CJFBBFRX6DYG5MTWY8";
    private static string ListId = "901812735003";

    private static string CustomPhoneId = "687bb217-8df3-4e70-8b2f-c2b43e323b9c";
    private static string CustomEmailId = "53c65b2d-ac3e-4682-990c-2e85d8d5b610";
    private static string CustomFullNameId = "83443682-89a4-4058-b022-fdda5a72247c";

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

    // Convert tên task sang ASCII hợp lệ để tránh lỗi
    private static string NormalizeTaskName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "NoName";
        // Loại bỏ các ký tự đặc biệt
        var ascii = Regex.Replace(name.Normalize(NormalizationForm.FormKD), @"[^\u0000-\u007F]+", "");
        // Nếu rỗng sau khi loại bỏ ký tự, đặt tên mặc định
        return string.IsNullOrWhiteSpace(ascii) ? "ContactTask" : ascii;
    }

    public static async Task PushContactAsync(ContactDto contact)
    {
        var client = new RestClient("https://api.clickup.com/api/v2");
        var taskName = NormalizeTaskName(contact.Contact_Name);

        var body = new
        {
            name = taskName,
            custom_fields = new[]
            {
                new { id = CustomPhoneId, value = FormatPhone(contact.Phone) },
                new { id = CustomEmailId, value = contact.Email },
                new { id = CustomFullNameId, value = contact.Contact_Name } 
            }
        };

        var request = new RestRequest($"/list/{ListId}/task", Method.Post)
            .AddHeader("Authorization", ApiToken)
            .AddHeader("Content-Type", "application/json")
            .AddStringBody(JsonConvert.SerializeObject(body), DataFormat.Json);

        var response = await client.ExecuteAsync(request);

        if (!response.IsSuccessful)
            throw new Exception($"ClickUp API error (StatusCode: {response.StatusCode}): {response.Content}");
    }
}
