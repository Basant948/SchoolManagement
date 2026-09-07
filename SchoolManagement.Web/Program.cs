using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SchoolManagement.Infrastructure;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Web.Middleware;

const string BearerScheme = "Bearer";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SchoolManagement API",
        Version = "v1"
    });

    options.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste ONLY the raw JWT here - do not include the word 'Bearer'. Swagger adds that prefix for you.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(BearerScheme, document)] = []
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseGlobalExceptionHandling();
app.UseRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();