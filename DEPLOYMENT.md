# Deployment Guide

## Production Deployment Checklist

### Prerequisites
- [ ] .NET 6.0 Runtime installed on target server
- [ ] SQL Server instance configured
- [ ] NewsAPI key obtained
- [ ] Domain/hosting configured
- [ ] SSL certificate ready

### Database Setup

1. **Create Production Database**
```sql
CREATE DATABASE NewsSiteProDB_Production;
```

2. **Run Database Scripts in Order**
```bash
# Navigate to Database2025 folder
cd Database2025

# Execute scripts in order:
sqlcmd -S your-server -d NewsSiteProDB_Production -i database_setup.sql
sqlcmd -S your-server -d NewsSiteProDB_Production -i tables.sql
sqlcmd -S your-server -d NewsSiteProDB_Production -i stored_procedures.sql
sqlcmd -S your-server -d NewsSiteProDB_Production -i constraints_indexes.sql
```

### Application Configuration

1. **Update Production Configuration**
Create `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "myProjDB": "Server=your-prod-server;Database=NewsSiteProDB_Production;User Id=your-user;Password=your-password;TrustServerCertificate=true"
  },
  "NewsAPI": {
    "ApiKey": "your-production-api-key",
    "BaseUrl": "https://newsapi.org/v2/"
  },
  "JWT": {
    "SecretKey": "your-super-secure-secret-key-min-32-chars",
    "Issuer": "NewsSitePro",
    "Audience": "NewsSiteUsers",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

2. **Environment Variables (Recommended for Security)**
```bash
# Database
export ConnectionStrings__myProjDB="your-connection-string"

# NewsAPI
export NewsAPI__ApiKey="your-api-key"

# JWT
export JWT__SecretKey="your-secret-key"
```

### Build and Publish

1. **Publish Application**
```bash
# Clean and publish for production
dotnet clean
dotnet restore
dotnet publish -c Release -o ./publish --runtime win-x64 --self-contained false
```

2. **Deploy Files**
```bash
# Copy published files to server
scp -r ./publish/* user@your-server:/var/www/newssitepro/
```

### IIS Deployment (Windows Server)

1. **Install Prerequisites**
- Install IIS with ASP.NET Core Module
- Install .NET 6.0 Hosting Bundle

2. **Create IIS Application**
```powershell
# Create application pool
New-WebAppPool -Name "NewsSiteProPool" -Force
Set-ItemProperty -Path "IIS:\AppPools\NewsSiteProPool" -Name processModel.identityType -Value ApplicationPoolIdentity

# Create website
New-Website -Name "NewsSitePro" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\newssitepro" -ApplicationPool "NewsSiteProPool"
```

3. **Configure web.config**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\NewsSite.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

### Linux/Docker Deployment

1. **Create Dockerfile**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NewsSite.csproj", "."]
RUN dotnet restore "./NewsSite.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "NewsSite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NewsSite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewsSite.dll"]
```

2. **Docker Compose Setup**
```yaml
version: '3.8'
services:
  newssitepro:
    build: .
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__myProjDB=${DB_CONNECTION_STRING}
      - NewsAPI__ApiKey=${NEWS_API_KEY}
    depends_on:
      - sqlserver

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SQL_PASSWORD}
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

### SSL/HTTPS Configuration

1. **Obtain SSL Certificate**
```bash
# Using Let's Encrypt (Linux)
sudo certbot --nginx -d yourdomain.com
```

2. **Configure HTTPS Redirection**
```csharp
// In Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

### Performance Optimization

1. **Enable Response Compression**
```csharp
// In Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

app.UseResponseCompression();
```

2. **Configure Caching**
```csharp
builder.Services.AddResponseCaching();
app.UseResponseCaching();
```

3. **Database Connection Pooling**
```json
{
  "ConnectionStrings": {
    "myProjDB": "Server=...;Database=...;Pooling=true;Max Pool Size=100;Min Pool Size=5;"
  }
}
```

### Monitoring and Logging

1. **Application Insights (Azure)**
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

2. **Structured Logging**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

3. **Health Checks**
```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString);

app.MapHealthChecks("/health");
```

### Security Considerations

1. **Update appsettings.Production.json**
```json
{
  "AllowedHosts": "yourdomain.com,www.yourdomain.com",
  "ForwardedHeaders": {
    "ForwardedHeaders": "XForwardedFor,XForwardedProto"
  }
}
```

2. **CORS Configuration**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("ProductionPolicy");
```

### Backup Strategy

1. **Database Backup Script**
```sql
-- Daily backup
BACKUP DATABASE NewsSiteProDB_Production 
TO DISK = 'C:\Backups\NewsSitePro_' + FORMAT(GETDATE(), 'yyyyMMdd') + '.bak'
WITH COMPRESSION, CHECKSUM;
```

2. **Application Backup**
```bash
#!/bin/bash
# Weekly application backup
tar -czf /backups/newssitepro-$(date +%Y%m%d).tar.gz /var/www/newssitepro/
```

### Post-Deployment Testing

1. **Smoke Tests**
```bash
# Test basic endpoints
curl -k https://yourdomain.com/health
curl -k https://yourdomain.com/api/posts
```

2. **Load Testing**
```bash
# Using Apache Bench
ab -n 1000 -c 10 https://yourdomain.com/
```

### Troubleshooting

#### Common Issues

1. **Connection String Issues**
```bash
# Test database connectivity
sqlcmd -S your-server -U your-user -P your-password -Q "SELECT 1"
```

2. **Permission Issues (Linux)**
```bash
# Fix file permissions
sudo chown -R www-data:www-data /var/www/newssitepro/
sudo chmod -R 755 /var/www/newssitepro/
```

3. **Memory Issues**
```bash
# Monitor memory usage
top -p $(pgrep dotnet)
```

#### Log Locations
- **Windows IIS**: `C:\inetpub\logs\LogFiles\`
- **Linux**: `/var/log/newssitepro/` or configured log directory
- **Docker**: `docker logs container-name`

### Maintenance

1. **Regular Updates**
```bash
# Update .NET runtime
sudo apt update && sudo apt upgrade dotnet-runtime-6.0
```

2. **Database Maintenance**
```sql
-- Weekly index maintenance
EXEC sp_updatestats;
ALTER INDEX ALL ON [dbo].[NewsArticles] REORGANIZE;
```

3. **Log Cleanup**
```bash
# Clean old logs (keep 30 days)
find /var/log/newssitepro/ -name "*.log" -mtime +30 -delete
```

This deployment guide ensures a production-ready setup with proper security, monitoring, and maintenance procedures.
