using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TodoLists.Tests.Integration.Utils;

namespace TodoLists.Tests.Integration.Tests;

public class SmokeTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        using var chromeDriver = ChromeDriverFactory.CreateChromeDriver();
        var wait = new WebDriverWait(chromeDriver, TimeSpan.FromSeconds(3))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };
        chromeDriver.Url = "https://localhost:7147/";
        var loginPopoverLinkElement = chromeDriver.FindElement(By.Id("loginPopoverLink"));
        loginPopoverLinkElement.Click();

        wait.Until(d => d.FindElement(By.Name("profile")).Displayed);

        chromeDriver.FindElement(By.XPath("//input[@name='profile']")).SendKeys("dev");
        chromeDriver.FindElement(By.XPath("//input[@name='username']")).SendKeys("user");
        chromeDriver.FindElement(By.XPath("//input[@name='password']")).SendKeys("user");
        chromeDriver.FindElement(By.Id("login-button")).Click();

        wait.Until(d => d.FindElement(By.Id("se-user-name")));

        
        Assert.That(chromeDriver.FindElement(By.Id("se-user-name")).Text, Is.EqualTo("user"));
    }
}