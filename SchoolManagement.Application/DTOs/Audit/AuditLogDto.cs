namespace SchoolManagement.Application.DTOs.Audit;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? PerformedBy { get; set; }
    public string? PerformedByName { get; set; }
    public DateTime PerformedAtUtc { get; set; }
    public string? AdditionalInfo { get; set; }
}