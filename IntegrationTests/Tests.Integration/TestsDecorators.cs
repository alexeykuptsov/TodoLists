using System.Reactive.Disposables;
using TodoLists.Utils;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace TodoLists.Tests.Integration;

public static class TestsDecorators
{
    public static async Task Default(TestDecoratorOptions<MainPage> options)
    {
        var profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync();
        Assertion.Assert(options.SetUpAsync == null || options.SetUp == null,
            "options.SetUpAsync == null || options.SetUp == null");
        using var compositeDisposable = new CompositeDisposable();
        var testSetUpContext = new TestSetUpContext(profileName, compositeDisposable);
        if (options.SetUpAsync != null)
            await options.SetUpAsync(testSetUpContext);
        else if (options.SetUp != null)
            options.SetUp(testSetUpContext);
        using var browser = new Browser();
        var mainPage = browser.OpenSiteAndLogin(profileName, TestDataBuilder.DefaultUserName);
        mainPage.WaitUntilLoaded();

        var testContext = new TestContext<MainPage>(profileName, browser, mainPage);
        Assertion.Assert(options.TestAsync != null ^ options.Test != null,
            "options.ActionAsync != null ^ options.Action != null");
        if (options.TestAsync != null)
            await options.TestAsync(testContext);
        else
            options.Test(testContext);
    }
    
    public static async Task Default(Action<TestContext<MainPage>> action)
    {
        await Default(new TestDecoratorOptions<MainPage>
        {
            Test = action,
        });
    }
}