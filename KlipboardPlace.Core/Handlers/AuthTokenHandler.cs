using System.Net;
using System.Net.Http.Headers;
using KlipboardPlace.Core.Interfaces;

namespace KlipboardPlace.Core.Handlers;

public class AuthTokenHandler: DelegatingHandler
{
    private readonly IAuthService _authService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="authService"></param>
    public AuthTokenHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await AttachTokenAsync(request, cancellationToken).ConfigureAwait(false);
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            
            _authService.Invalidate();
            var retry = await CloneRequestAsync(request).ConfigureAwait(false);
            
            await AttachTokenAsync(retry, cancellationToken).ConfigureAwait(false);
            return await base.SendAsync(retry, cancellationToken).ConfigureAwait(false);
        }
        
        return response;
    }
    
    private async Task AttachTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await _authService.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(response.token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(response.tokenType, response.token);
        }
    }
    
    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
        };

        if (request.Content is not null)
        {
            var buffer = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            clone.Content = new ByteArrayContent(buffer);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        foreach (var option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        return clone;
    }
}