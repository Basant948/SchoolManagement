using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Resend;
using SchoolManagement.Application.Interfaces.Services;
using SchoolManagement.Application.Interfaces.Utilities;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Identity;
using SchoolManagement.Infrastructure.Interceptors;
using SchoolManagement.Infrastructure.Services;
using System;
using System.Net.Http.Headers;
using System.Text;

namespace SchoolManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public const string CorsPolicyName =
            "SchoolManagementCorsPolicy";

        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // =========================================================
            // Database
            // =========================================================

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("NeonDb")
                    ?? throw new InvalidOperationException(
                        "Connection string 'NeonDb' not found.")
                );

                options.AddInterceptors(
                    sp.GetRequiredService<AuditSaveChangesInterceptor>());
            });


            // =========================================================
            // Identity
            // =========================================================

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;

                options.User.RequireUniqueEmail = false;

                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);

                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();


            // =========================================================
            // Identity / Application Services
            // =========================================================

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<ITokenService, TokenService>();


            // =========================================================
            // JWT Authentication
            // =========================================================

            services.AddJwtAuthentication(configuration);


            // =========================================================
            // Resend Email
            // =========================================================

            services.AddOptions();

            services.Configure<ResendClientOptions>(options =>
            {
                options.ApiToken =
                    configuration["Resend:ApiKey"]
                    ?? throw new InvalidOperationException(
                        "Resend:ApiKey is missing.");
            });

            services.AddHttpClient<ResendClient>();

            services.AddTransient<IResend, ResendClient>();

            services.AddScoped<IEmailService, ResendEmailService>();


            // =========================================================
            // HTTP Context / Current User
            // =========================================================

            services.AddHttpContextAccessor();

            services.AddScoped<ICurrentUserService, CurrentUserService>();


            // =========================================================
            // Audit
            // =========================================================

            services.AddScoped<AuditSaveChangesInterceptor>();
            services.AddScoped<IAuditService, AuditService>();


            // =========================================================
            // CORS
            // =========================================================

            services.AddCors(options =>
            {
                var allowedOrigins =
                    configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>()
                    ?? Array.Empty<string>();

                options.AddPolicy(
                    CorsPolicyName,
                    policy =>
                    {
                        if (allowedOrigins.Length > 0)
                        {
                            policy
                                .WithOrigins(allowedOrigins)
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                        }
                        else
                        {
                            policy
                                .AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        }
                    });
            });


            return services;
        }


        // =============================================================
        // JWT Authentication
        // =============================================================

        private static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");

            var key =
                jwtSection["Key"]
                ?? throw new InvalidOperationException(
                    "Jwt:Key is not configured.");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(key)),

                        ClockSkew = TimeSpan.Zero
                    };
            });

            services.AddAuthorization();

            return services;
        }
    }
}