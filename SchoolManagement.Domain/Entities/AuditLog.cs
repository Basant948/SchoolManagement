namespace SchoolManagement.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string EntityName { get; set; } = string.Empty;  
    public string EntityId { get; set; } = string.Empty;     

    public string Action { get; set; } = string.Empty;       

    public string? PerformedBy { get; set; }                
    public string? PerformedByName { get; set; }             

    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;

    public string? AdditionalInfo { get; set; }

}