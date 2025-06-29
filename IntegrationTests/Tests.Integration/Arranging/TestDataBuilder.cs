using System.Net.Http.Headers;
using Newtonsoft.Json;

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

    public static async Task<long> CreateProjectAsync(string name, HttpClient httpClient)
    {
        var content = new StringContent($"{{\"name\":\"{name}\"}}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await httpClient.PostAsync("api/Projects", content);
        response.EnsureSuccessStatusCode();
        return long.Parse(await response.Content.ReadAsStringAsync());
    }

    public static async Task RenameProjectAsync(string sourceName, string targetName, HttpClient httpClient)
    {
        var getResponse = await httpClient.GetAsync("api/Projects");
        getResponse.EnsureSuccessStatusCode();
        var getResponseDict = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(await getResponse.Content.ReadAsStringAsync());
        var id = (long)getResponseDict!.Single(x => (string)x["name"] == sourceName)["id"];

        var content = new StringContent($"[{{\"data\":{{\"id\":{id},\"name\":\"{targetName}\"}},\"key\":{id},\"type\":\"update\"}}]");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await httpClient.PatchAsync("api/Projects", content);
        response.EnsureSuccessStatusCode();
    }

    public static async Task CreateTodoItemAsync(long projectId, string todoItemName, bool isComplete, HttpClient httpClient)
    {
        var content =
            new StringContent($"{{\"projectId\":{projectId},\"name\":\"{todoItemName}\",\"isComplete\":{isComplete.ToString().ToLower()}}}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await httpClient.PostAsync("api/TodoItems", content);
        response.EnsureSuccessStatusCode();
    }

    public static async Task RenameTodoItemAsync(long projectId, string sourceName, string targetName, HttpClient httpClient)
    {
        var getResponse = await httpClient.GetAsync($"api/TodoItems?projectId={projectId}");
        getResponse.EnsureSuccessStatusCode();
        var getResponseDict = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(await getResponse.Content.ReadAsStringAsync());
        var todoItem = getResponseDict!.Single(x => (string)x["name"] == sourceName);
        var id = (long)todoItem["id"];

        var content = new StringContent($"[{{\"data\":{{\"id\":{id},\"name\":\"{targetName}\"}},\"key\":{id},\"type\":\"update\"}}]");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await httpClient.PatchAsync("api/TodoItems", content);
        response.EnsureSuccessStatusCode();
    }

    private static async Task CreateUserAsync(string profileName, string username, HttpClient hHttpClient)
    {
        var content =
            new StringContent($"{{\"profile\":\"{profileName}\",\"username\":\"{username}\",\"password\":\"{username}\"}}");
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
        var content = new StringContent("{\"username\":\"admin\",\"password\":\"admin\"}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await superUserHttpClient.PostAsync("api/Auth/LoginSuperUser", content);
        response.EnsureSuccessStatusCode();
        var jwtToken = await response.Content.ReadAsStringAsync();
        superUserHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    }

    private static async Task AuthenticateUserAsync(HttpClient httpClient, string profileName, string username)
    {
        var content = new StringContent($"{{\"profile\":\"{profileName}\",\"username\":\"{username}\",\"password\":\"{username}\"}}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await httpClient.PostAsync("api/Auth/Login", content);
        response.EnsureSuccessStatusCode();
        var jwtToken = await response.Content.ReadAsStringAsync();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    }
}