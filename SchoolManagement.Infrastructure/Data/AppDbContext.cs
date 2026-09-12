using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext)
                        .GetMethod(nameof(ApplySoftDeleteFilter),
                                   System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { builder });
                }
            }
        }

        private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder)
            where TEntity : class, ISoftDeletable
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }

        /// Use this when Admin wants to see deleted records
        public IQueryable<T> GetAllIncludingDeleted<T>() where T : class, ISoftDeletable
            => Set<T>().IgnoreQueryFilters();

        /// Use this when Admin wants only deleted records
        public IQueryable<T> GetOnlyDeleted<T>() where T : class, ISoftDeletable
            => Set<T>().IgnoreQueryFilters().Where(e => e.IsDeleted);
    }
}
