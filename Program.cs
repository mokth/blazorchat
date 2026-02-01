using blazorchat.Hubs;
using blazorchat.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

// Register custom services
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddScoped<FileUploadService>();

// Configure CORS for WebForm integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebForms", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

app.UseRouting();
app.UseCors("AllowWebForms");
app.UseAntiforgery();

app.MapRazorComponents<blazorchat.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapHub<ChatHub>("/chathub");

app.Run();
