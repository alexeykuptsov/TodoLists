namespace TodoLists.Tests.Integration;

public static class TestsDecorators
{
    public static async Task Default(Func<TestContext, Task> asyncAction)
    {
        string profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync();
        using var browser = new Browser();
        var mainPage = browser.OpenSiteAndLogin(profileName, TestDataBuilder.DefaultUserName);
        mainPage.WaitUntilLoaded();
        await asyncAction(new TestContext(profileName, browser, mainPage));
    }
    
    public static async Task Default(Action<TestContext> action)
    {
        await Default(x =>
        {
            action(x);
            return Task.CompletedTask;
        });
    }
}