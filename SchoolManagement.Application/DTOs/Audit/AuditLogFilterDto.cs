namespace SchoolManagement.Application.DTOs.Audit;

public class AuditLogFilterDto
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}