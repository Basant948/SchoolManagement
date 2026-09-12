using SchoolManagement.Application.DTOs.Audit;

namespace SchoolManagement.Application.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(string entityName, string entityId, string action, string? additionalInfo = null);
    Task<(List<AuditLogDto> Items, int TotalCount)> GetLogsAsync(AuditLogFilterDto filter);
}