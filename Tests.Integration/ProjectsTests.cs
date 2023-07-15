using TodoLists.Tests.Integration.Arranging;
using TodoLists.Tests.Integration.PageObject;

namespace TodoLists.Tests.Integration;

public class ProjectsTests
{
  [Test]
  public async Task NewProfile01()
  {
    var username = "kevin";
    var profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync(username);
    using var browser = new Browser();
    var mainPage = browser.OpenSiteAndLogin(profileName, username);

    // Do nothing
            
    browser.Wait.Until(_ => mainPage.ProjectNameLabel.Text == "Inbox");
    browser.Wait.Until(_ => mainPage.TodoItemsDataGrid.Rows.Count == 0);
  }
  
  [Test]
  public async Task Delete01()
  {
    var username = "kevin";
    var profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync(username);
    using var browser = new Browser();
    var mainPage = browser.OpenSiteAndLogin(profileName, username);
  
    // mainPage.ProjectsDataGrid.Rows[0].DeleteButton.Click();
            
    var expectedErrors = new[]
    {
      "It is impossible to delete the last project. There should be at least one.",
    };
    browser.WaitAndAssertThat(() => mainPage.ErrorMessages, Is.EquivalentTo(expectedErrors));
  }
}