using OpenQA.Selenium.Chrome;

namespace TodoLists.Tests.Integration.Utils;

public static class ChromeDriverFactory
{
    /// <summary>
    /// Creates a ChromeDriver with options configured to suppress password security warnings and notifications.
    /// </summary>
    /// <returns>A configured ChromeDriver instance</returns>
    public static ChromeDriver CreateChromeDriver()
    {
        var options = new ChromeOptions();
        
        // Suppress password security warnings and notifications
        options.AddArgument("--disable-password-generation");
        options.AddArgument("--disable-password-manager-reauthentication");
        options.AddArgument("--disable-save-password-bubble");
        options.AddArgument("--disable-features=VizDisplayCompositor,PasswordManager");
        options.AddArgument("--no-first-run");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--disable-notifications");
        
        return new ChromeDriver(options);
    }
}