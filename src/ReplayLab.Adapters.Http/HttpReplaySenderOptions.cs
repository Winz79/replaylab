namespace ReplayLab.Adapters.Http;

public sealed record HttpReplaySenderOptions
{
    public HttpReplaySenderOptions(Uri endpointUrl)
    {
        if (!endpointUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("Endpoint URL must be an absolute URI.", nameof(endpointUrl));
        }

        EndpointUrl = endpointUrl;
    }

    public Uri EndpointUrl { get; }
}
