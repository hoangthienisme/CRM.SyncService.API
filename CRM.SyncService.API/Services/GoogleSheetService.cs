//using CRM.SyncService.API;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Services;
//using Google.Apis.Sheets.v4;
//using Google.Apis.Sheets.v4.Data;

//public static class GoogleSheetService
//{
//    private static string SpreadsheetId = "YOUR_SPREADSHEET_ID";
//    private static SheetsService? _service;

//    private static SheetsService GetService()
//    {
//        if (_service != null) return _service;

//        var credential = GoogleCredential.FromFile("credentials.json")
//                                         .CreateScoped(SheetsService.Scope.Spreadsheets);

//        _service = new SheetsService(new BaseClientService.Initializer
//        {
//            HttpClientInitializer = credential,
//            ApplicationName = "CRM Sync API"
//        });

//        return _service;
//    }

//    public static async Task UpdateContactAsync(ContactDto contact)
//    {
//        try
//        {
//            var service = GetService();
//            var range = "Sheet1!A:E";
//            var valueRange = new ValueRange
//            {
//                Values = new List<IList<object>>
//                {
//                    new List<object> { contact.Contact_Name, contact.Phone, contact.Email, contact.Status, contact.Source }
//                }
//            };

//            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
//            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
//            await appendRequest.ExecuteAsync();
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Google Sheet append error: {ex.Message}");
//            throw;
//        }
//    }
//}
