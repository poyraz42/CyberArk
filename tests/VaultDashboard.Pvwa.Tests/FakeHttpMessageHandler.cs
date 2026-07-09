using System.Net;
using System.Text;

namespace VaultDashboard.Pvwa.Tests;

/// <summary>Routes requests to a canned response by matching on (method, path-and-query) prefix.</summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly List<(Func<HttpRequestMessage, bool> Match, Func<HttpRequestMessage, HttpResponseMessage> Respond)> _routes = new();

    public List<HttpRequestMessage> Requests { get; } = new();

    public FakeHttpMessageHandler When(HttpMethod method, string pathContains, string jsonBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _routes.Add((
            req => req.Method == method && req.RequestUri!.PathAndQuery.Contains(pathContains, StringComparison.OrdinalIgnoreCase),
            _ => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
            }));
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        var route = _routes.FirstOrDefault(r => r.Match(request));
        if (route.Respond is null)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent($"No fake route registered for {request.Method} {request.RequestUri}"),
            });
        }

        return Task.FromResult(route.Respond(request));
    }
}
