# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files first
COPY ["SchoolManagement.Web/SchoolManagement.Web.csproj", "SchoolManagement.Web/"]
COPY ["SchoolManagement.Application/SchoolManagement.Application.csproj", "SchoolManagement.Application/"]
COPY ["SchoolManagement.Domain/SchoolManagement.Domain.csproj", "SchoolManagement.Domain/"]
COPY ["SchoolManagement.Infrastructure/SchoolManagement.Infrastructure.csproj", "SchoolManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SchoolManagement.Web/SchoolManagement.Web.csproj"

# Copy everything else
COPY . .

# Build/publish Web project
WORKDIR "/src/SchoolManagement.Web"

RUN dotnet publish "SchoolManagement.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore


# =========================
# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Render provides the PORT environment variable
ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "SchoolManagement.Web.dll"]