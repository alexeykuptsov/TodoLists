using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace TodoLists.Tests.Integration;

public class SmokeTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        using var chromeDriver = new ChromeDriver();
        var wait = new WebDriverWait(chromeDriver, TimeSpan.FromSeconds(1))
        {
            PollingInterval = TimeSpan.FromMilliseconds(100),
        };
        chromeDriver.Url = "https://localhost:7147/";
        var loginPopoverLinkElement = chromeDriver.FindElement(By.Id("loginPopoverLink"));
        loginPopoverLinkElement.Click();

        wait.Until(d => d.FindElement(By.ClassName("dx-popup-content")));

        chromeDriver.FindElement(By.XPath("//input[@name='profile']")).SendKeys("dev");
        chromeDriver.FindElement(By.XPath("//input[@name='username']")).SendKeys("user");
        chromeDriver.FindElement(By.XPath("//input[@name='password']")).SendKeys("pass");
        chromeDriver.FindElement(By.Id("login-button")).Click();

        wait.Until(d => d.FindElement(By.Id("se-user-name")));

        Assert.AreEqual("user", chromeDriver.FindElement(By.Id("se-user-name")).Text);
    }
}