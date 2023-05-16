using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

public class CheckBoxElement : BaseElement
{
    public CheckBoxElement(Browser browser, IEnumerable<By> webElementLocatorsChain) : base(browser, webElementLocatorsChain)
    {
    }

    public bool Checked
    {
        get
        {
            return FindElementByChain().GetAttribute("aria-checked") == "true";
        }
    }
}