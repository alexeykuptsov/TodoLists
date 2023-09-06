using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject.Elements;

public class TextBoxElement : BaseElement
{
    public TextBoxElement(Browser browser, IEnumerable<By> webElementLocatorsChain) : base(browser, webElementLocatorsChain)
    {
    }

    public string Text
    {
        get => FindElementByChain().Text;
        set
        {
            var element = FindElementByChain();
            element.Clear();
            element.SendKeys(value);
            element.SendKeys(Keys.Enter);
        }
    }

    public void SendKeys(string text)
    {
        FindElementByChain().SendKeys(text);
    }
}