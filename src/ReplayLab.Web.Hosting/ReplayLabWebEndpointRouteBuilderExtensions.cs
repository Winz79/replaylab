using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace ReplayLab.Web.Hosting;

public static class ReplayLabWebEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapReplayLabWeb(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRazorPages();
        return endpoints;
    }
}
