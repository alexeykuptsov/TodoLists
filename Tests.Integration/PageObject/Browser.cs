using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace TodoLists.Tests.Integration.PageObject;

public class Browser : IDisposable
{
    public ChromeDriver Driver { get; }
    public WebDriverWait Wait { get; }

    public Browser()
    {
        Driver = new ChromeDriver();
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(1))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };
    }

    public void Dispose()
    {
        Driver.Dispose();
    }

    public MainBasePage OpenSiteAndLogin(string profileName, string username)
    {
        Driver.Url = "https://localhost:7147/";
        var loginPopoverLinkElement = Driver.FindElement(By.Id("loginPopoverLink"));
        loginPopoverLinkElement.Click();

        Wait.Until(d => d.FindElement(By.ClassName("dx-popup-content")));

        Driver.FindElement(By.XPath("//input[@name='profile']")).SendKeys(profileName);
        Driver.FindElement(By.XPath("//input[@name='username']")).SendKeys(username);
        Driver.FindElement(By.XPath("//input[@name='password']")).SendKeys("pass");
        Driver.FindElement(By.Id("login-button")).Click();

        var mainPage = new MainBasePage(this);
        mainPage.WaitUntilLoaded();
        return mainPage;
    }

    public void RefreshPage()
    {
        Driver.Url = Driver.Url;
    }
}