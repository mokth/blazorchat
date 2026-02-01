# Deployment Guide for Blazor Chat Application

This guide provides step-by-step instructions for deploying the Blazor Chat application to various environments.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Development](#local-development)
3. [Production Deployment](#production-deployment)
4. [IIS Deployment](#iis-deployment)
5. [Docker Deployment](#docker-deployment)
6. [Azure Deployment](#azure-deployment)
7. [WebForm Integration](#webform-integration)

## Prerequisites

- .NET 8.0 SDK or later
- A web server (IIS, Kestrel, Nginx, Apache)
- For HTTPS in production (required for voice recording)
- SQL Server or other database (optional, for persistent storage)

## Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/mokth/blazorchat.git
   cd blazorchat
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Access the application**
   - Open your browser to `https://localhost:5001`
   - Or `http://localhost:5000`

## Production Deployment

### Build for Production

```bash
dotnet publish -c Release -o ./publish
```

This creates a production-ready build in the `./publish` directory.

### Configuration for Production

1. **Update appsettings.json**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Warning"
       }
     },
     "AllowedHosts": "*",
     "Urls": "http://0.0.0.0:5000;https://0.0.0.0:5001"
   }
   ```

2. **Set environment variables**
   ```bash
   export ASPNETCORE_ENVIRONMENT=Production
   export ASPNETCORE_URLS="http://0.0.0.0:5000;https://0.0.0.0:5001"
   ```

## IIS Deployment

### Prerequisites
- IIS 10 or later
- ASP.NET Core Hosting Bundle installed

### Steps

1. **Install ASP.NET Core Hosting Bundle**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Install and restart IIS

2. **Create IIS Site**
   - Open IIS Manager
   - Right-click "Sites" → "Add Website"
   - Set site name: "BlazorChat"
   - Set physical path to your publish folder
   - Set binding (port 80 or 443 for HTTPS)

3. **Configure Application Pool**
   - Select the application pool for your site
   - Set ".NET CLR Version" to "No Managed Code"
   - Set "Managed Pipeline Mode" to "Integrated"

4. **Set Permissions**
   - Give IIS_IUSRS read/write access to:
     - Application folder
     - wwwroot/uploads folder

5. **Configure CORS (if needed)**
   - Update appsettings.json with allowed origins:
   ```json
   {
     "Cors": {
       "AllowedOrigins": ["https://your-webform-site.com"]
     }
   }
   ```

6. **SSL Certificate (Required for Voice Messages)**
   - Install SSL certificate in IIS
   - Bind certificate to your site
   - Enable HTTPS redirect

### web.config for IIS

Create a `web.config` file in your publish folder:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" 
                arguments=".\blazorchat.dll" 
                stdoutLogEnabled="false" 
                stdoutLogFile=".\logs\stdout" 
                hostingModel="inprocess">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

## Docker Deployment

### Create Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["blazorchat.csproj", "./"]
RUN dotnet restore "blazorchat.csproj"
COPY . .
RUN dotnet build "blazorchat.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "blazorchat.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "blazorchat.dll"]
```

### Build and Run Docker Container

```bash
# Build image
docker build -t blazorchat .

# Run container
docker run -d -p 5000:80 -p 5001:443 --name blazorchat blazorchat

# View logs
docker logs blazorchat
```

### Docker Compose

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  blazorchat:
    image: blazorchat
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80;https://+:443
    volumes:
      - ./uploads:/app/wwwroot/uploads
    restart: unless-stopped
```

Run with:
```bash
docker-compose up -d
```

## Azure Deployment

### Deploy to Azure App Service

1. **Install Azure CLI**
   ```bash
   curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
   ```

2. **Login to Azure**
   ```bash
   az login
   ```

3. **Create Resource Group**
   ```bash
   az group create --name BlazorChatRG --location eastus
   ```

4. **Create App Service Plan**
   ```bash
   az appservice plan create --name BlazorChatPlan --resource-group BlazorChatRG --sku B1 --is-linux
   ```

5. **Create Web App**
   ```bash
   az webapp create --resource-group BlazorChatRG --plan BlazorChatPlan --name blazorchat-app --runtime "DOTNET:8.0"
   ```

6. **Deploy Application**
   ```bash
   az webapp deployment source config-zip --resource-group BlazorChatRG --name blazorchat-app --src ./publish.zip
   ```

7. **Configure App Settings**
   ```bash
   az webapp config appsettings set --resource-group BlazorChatRG --name blazorchat-app --settings ASPNETCORE_ENVIRONMENT=Production
   ```

8. **Enable WebSockets**
   ```bash
   az webapp config set --resource-group BlazorChatRG --name blazorchat-app --web-sockets-enabled true
   ```

## WebForm Integration

### Same Server Deployment

If deploying on the same IIS server as your WebForm application:

1. **Deploy Blazor Chat** to a separate application/site (e.g., `https://yourserver.com/blazorchat`)

2. **Add reference in WebForm pages**:
   ```html
   <script src="/blazorchat/Integration/webform-integration.js"></script>
   ```

3. **Use relative URLs** in integration code:
   ```javascript
   openChatPopup('/blazorchat', userId, userName);
   ```

### Different Server Deployment

If Blazor Chat is on a different server:

1. **Configure CORS** in Blazor Chat's `appsettings.json`:
   ```json
   {
     "Cors": {
       "AllowedOrigins": ["https://your-webform-site.com"]
     }
   }
   ```

2. **Update Program.cs** to use specific CORS policy:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddDefaultPolicy(policy =>
       {
           var allowedOrigins = builder.Configuration
               .GetSection("Cors:AllowedOrigins")
               .Get<string[]>() ?? new[] { "*" };
           
           policy.WithOrigins(allowedOrigins)
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials();
       });
   });
   ```

3. **Use absolute URLs** in WebForm:
   ```javascript
   openChatPopup('https://blazorchat.yourserver.com', userId, userName);
   ```

### Testing Integration

1. **Test popup window**:
   ```html
   <button onclick="openChatPopup('https://blazorchat-url', '123', 'John Doe')">
       Open Chat
   </button>
   ```

2. **Test floating button**:
   ```javascript
   window.onload = function() {
       createFloatingChatButton('https://blazorchat-url', '123', 'John Doe');
   };
   ```

3. **Test embedded chat**:
   ```html
   <div id="chatContainer" style="height: 600px;"></div>
   <script>
       embedChatIframe('chatContainer', 'https://blazorchat-url', '123', 'John Doe');
   </script>
   ```

## Troubleshooting

### WebSocket Connection Issues

- Ensure WebSockets are enabled in IIS/Azure
- Check firewall rules allow WebSocket connections
- Verify proxy servers support WebSocket upgrade

### HTTPS Issues

- Install valid SSL certificate
- Configure HTTPS redirect
- Ensure SignalR uses secure WebSocket (wss://)

### File Upload Issues

- Check wwwroot/uploads folder permissions
- Verify file size limits in appsettings.json
- Ensure allowed file extensions are configured

### Performance Tuning

- Enable response compression
- Configure SignalR hub scaling (Azure SignalR Service)
- Implement message pagination for large chat histories
- Use Redis for distributed caching across multiple instances

## Security Considerations

1. **Input Validation**: All user inputs are validated
2. **File Upload**: File types and sizes are restricted
3. **HTTPS**: Required for production, especially for voice recording
4. **CORS**: Configure only allowed origins
5. **Authentication**: Consider adding authentication for production use
6. **Rate Limiting**: Implement rate limiting to prevent abuse

## Monitoring and Logging

### Application Insights (Azure)

```bash
az monitor app-insights component create --app blazorchat-insights --location eastus --resource-group BlazorChatRG
```

### Configure in appsettings.json

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

## Scaling

For high-traffic scenarios:

1. **Use Azure SignalR Service** for connection management
2. **Deploy multiple instances** behind a load balancer
3. **Use Redis** for distributed caching
4. **Implement message persistence** with database
5. **Use blob storage** for file uploads

## Support

For issues or questions:
- GitHub Issues: https://github.com/mokth/blazorchat/issues
- Documentation: See README.md

## License

This project is open source and available under the MIT License.
