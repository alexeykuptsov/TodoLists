using System.Net.Http.Headers;

namespace TodoLists.Tests.Integration.Arranging;

public class TestDataBuilder
{
    public const string DefaultUserName = "kevin";
    
    public static async Task<string> CreateProfileWithSingleUserAsync(string username = DefaultUserName)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7147");
        await AuthenticateSuperUser(httpClient);
        var profileNumber = await GetProfilesCountAsync(httpClient);
        var profileName = "test" + profileNumber;
        await CreateProfileAsync(profileName, httpClient);
        await CreateUserAsync(profileName, username, httpClient);
        return profileName;
    }

    public static async Task<HttpClient> CreateHttpClientAndAuthenticateAsync(string profileName, string username)
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7147");
        await AuthenticateUserAsync(httpClient, profileName, username);
        return httpClient;
    }

    public static async Task CreateTodoItemAsync(long projectId, string todoItemName, bool isComplete, HttpClient httpClient)
    {
        var content =
            new StringContent($"{{\"projectId\":{projectId},\"name\":\"{todoItemName}\",\"isComplete\":{isComplete.ToString().ToLower()}}}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await httpClient.PostAsync("api/TodoItems", content);
        response.EnsureSuccessStatusCode();
    }

    private static async Task CreateUserAsync(string profileName, string username, HttpClient hHttpClient)
    {
        var content =
            new StringContent($"{{\"profile\":\"{profileName}\",\"username\":\"{username}\",\"password\":\"pass\"}}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await hHttpClient.PostAsync("api/Users/register", content);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<int> GetProfilesCountAsync(HttpClient superUserHttpClient)
    {
        var response = await superUserHttpClient.GetAsync("api/Profiles/MaxId");
        response.EnsureSuccessStatusCode();
        var profileNumber = int.Parse(await response.Content.ReadAsStringAsync()) + 1;
        return profileNumber;
    }

    private static async Task CreateProfileAsync(string profileName, HttpClient superUserHttpClient)
    {
        var content = new StringContent($"{{\"name\":\"{profileName}\"}}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await superUserHttpClient.PostAsync("api/Profiles", content);
        response.EnsureSuccessStatusCode();
    }

    private static async Task AuthenticateSuperUser(HttpClient superUserHttpClient)
    {
        var content = new StringContent("{\"username\":\"admin\",\"password\":\"pass\"}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await superUserHttpClient.PostAsync("api/Auth/LoginSuperUser", content);
        response.EnsureSuccessStatusCode();
        var jwtToken = await response.Content.ReadAsStringAsync();
        superUserHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    }

    private static async Task AuthenticateUserAsync(HttpClient httpClient, string profileName, string username)
    {
        var content = new StringContent($"{{\"profile\":\"{profileName}\",\"username\":\"{username}\",\"password\":\"pass\"}}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await httpClient.PostAsync("api/Auth/Login", content);
        response.EnsureSuccessStatusCode();
        var jwtToken = await response.Content.ReadAsStringAsync();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    }
}