using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject.Elements.Dialogs;

public class NewProjectDialogElement : BaseElement
{
    public ButtonElement SubmitButton { get; }
    public TextBoxElement ProjectNameInput { get; }

    public NewProjectDialogElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
        : base(browser, webElementLocatorsChain)
    {
        SubmitButton = new ButtonElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".se-new-project-submit-button")));
        ProjectNameInput = new TextBoxElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".se-new-project-name-input")));
    }

    public override bool Displayed
    {
        get
        {
            try
            {
                return FindElementByChain().Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }

    public void SelectTemplate(string templateName)
    {
        FindElementByChain().FindElement(By.CssSelector(".se-template-select")).Click();
        Browser.Wait.Until(d =>
        {
            var options = d.FindElements(By.CssSelector(".p-select-option"));
            var option = options.FirstOrDefault(o => o.Text == templateName);
            if (option == null || !option.Displayed) return false;
            option.Click();
            return true;
        });
    }
}
