using blazorchat.Hubs;
using blazorchat.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Register application services
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddScoped<FileUploadService>();

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
