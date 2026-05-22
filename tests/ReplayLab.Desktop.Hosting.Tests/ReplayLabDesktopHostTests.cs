using Microsoft.AspNetCore.Builder;
using ReplayLab.Core;
using ReplayLab.Desktop.Hosting;
using ReplayLab.Parsers.Csv;

namespace ReplayLab.Desktop.Hosting.Tests;

public class ReplayLabDesktopHostTests
{
    [Fact]
    public void BuildApp_registers_default_services()
    {
        var app = ReplayLabDesktopHost.BuildApp(Array.Empty<string>());

        var parser = app.Services.GetService<IMessageParser>();
        var sender = app.Services.GetService<IReplaySender>();

        Assert.NotNull(parser);
        Assert.IsType<CsvReplayMessageParser>(parser);
        Assert.NotNull(sender);
    }

    [Fact]
    public void BuildApp_allows_custom_service_registration()
    {
        var app = ReplayLabDesktopHost.BuildApp(Array.Empty<string>(), services =>
        {
            services.AddSingleton<IMessageParser, CustomTestParser>();
        });

        var parser = app.Services.GetService<IMessageParser>();

        Assert.NotNull(parser);
        Assert.IsType<CustomTestParser>(parser);
    }

    [Fact]
    public void BuildApp_allows_custom_sender_override()
    {
        var app = ReplayLabDesktopHost.BuildApp(Array.Empty<string>(), services =>
        {
            services.AddSingleton<IReplaySender, CustomTestSender>();
        });

        var sender = app.Services.GetService<IReplaySender>();

        Assert.NotNull(sender);
        Assert.IsType<CustomTestSender>(sender);
    }

    [Fact]
    public void BuildApp_allows_custom_parser_and_sender_override()
    {
        var app = ReplayLabDesktopHost.BuildApp(Array.Empty<string>(), services =>
        {
            services.AddSingleton<IMessageParser, CustomTestParser>();
            services.AddSingleton<IReplaySender, CustomTestSender>();
        });

        var parser = app.Services.GetService<IMessageParser>();
        var sender = app.Services.GetService<IReplaySender>();

        Assert.NotNull(parser);
        Assert.IsType<CustomTestParser>(parser);
        Assert.NotNull(sender);
        Assert.IsType<CustomTestSender>(sender);
    }

    [Fact]
    public void GetLocalUrl_throws_on_null_app()
    {
        Assert.Throws<ArgumentNullException>(() => ReplayLabDesktopHost.GetLocalUrl(null!));
    }

    [Fact]
    public void GetLocalUrl_throws_when_server_has_no_addresses()
    {
        var app = ReplayLabDesktopHost.BuildApp(Array.Empty<string>());

        // App has not been started, so IServerAddressesFeature has no addresses.
        Assert.Throws<InvalidOperationException>(() => ReplayLabDesktopHost.GetLocalUrl(app));
    }

    private sealed class CustomTestParser : IMessageParser
    {
        public Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class CustomTestSender : IReplaySender
    {
        public Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ReplayResult { Success = true, MessageId = message.Id });
        }
    }
}
