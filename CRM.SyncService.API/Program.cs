using CRM.SyncService.API;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/crm/sync", async (ContactDto contact) =>
{
    try
    {
        // 1. Push sang ClickUp
        await ClickUpService.PushContactAsync(contact);

        // 2. Update Google Sheet
        await GoogleSheetService.UpdateContactAsync(contact);

        // 3. Gửi log Telegram
        await TelegramService.SendLogAsync($" Sync thành công: {contact.Contact_Name}");

        return Results.Ok(new { status = "success" });
    }
    catch (Exception ex)
    {
        await TelegramService.SendLogAsync($" Sync lỗi: {ex.Message}");
        return Results.Problem(ex.Message);
    }
});
app.MapGet("/test-clickup", async () =>
{
    try
    {
        var contact = new ContactDto(
     Contact_Name: "Nguyen Van c",
     Phone: "0333715129", // đủ số
     Email: "c.nguyen@example.com",
     Status: "lead",
     Source: "Affiliate"
 );

        await ClickUpService.PushContactAsync(contact);

        return Results.Ok(new { status = "success", message = "Contact pushed to ClickUp!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
});

app.Run();
