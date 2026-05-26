using ReplayLab.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);

var seqUrl = Environment.GetEnvironmentVariable("SEQ_SERVER_URL");
if (!string.IsNullOrWhiteSpace(seqUrl))
{
    builder.Logging.AddSeq(seqUrl);
}

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

public partial class Program;
