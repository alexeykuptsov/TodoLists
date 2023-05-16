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
            return documentReadyState == "complete";
        });
    }
    
    public void RefreshPage()
    {
        Browser.Driver.Url = Browser.Driver.Url;
        WaitUntilLoaded();
    }
}