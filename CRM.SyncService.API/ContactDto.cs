namespace CRM.SyncService.API
{
    public record ContactDto(
        string Contact_Name,
        string Phone,
        string Email,
        string Status,
        string Source
    );

}
