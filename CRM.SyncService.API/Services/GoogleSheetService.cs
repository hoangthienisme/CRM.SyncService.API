using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using CRM.SyncService.API;

public static class GoogleSheetService
{
    private static string SpreadsheetId = "1UC_DbJt0lQ65deIVLHqn8u37AfLoC3Inaus-eQ8Dbrg";
    private static SheetsService? _service;

    private static SheetsService GetService()
    {
        if (_service != null) return _service;

        var credential = GoogleCredential.FromFile("credentials.json")
                                         .CreateScoped(SheetsService.Scope.Spreadsheets);

        _service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "CRM Sync API"
        });

        return _service;
    }

    public static async Task AppendContactWithHeaderAsync(ContactDto contact)
    {
        var service = GetService();
        var sheetName = "Contact";

        // 1. Kiểm tra sheet tồn tại chưa
        var spreadsheet = await service.Spreadsheets.Get(SpreadsheetId).ExecuteAsync();
        if (!spreadsheet.Sheets.Any(s => s.Properties.Title == sheetName))
        {
            // Tạo sheet mới
            var addSheetRequest = new Request
            {
                AddSheet = new AddSheetRequest
                {
                    Properties = new SheetProperties { Title = sheetName }
                }
            };
            var batchUpdate = new BatchUpdateSpreadsheetRequest { Requests = new List<Request> { addSheetRequest } };
            await service.Spreadsheets.BatchUpdate(batchUpdate, SpreadsheetId).ExecuteAsync();
        }

        // 2. Kiểm tra sheet trống chưa
        var existingData = await service.Spreadsheets.Values.Get(SpreadsheetId, $"{sheetName}!A1:E1").ExecuteAsync();
        if (existingData.Values == null || existingData.Values.Count == 0)
        {
            // Thêm header
            var headerRange = $"{sheetName}!A1:E1";
            var headerValues = new ValueRange
            {
                Values = new List<IList<object>> { new List<object> { "Name", "Phone", "Email", "Status", "Source" } }
            };
            var appendHeaderRequest = service.Spreadsheets.Values.Append(headerValues, SpreadsheetId, headerRange);
            appendHeaderRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            await appendHeaderRequest.ExecuteAsync();
        }

        // 3. Append dữ liệu contact
        var dataRange = $"{sheetName}!A:E";
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>>
        {
            new List<object> { contact.contactName, contact.Phone, contact.Email, contact.Status, contact.Source }
        }
        };
        var appendDataRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, dataRange);
        appendDataRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
        await appendDataRequest.ExecuteAsync();
    }

}
