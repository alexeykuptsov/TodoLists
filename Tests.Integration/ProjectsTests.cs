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
  }
}