namespace TodoLists.Tests.Integration;

public class TestContext
{
    public string ProfileName { get; }
    public Browser Browser { get; }
    public MainPage MainPage { get; }

    public TestContext(string profileName, Browser browser, MainPage mainPage)
    {
        ProfileName = profileName;
        Browser = browser;
        MainPage = mainPage;
    }
}