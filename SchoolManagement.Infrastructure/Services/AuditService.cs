using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.DTOs.Audit;
using SchoolManagement.Application.Interfaces.Services;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public AuditService(AppDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(string entityName, string entityId, string action, string? additionalInfo = null)
    {
        var log = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            PerformedBy = _currentUserService.UserId,
            PerformedAtUtc = DateTime.UtcNow,
            AdditionalInfo = additionalInfo
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<(List<AuditLogDto> Items, int TotalCount)> GetLogsAsync(AuditLogFilterDto filter)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
            query = query.Where(x => x.EntityName == filter.EntityName);

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(x => x.Action == filter.Action);

        if (!string.IsNullOrWhiteSpace(filter.PerformedBy))
            query = query.Where(x => x.PerformedBy == filter.PerformedBy);

        if (filter.FromDate.HasValue)
            query = query.Where(x => x.PerformedAtUtc >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(x => x.PerformedAtUtc <= filter.ToDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.PerformedAtUtc)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Action = x.Action,
                PerformedBy = x.PerformedBy,
                PerformedByName = x.PerformedByName,
                PerformedAtUtc = x.PerformedAtUtc,
                AdditionalInfo = x.AdditionalInfo
            })
            .ToListAsync();

        return (items, totalCount);
    }
}