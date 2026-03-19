using System.Net;
using System.Net.Http.Json;
using KanbanApp.Backend.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace KanbanApp.Tests;

public class UserProfileTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserProfileTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var toRemove = services.Where(d =>
                    d.ServiceType.FullName != null &&
                    d.ServiceType.FullName.Contains("DbContext")).ToList();
                foreach (var d in toRemove) services.Remove(d);

                var dbName = Guid.NewGuid().ToString();
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        }).CreateClient();
    }

    private async Task RegisterAndLogin(string email)
    {
        var reg = await _client.PostAsJsonAsync("/register", new { email, password = "Test123!" });
        reg.EnsureSuccessStatusCode();

        var login = await _client.PostAsJsonAsync("/login?useCookies=false&useSessionCookies=false",
            new { email, password = "Test123!" });
        login.EnsureSuccessStatusCode();

        var tokenData = await login.Content.ReadFromJsonAsync<JsonElement>();
        var token = tokenData.GetProperty("accessToken").GetString();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetMe_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsUserData()
    {
        await RegisterAndLogin("me@test.com");

        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("me@test.com", data.GetProperty("email").GetString());
        Assert.True(data.TryGetProperty("id", out _));
        Assert.True(data.TryGetProperty("userName", out _));
    }
}