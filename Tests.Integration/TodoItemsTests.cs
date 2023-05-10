using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace TodoLists.Tests.Integration;

public class TodoItemsTests
{
    [Test]
    public async Task AddTest01()
    {
        using var superUserHttpClient = new HttpClient();
        superUserHttpClient.BaseAddress = new Uri("https://localhost:7147");
        await AuthenticateSuperUser(superUserHttpClient);
        var profileNumber = await GetProfilesCount(superUserHttpClient);
        var profileName = "test" + profileNumber;
        var username = "kevin";
        await CreateProfile(profileName, superUserHttpClient);
        await CreateUser(profileName, username, superUserHttpClient);

        using var chromeDriver = new ChromeDriver();
        var wait = new WebDriverWait(chromeDriver, TimeSpan.FromSeconds(1))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };
        chromeDriver.Url = "https://localhost:7147/";
        var loginPopoverLinkElement = chromeDriver.FindElement(By.Id("loginPopoverLink"));
        loginPopoverLinkElement.Click();

        wait.Until(d => d.FindElement(By.ClassName("dx-popup-content")));

        chromeDriver.FindElement(By.XPath("//input[@name='profile']")).SendKeys(profileName);
        chromeDriver.FindElement(By.XPath("//input[@name='username']")).SendKeys(username);
        chromeDriver.FindElement(By.XPath("//input[@name='password']")).SendKeys("pass");
        chromeDriver.FindElement(By.Id("login-button")).Click();

        wait.Until(d => d.FindElement(By.Id("se-user-name")));
        
        chromeDriver.FindElement(By.CssSelector("#add-name")).SendKeys("foo");
        chromeDriver.FindElement(By.CssSelector(".se-add-todo-item-button")).Click();

        ReadOnlyCollection<IWebElement>? todoItemNameElements = null;
        wait.Until(d =>
        {
            todoItemNameElements = d.FindElements(By.CssSelector(".dx-datagrid .dx-data-row td[aria-colindex='2']"));
            return todoItemNameElements.Count == 1;
        });

        Assert.AreEqual("foo", todoItemNameElements![0].Text);
    }

    private static async Task CreateUser(string profileName, string username, HttpClient superUserHttpClient)
    {
        var content =
            new StringContent($"{{\"profile\":\"{profileName}\",\"username\":\"{username}\",\"password\":\"pass\"}}");
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var response = await superUserHttpClient.PostAsync("api/Users/register", content);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<int> GetProfilesCount(HttpClient superUserHttpClient)
    {
        var response = await superUserHttpClient.GetAsync("api/Profiles/Count");
        response.EnsureSuccessStatusCode();
        var profileNumber = int.Parse(await response.Content.ReadAsStringAsync());
        return profileNumber;
    }

    private static async Task CreateProfile(string profileName, HttpClient superUserHttpClient)
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
}