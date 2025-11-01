using CRM.SyncService.API;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Swagger cấu hình ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM Sync API",
        Version = "v1",
        Description = "API đồng bộ dữ liệu từ Charm CRM sang ClickUp và Google Sheet"
    });
});

var app = builder.Build();

// --- Swagger UI ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM Sync API v1");
    c.RoutePrefix = string.Empty;
});

app.MapPost("/crm/sync", async () =>
{
    var charmBase = builder.Configuration["CharmContact:BaseUrl"];
    var charmEmail = builder.Configuration["CharmContact:Email"];
    var charmPassword = builder.Configuration["CharmContact:Password"];

    if (string.IsNullOrEmpty(charmBase))
        return Results.Problem("⚠️ Missing Charm BaseUrl in configuration.");

    try
    {
        var client = new HttpClient();

        // === 1️⃣ Đăng nhập lấy token ===
        var loginData = new
        {
            email = charmEmail,
            password = charmPassword
        };

        var loginContent = new StringContent(
            JsonConvert.SerializeObject(loginData),
            Encoding.UTF8,
            "application/json"
        );

        // ⚠️ DÙNG /api/login — KHÔNG PHẢI /Account/Login
        var loginUrl = $"{charmBase}/api/login";
        var loginResponse = await client.PostAsync(loginUrl, loginContent);

        var loginBody = await loginResponse.Content.ReadAsStringAsync();

        if (!loginResponse.IsSuccessStatusCode)
        {
            await TelegramService.SendLogAsync($"❌ Login thất bại ({loginResponse.StatusCode}). Body: {loginBody}");
            return Results.Problem($"Login failed: {loginResponse.StatusCode}, body={loginBody}");
        }

        // === 2️⃣ Lấy token ===
        dynamic? loginResult = JsonConvert.DeserializeObject(loginBody);
        string? token = loginResult?.token;

        if (string.IsNullOrEmpty(token))
            return Results.Problem("Không lấy được token từ CRM.");

        // === 3️⃣ Gọi Contacts API ===
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        var response = await client.GetAsync($"{charmBase}/api/ContactsApi");
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            await TelegramService.SendLogAsync($"❌ ContactsApi lỗi: {response.StatusCode}, body={json}");
            return Results.Problem($"ContactsApi lỗi: {response.StatusCode}, body={json}");
        }

        var contacts = JsonConvert.DeserializeObject<List<ContactDto>>(json);

        if (contacts == null || contacts.Count == 0)
            return Results.Ok(new { status = "no contacts found" });

        // === 4️⃣ Sync lên ClickUp + Google Sheet ===
        foreach (var c in contacts)
        {
            await ClickUpService.PushContactAsync(c);
            await GoogleSheetService.AppendContactWithHeaderAsync(c);
            await TelegramService.SendLogAsync($"✅ Sync thành công: {c.contactName}");
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

app.Run();
