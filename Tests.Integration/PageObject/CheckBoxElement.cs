using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

public class CheckBoxElement : BaseElement
{
    public CheckBoxElement(Browser browser, IEnumerable<By> webElementLocatorsChain) : base(browser, webElementLocatorsChain)
    {
    }

    public bool Checked => FindElementByChain().GetAttribute("aria-checked") == "true";

    public void Click()
    {
        FindElementByChain().Click();
    }
}