using System.Net;
using System.Net.Http.Json;
using KanbanApp.Backend.Data;
using KanbanApp.Backend.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace KanbanApp.Tests;

public class BoardTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public BoardTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
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
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateBoard_WithValidData_CreatesBoardAndMembership()
    {
        await _client.PostAsJsonAsync("/register", new { email = "board@test.com", password = "Test123!" });
        var loginResponse = await _client.PostAsJsonAsync("/login?useCookies=false&useSessionCookies=false",
            new { email = "board@test.com", password = "Test123!" });
        var tokenData = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = tokenData.GetProperty("accessToken").GetString();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/boards", new { boardName = "Test Board" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var board = db.Boards.FirstOrDefault(b => b.Name == "Test Board");
        Assert.NotNull(board);
        var membership = db.BoardMembers.FirstOrDefault(bm => bm.BoardId == board.Id);
        Assert.NotNull(membership);
        Assert.Equal(BoardRole.Owner, membership.Role);
    }
    
    [Fact]
    public async Task GetBoards_ReturnsUserBoards()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/register", new { email = "listboards@test.com", password = "Test123!" });
        var login = await client.PostAsJsonAsync("/login?useCookies=false&useSessionCookies=false",
            new { email = "listboards@test.com", password = "Test123!" });
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await client.PostAsJsonAsync("/api/boards", new { boardName = "Board A" });
        await client.PostAsJsonAsync("/api/boards", new { boardName = "Board B" });

        var response = await client.GetAsync("/api/boards");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var boards = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, boards.GetArrayLength());
    }
    
}

