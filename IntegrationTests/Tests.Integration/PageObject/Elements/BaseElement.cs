using System.Collections.ObjectModel;
using OpenQA.Selenium;
using TodoLists.Utils;

namespace TodoLists.Tests.Integration.PageObject.Elements;

public class BaseElement
{
    public Browser Browser { get; }
    public List<By> WebElementLocatorsChain { get; }

    public virtual bool Displayed => FindElementByChain().Displayed;

    public virtual bool Enabled => FindElementByChain().Enabled;

    public BaseElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
    {
        Browser = browser;
        WebElementLocatorsChain = webElementLocatorsChain.ToList();
    }

    public IWebElement FindElementByChain()
    { 
        return FindElementByChain(WebElementLocatorsChain);
    }

    public IWebElement FindElementByChain(List<By> webElementLocatorsChain)
    { 
        Assertion.Assert(webElementLocatorsChain.Count > 0, "webElementLocatorsChain.Count > 0");
        IWebElement? webElement = null;
        for (int i = 0; i < webElementLocatorsChain.Count; i++)
        {
            webElement = i == 0 ?
                Browser.Driver.FindElement(webElementLocatorsChain[i]) :
                webElement!.FindElement(webElementLocatorsChain[i]);
        }
        Assertion.Assert(webElement != null, "webElement != null");
        return webElement!;
    }

    public ReadOnlyCollection<IWebElement> FindElementsByChain(List<By> webElementLocatorsChain)
    { 
        Assertion.Assert(webElementLocatorsChain.Count > 0, "webElementLocatorsChain.Count > 0");
        IWebElement? webElement = null;
        for (int i = 0; i < webElementLocatorsChain.Count - 1; i++)
        {
            webElement = i == 0 ?
                Browser.Driver.FindElement(webElementLocatorsChain[i]) :
                webElement!.FindElement(webElementLocatorsChain[i]);
        }

        if (webElementLocatorsChain.Count == 1)
            return Browser.Driver.FindElements(webElementLocatorsChain[^1]);
        
        Assertion.Assert(webElement != null, "webElement != null");
        return webElement!.FindElements(webElementLocatorsChain[^1]);
    }
}