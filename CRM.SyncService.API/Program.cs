using CRM.SyncService.API;
using Newtonsoft.Json;
using RestSharp;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CRM Sync API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// --- Minimal API Endpoint ---
app.MapPost("/crm/sync", async () =>
{
    var charmBase = builder.Configuration["CharmContact:BaseUrl"];
    //var apiToken = builder.Configuration["CharmContact:ApiToken"] ?? "Bearer_test_token";


    if (string.IsNullOrEmpty(charmBase))
        return Results.Problem("Charm BaseUrl missing in configuration.");

    try
    {
        var client = new HttpClient();

        // Gửi header Authorization tới ClickUp
        //if (!string.IsNullOrEmpty(apiToken))
            //client.DefaultRequestHeaders.Add("Authorization", apiToken);

        client.DefaultRequestHeaders.Add("Accept", "application/json");
        //client.DefaultRequestHeaders.Add("Authorization", "Bearer test_token_123");


        // Lấy contact từ Charm.Contact
        var response = await client.GetAsync($"{charmBase}/api/ContactsApi");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var contacts = JsonConvert.DeserializeObject<List<ContactDto>>(json);

        if (contacts == null || contacts.Count == 0)
            return Results.Ok(new { status = "no contacts found" });

        foreach (var c in contacts)
        {
            // Push sang ClickUp
            await ClickUpService.PushContactAsync(c);

            // Cập nhật Google Sheet
            //await GoogleSheetService.UpdateContactAsync(c);

            // Log Telegram
            await TelegramService.SendLogAsync($" Sync thành công: {c.contactName}");
        }

        return Results.Ok(new { status = "success", total = contacts.Count });
    }
    catch (Exception ex)
    {
        await TelegramService.SendLogAsync($"❌ Sync lỗi: {ex.Message}");
        return Results.Problem(ex.Message);
    }
})
.WithName("SyncContacts");
//.WithOpenApi();

app.Run();
