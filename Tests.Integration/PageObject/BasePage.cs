using OpenQA.Selenium;
using TodoLists.Utils;

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
}

public class BaseElement
{
    public Browser Browser { get; }
    public List<By> WebElementLocatorsChain { get; }

    public BaseElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
    {
        Browser = browser;
        WebElementLocatorsChain = webElementLocatorsChain.ToList();
    }

    public IWebElement FindElementByChain()
    { 
        IWebElement? webElement = null;
        for (int i = 0; i < WebElementLocatorsChain.Count; i++)
        {
            webElement = i == 0 ?
                Browser.Driver.FindElement(WebElementLocatorsChain[i]) :
                webElement!.FindElement(WebElementLocatorsChain[i]);
        }
        Assertion.Assert(webElement != null, "webElement != null");
        return webElement!;
    }
}

public class InputElement : BaseElement
{
    public InputElement(Browser browser, IEnumerable<By> webElementLocatorsChain) : base(browser, webElementLocatorsChain)
    {
    }

    public string Text
    {
        get => throw new NotImplementedException();
        set => FindElementByChain().SendKeys(value);
    }
}

public class ButtonElement : BaseElement
{
    public ButtonElement(Browser browser, IEnumerable<By> webElementLocatorsChain) : base(browser, webElementLocatorsChain)
    {
    }

    public void Click()
    {
        FindElementByChain().Click();
    }
}