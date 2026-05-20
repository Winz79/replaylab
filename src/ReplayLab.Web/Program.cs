using ReplayLab.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReplayLabWeb();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapReplayLabWeb();

app.Run();

public partial class Program;
