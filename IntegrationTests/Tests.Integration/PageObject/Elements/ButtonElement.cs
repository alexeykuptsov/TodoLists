using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject.Elements;

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