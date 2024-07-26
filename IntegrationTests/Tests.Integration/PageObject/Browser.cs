using NUnit.Framework.Constraints;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TodoLists.Tests.Integration.Utils.NUnit;

namespace TodoLists.Tests.Integration.PageObject;

public sealed class Browser : IDisposable
{
    public ChromeDriver Driver { get; }
    public WebDriverWait Wait { get; }

    public Browser()
    {
        Driver = new ChromeDriver();
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(3))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };
        Wait.IgnoreExceptionTypes(typeof(AssertionException));
        Wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
    }

    public void Dispose()
    {
        Driver.Close();
        Driver.Quit();
        Driver.Dispose();
    }

    public MainPage OpenSiteAndLogin(string profileName, string username)
    {
        Driver.Url = "https://localhost:7147/";
        var loginPopoverLinkElement = Driver.FindElement(By.Id("loginPopoverLink"));
        loginPopoverLinkElement.Click();

        Wait.Until(d => d.FindElement(By.ClassName("dx-popup-content")));

        Driver.FindElement(By.XPath("//input[@name='profile']")).SendKeys(profileName);
        Driver.FindElement(By.XPath("//input[@name='username']")).SendKeys(username);
        Driver.FindElement(By.XPath("//input[@name='password']")).SendKeys(username);
        Driver.FindElement(By.Id("login-button")).Click();

        var mainPage = new MainPage(this);
        mainPage.WaitUntilLoaded();
        return mainPage;
    }
    
    public void WaitAndAssertThat<TActual>(Func<TActual> actualFunc, IResolveConstraint expression)
    {
        try
        {
            Wait.Until(_ =>
            {
                AssertSlim.That(actualFunc(), expression);
                return true;
            });
        }
        catch (WebDriverTimeoutException e)
        {
            throw e.InnerException ?? e;
        }
    }
}