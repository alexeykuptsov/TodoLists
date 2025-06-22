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
        options.AddArgument("--no-first-run");
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);
        options.AddUserProfilePreference("profile.password_manager_leak_detection", false);
        
        return new ChromeDriver(options);
    }
}