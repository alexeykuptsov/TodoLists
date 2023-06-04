using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

public class LabelElement : BaseElement
{
  public LabelElement(Browser browser, IEnumerable<By> webElementLocatorsChain) : base(browser, webElementLocatorsChain)
  {
  }

  public string Text => FindElementByChain().Text;
}