using System.Net;
using System.Text;
using AutoFixture;
using Elisoft.Pushover.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace Elisoft.Pushover.Tests;

[TestFixture]
public class PushoverNotificatorTests
{
    private Fixture _fixture = null!;
    private ILogger<PushoverNotificator> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = A.Fake<ILogger<PushoverNotificator>>();
    }

    [Test]
    public async Task SendMessageAsync_ApiTokenIsNull_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK);
        var sut = new PushoverNotificator(httpClient, _logger);

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
            await sut.SendMessageAsync(null!, "user-key", "msg"));
    }

    [Test]
    public async Task SendMessageAsync_UserKeyIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK);
        var sut = new PushoverNotificator(httpClient, _logger);

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
            await sut.SendMessageAsync("api-token", "", "msg"));
    }

    [Test]
    public async Task SendMessageAsync_MessageTextIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK);
        var sut = new PushoverNotificator(httpClient, _logger);

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
            await sut.SendMessageAsync("api-token", "user-key", ""));
    }

    [Test]
    public async Task SendMessageAsync_ResponseIsSuccess_DoesNotThrow()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK);
        var sut = new PushoverNotificator(httpClient, _logger);
        var apiToken = _fixture.Create<string>();
        var userKey = _fixture.Create<string>();
        var message = _fixture.Create<string>();

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await sut.SendMessageAsync(apiToken, userKey, message));
    }

    [Test]
    public async Task SendMessageAsync_ResponseIsFailure_ThrowsHttpRequestException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.BadRequest);
        var sut = new PushoverNotificator(httpClient, _logger);
        var apiToken = _fixture.Create<string>();
        var userKey = _fixture.Create<string>();
        var message = _fixture.Create<string>();

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(async () =>
            await sut.SendMessageAsync(apiToken, userKey, message));
    }

    [Test]
    public async Task SendMessageAsync_HttpClientThrowsException_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = A.Fake<HttpMessageHandler>();
        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ThrowsAsync(new HttpRequestException());

        var httpClient = new HttpClient(handler);
        var sut = new PushoverNotificator(httpClient, _logger);
        var apiToken = _fixture.Create<string>();
        var userKey = _fixture.Create<string>();
        var message = _fixture.Create<string>();

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(async () =>
            await sut.SendMessageAsync(apiToken, userKey, message));
    }

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode)
    {
        var handler = A.Fake<HttpMessageHandler>();

        A.CallTo(handler)
            .Where(call => call.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .Returns(Task.FromResult(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("response", Encoding.UTF8)
            }));

        return new HttpClient(handler);
    }
}
