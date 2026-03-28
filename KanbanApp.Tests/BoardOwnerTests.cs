using System.Net;
using System.Net.Http.Json;
using KanbanApp.Backend.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace KanbanApp.Tests;

public class BoardOwnerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BoardOwnerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var toRemove = services.Where(d =>
                    d.ServiceType.FullName != null &&
                    d.ServiceType.FullName.Contains("DbContext")).ToList();
                foreach (var d in toRemove) services.Remove(d);
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb6"));
            });
        });
    }

    private async Task<HttpClient> RegisterAndLogin(string email)
    {
        var client = _factory.CreateClient();
        var reg = await client.PostAsJsonAsync("/register", new { email, password = "Test123!" });
        reg.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/login?useCookies=false&useSessionCookies=false",
            new { email, password = "Test123!" });
        login.EnsureSuccessStatusCode();

        var token = (await login.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task UpdateBoard_AsOwner_ReturnsOk()
    {
        var client = await RegisterAndLogin("updateowner@test.com");
        var create = await client.PostAsJsonAsync("/api/boards", new { boardName = "Old Name" });
        var boardId = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var response = await client.PutAsJsonAsync($"/api/boards/{boardId}", new { name = "New Name" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("New Name", data.GetProperty("name").GetString());
    }

    [Fact]
    public async Task DeleteBoard_AsOwner_ReturnsNoContent()
    {
        var client = await RegisterAndLogin("deleteowner@test.com");
        var create = await client.PostAsJsonAsync("/api/boards", new { boardName = "To Delete" });
        var boardId = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var response = await client.DeleteAsync($"/api/boards/{boardId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBoard_AsNonMember_ReturnsForbid()
    {
        var ownerClient = await RegisterAndLogin("boardowner3@test.com");
        var create = await ownerClient.PostAsJsonAsync("/api/boards", new { boardName = "Owner Board" });
        var boardId = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var memberClient = await RegisterAndLogin("member3@test.com");

        var response = await memberClient.PutAsJsonAsync($"/api/boards/{boardId}", new { name = "Hacked" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBoard_AsNonMember_ReturnsForbid()
    {
        var ownerClient = await RegisterAndLogin("boardowner4@test.com");
        var create = await ownerClient.PostAsJsonAsync("/api/boards", new { boardName = "Owner Board 2" });
        var boardId = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var memberClient = await RegisterAndLogin("member4@test.com");

        var response = await memberClient.DeleteAsync($"/api/boards/{boardId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBoard_AsInvitedBoardMember_ReturnsForbid()
    {
        var ownerClient = await RegisterAndLogin("inviteowner1@test.com");
        var create = await ownerClient.PostAsJsonAsync("/api/boards", new { boardName = "Secured Board" });
        var boardId = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var memberClient = await RegisterAndLogin("invitedmember1@test.com");
        await ownerClient.PostAsJsonAsync($"/api/boards/{boardId}/members",
            new { email = "invitedmember1@test.com" });

        var response = await memberClient.PutAsJsonAsync($"/api/boards/{boardId}", new { name = "Hacked" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBoard_AsInvitedBoardMember_ReturnsForbid()
    {
        var ownerClient = await RegisterAndLogin("inviteowner2@test.com");
        var create = await ownerClient.PostAsJsonAsync("/api/boards", new { boardName = "Secured Board 2" });
        var boardId = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var memberClient = await RegisterAndLogin("invitedmember2@test.com");
        await ownerClient.PostAsJsonAsync($"/api/boards/{boardId}/members",
            new { email = "invitedmember2@test.com" });

        var response = await memberClient.DeleteAsync($"/api/boards/{boardId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}