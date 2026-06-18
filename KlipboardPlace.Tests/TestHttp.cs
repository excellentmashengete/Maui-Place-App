using System.Net;
using System.Text;

namespace KlipboardPlace.Tests;

/// <summary>
/// An <see cref="HttpMessageHandler"/> that returns a canned response, records the requests it
/// receives and counts how many times it was invoked.
/// </summary>
public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public StubHttpMessageHandler(string jsonBody, HttpStatusCode statusCode = HttpStatusCode.OK)
        : this(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
        })
    {
    }

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public int CallCount { get; private set; }

    public List<HttpRequestMessage> Requests { get; } = new();

    /// <summary>Request body text captured at send time (the request itself may be disposed later).</summary>
    public List<string?> RequestBodies { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        Requests.Add(request);
        RequestBodies.Add(request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken));
        return _responder(request);
    }
}

/// <summary>
/// Minimal <see cref="IHttpClientFactory"/> that hands out clients backed by a single handler.
/// Optionally pre-configures a base address, mirroring what <c>AddHttpClient</c> does in the app.
/// </summary>
public class StubHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;
    private readonly Uri? _baseAddress;

    public StubHttpClientFactory(HttpMessageHandler handler, string? baseAddress = null)
    {
        _handler = handler;
        _baseAddress = baseAddress is null ? null : new Uri(baseAddress);
    }

    public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false)
    {
        BaseAddress = _baseAddress,
    };
}
