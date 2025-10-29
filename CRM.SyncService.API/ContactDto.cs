namespace CRM.SyncService.API
{
    public record ContactDto(
        string contactName,
        string Phone,
        string Email,
        string Status,
        string Source
    );

}
