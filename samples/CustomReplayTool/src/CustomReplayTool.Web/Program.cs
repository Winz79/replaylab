using CustomReplayTool.Domain;
using ReplayLab.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Register custom parser and sender before AddReplayLabWeb so TryAdd doesn't override them.
builder.Services.AddTicketReplayServices();

// Compose ReplayLab Web hosting; default parser/sender won't override our registrations.
builder.Services.AddReplayLabWeb();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.MapStaticAssets();
app.MapReplayLabWeb();

app.Run();
