using NUnit.Framework.Constraints;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using TodoLists.Tests.Integration.Utils;
using TodoLists.Tests.Integration.Utils.NUnit;

namespace TodoLists.Tests.Integration.PageObject;

public sealed class Browser : IDisposable
{
    public ChromeDriver Driver { get; }
    public WebDriverWait Wait { get; }

    public Browser()
    {
        Driver = ChromeDriverFactory.CreateChromeDriver();
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10))
        {
            PollingInterval = TimeSpan.FromMilliseconds(300),
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

        Wait.Until(d => d.FindElement(By.Name("profile")).Displayed);

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

    public void DragAndDrop(IWebElement sourceElement, IWebElement targetElement)
    {
        var actions = new Actions(Driver);
        actions.DragAndDrop(sourceElement, targetElement).Perform();
        
        // Wait a bit for the drag operation to complete
        Thread.Sleep(500);
    }

    public void DragAndDropByOffset(IWebElement sourceElement, int xOffset, int yOffset)
    {
        var actions = new Actions(Driver);
        actions.DragAndDropToOffset(sourceElement, xOffset, yOffset).Perform();
        
        // Wait a bit for the drag operation to complete
        Thread.Sleep(500);
    }

    public void DragAndDropWithActions(IWebElement sourceElement, IWebElement targetElement)
    {
        var actions = new Actions(Driver);
        actions.ClickAndHold(sourceElement)
               .MoveToElement(targetElement)
               .Release()
               .Perform();
        
        // Wait a bit for the drag operation to complete
        Thread.Sleep(500);
    }
}