using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

public class BasePage
{
    public Browser Browser { get; }

    public BasePage(Browser browser)
    {
        Browser = browser;
    }

    public virtual void WaitUntilLoaded()
    {
        Browser.Wait.Until(d =>
        {
            var documentReadyState = (string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState");
            string ajaxReadyState;
            try
            {
                ajaxReadyState = d.FindElement(By.Id("se-ajax-load-status")).GetAttribute("textContent");
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
            return documentReadyState == "complete" && ajaxReadyState == "complete";
        });
    }
    
    public void Refresh()
    {
        Thread.Sleep(200); // To execute HTTP requests from event handlers like button clicks
        Browser.Driver.Url = Browser.Driver.Url;
        WaitUntilLoaded();
    }
}