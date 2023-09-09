namespace TodoLists.Tests.Integration;

public class TestContext<TPage> where TPage : BasePage
{
    public string ProfileName { get; }
    public Browser Browser { get; }
    
    /// <summary>
    /// The page opened at the beginning of the current test.
    /// </summary>
    public TPage Page { get; }

    public TestContext(string profileName, Browser browser, TPage page)
    {
        ProfileName = profileName;
        Browser = browser;
        Page = page;
    }
}