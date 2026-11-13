using System.Net;
using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Notifications;
using ChorukoAPI.Data;
using ChorukoAPI.Services.Implementation;
using HouseholdService.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HouseholdService.Test;

public class NtfyNotifierTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly DataContext _context;

    public NtfyNotifierTests()
    {
        var databaseName = $"ntfy-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.AddDbContext<DataContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        _provider = services.BuildServiceProvider();
        _context = _provider.GetRequiredService<DataContext>();
    }

    public void Dispose()
    {
        _context.Dispose();
        _provider.Dispose();
    }

    [Fact]
    public void FromJson_ReadsTitleBodyAndUrl()
    {
        var notification = AppNotification.FromJson(
            """{"title":"New task","body":"Details","icon":"x","badge":"y","url":"/tasks"}""");

        Assert.Equal("New task", notification.Title);
        Assert.Equal("Details", notification.Body);
        Assert.Equal("/tasks", notification.Url);
    }

    [Fact]
    public async Task SendAsync_PostsToEachUserTopic()
    {
        await SeedUser("user-1", "choruko-topic-one");
        await SeedUser("user-2", "choruko-topic-two");

        var handler = new RecordingHandler();
        var notifier = CreateNotifier(handler, "http://ntfy.local", "secret", "http://choruko.local");

        await notifier.SendAsync(
            new[] { "user-1", "user-2", "user-1" },
            new AppNotification("Task completed", "Wash the dishes", "/tasks"));

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(
            new[] { "http://ntfy.local/choruko-topic-one", "http://ntfy.local/choruko-topic-two" },
            handler.Requests.Select(request => request.Uri.ToString()).OrderBy(uri => uri));

        var first = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, first.Method);
        Assert.Equal("Bearer secret", first.Authorization);
        Assert.Contains("\"title\":\"Task completed\"", first.Body);
        Assert.Contains("\"message\":\"Wash the dishes\"", first.Body);
        Assert.Contains("\"click\":\"http://choruko.local/tasks\"", first.Body);
    }

    [Fact]
    public async Task SendAsync_UserWithoutTopic_DoesNotPost()
    {
        await SeedUser("user-1", null);

        var handler = new RecordingHandler();
        var notifier = CreateNotifier(handler, "http://ntfy.local", "", "http://choruko.local");

        await notifier.SendAsync(new[] { "user-1" }, new AppNotification("Title", "Body"));

        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task SendAsync_MissingBaseUrl_DoesNotPost()
    {
        await SeedUser("user-1", "choruko-topic-one");

        var handler = new RecordingHandler();
        var notifier = CreateNotifier(handler, "", "", "http://choruko.local");

        await notifier.SendAsync(new[] { "user-1" }, new AppNotification("Title", "Body"));

        Assert.Empty(handler.Requests);
    }

    private NtfyNotifier CreateNotifier(
        RecordingHandler handler,
        string baseUrl,
        string token,
        string publicAppUrl)
    {
        var httpClient = new HttpClient(handler);

        return new NtfyNotifier(
            _provider.GetRequiredService<IServiceScopeFactory>(),
            new FakeHttpClientFactory(httpClient),
            Options.Create(new NtfySettings
            {
                BaseUrl = baseUrl,
                Token = token,
                PublicAppUrl = publicAppUrl,
            }),
            NullLogger<NtfyNotifier>.Instance);
    }

    private async Task SeedUser(string id, string? topic)
    {
        var user = TestData.CreateUser(id: id);
        user.NtfyTopic = topic;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    private sealed class FakeHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            Requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri!,
                request.Headers.Authorization?.ToString(),
                body));

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed record RecordedRequest(HttpMethod Method, Uri Uri, string? Authorization, string Body);
}
