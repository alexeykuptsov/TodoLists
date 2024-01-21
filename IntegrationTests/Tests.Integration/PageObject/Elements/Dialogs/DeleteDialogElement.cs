using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject.Elements.Dialogs;

public class DeleteDialogElement: BaseElement
{
    public ButtonElement YesButton { get; }

    public LabelElement Message { get; }

    public override bool Displayed
    {
        get
        {
            var dialogElement = FindElementByChain();
            if (!dialogElement.Displayed)
                return false;
            var messageElements = dialogElement.FindElements(By.CssSelector(".dx-dialog-message"));
            return messageElements.Count == 1 && messageElements[0].Text.StartsWith("Do you really want to delete");
        }
    }

    public DeleteDialogElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
        : base(browser, webElementLocatorsChain)
    {
        YesButton = new ButtonElement(Browser, WebElementLocatorsChain.Append(By.CssSelector("[aria-label=\"Yes\"].dx-button")));
        Message = new LabelElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".dx-dialog-message")));
    }
}