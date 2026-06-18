using System.Net;
using KlipboardPlace.Core.Handlers;
using KlipboardPlace.Core.Interfaces;
using Moq;
using Xunit;

namespace KlipboardPlace.Tests;

public class AuthTokenHandlerTests
{
    [Fact]
    public async Task SendAsync_AttachesBearerToken()
    {
        var inner = new StubHttpMessageHandler("{}");
        var auth = new Mock<IAuthService>();
        auth.Setup(a => a.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(("tok-42", "Bearer"));

        var handler = new AuthTokenHandler(auth.Object) { InnerHandler = inner };
        using var invoker = new HttpMessageInvoker(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example/x");
        using var _ = await invoker.SendAsync(request, CancellationToken.None);

        var sent = Assert.Single(inner.Requests);
        Assert.Equal("Bearer", sent.Headers.Authorization!.Scheme);
        Assert.Equal("tok-42", sent.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_OnUnauthorized_InvalidatesTokenAndRetries()
    {
        var statuses = new Queue<HttpStatusCode>(new[] { HttpStatusCode.Unauthorized, HttpStatusCode.OK });
        var inner = new StubHttpMessageHandler(_ => new HttpResponseMessage(statuses.Dequeue()));
        var auth = new Mock<IAuthService>();
        auth.Setup(a => a.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(("tok", "Bearer"));

        var handler = new AuthTokenHandler(auth.Object) { InnerHandler = inner };
        using var invoker = new HttpMessageInvoker(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example/x");
        using var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, inner.CallCount);              // original 401 + one retry
        auth.Verify(a => a.Invalidate(), Times.Once);  // cached token dropped before retrying
    }
}
