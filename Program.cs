using blazorchat.Data;
using blazorchat.Hubs;
using blazorchat.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Blazor Server Circuit options
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.DisconnectedCircuitMaxRetained = 100;
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(2);
});

builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.MaximumReceiveMessageSize = 30 * 1024 * 1024; // 30 MB to handle base64 payloads
    options.StreamBufferCapacity = 20;
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Register application services
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddHostedService<ChatCleanupService>();

builder.Services.AddDbContextFactory<ChatDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ChatDatabase");
    options.UseSqlServer(connectionString);
});

// Add CORS for WebForm integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebForm", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost" };
        
        if (allowedOrigins.Contains("*"))
        {
            // Development only - allow any origin
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Production - restrict to specific origins
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ChatDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();

    // Apply any pending migrations
    await dbContext.Database.MigrateAsync();
}

//app.UsePathBase("/chat");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseCors("AllowWebForm");

app.MapRazorComponents<blazorchat.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapHub<ChatHub>("/chathub");

app.Run();
