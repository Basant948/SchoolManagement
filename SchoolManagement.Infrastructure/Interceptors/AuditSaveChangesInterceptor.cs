using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SchoolManagement.Application.Interfaces.Services;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;

namespace SchoolManagement.Infrastructure.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.Entity is ISoftDeletable)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = now;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            string action = string.Empty;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is IAuditableEntity addedAuditable)
                    {
                        addedAuditable.CreatedAtUtc = now;
                        addedAuditable.CreatedBy = userId;
                    }
                    entry.Entity.IsDeleted = false;
                    action = "Created";
                    break;

                case EntityState.Modified:
                    if (entry.Entity.IsDeleted && entry.Property(nameof(ISoftDeletable.IsDeleted)).IsModified)
                    {
                        entry.Entity.DeletedAtUtc = now;
                        entry.Entity.DeletedBy = userId;
                        action = "Deleted";
                    }
                    else if (!entry.Entity.IsDeleted && entry.Property(nameof(ISoftDeletable.IsDeleted)).IsModified)
                    {
                        // Restored
                        entry.Entity.DeletedAtUtc = null;
                        entry.Entity.DeletedBy = null;
                        action = "Restored";
                    }
                    else
                    {
                        if (entry.Entity is IAuditableEntity modifiedAuditable)
                        {
                            modifiedAuditable.UpdatedAtUtc = now;
                            modifiedAuditable.UpdatedBy = userId;
                        }
                        action = "Updated";
                    }
                    break;

                case EntityState.Deleted:
                    // Convert to soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAtUtc = now;
                    entry.Entity.DeletedBy = userId;
                    action = "Deleted";
                    break;
            }

            if (!string.IsNullOrEmpty(action))
            {
                context.Set<AuditLog>().Add(new AuditLog
                {
                    EntityName = entry.Entity.GetType().Name,
                    EntityId = GetEntityId(entry.Entity),
                    Action = action,
                    PerformedBy = userId,
                    PerformedAtUtc = now
                });
            }
        }
    }

    private static string GetEntityId(object entity)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        var idValue = idProperty?.GetValue(entity);
        return idValue?.ToString() ?? string.Empty;
    }
}