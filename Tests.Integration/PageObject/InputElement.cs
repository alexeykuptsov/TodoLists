using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

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