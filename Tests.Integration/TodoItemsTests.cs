using TodoLists.Tests.Integration.Arranging;
using TodoLists.Tests.Integration.PageObject;

namespace TodoLists.Tests.Integration
{
    public class TodoItemsTests
    {
        [Test]
        public async Task AddTest01()
        {
            var username = "kevin";
            var profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync(username);
            using var browser = new Browser();
            var mainPage = browser.OpenSiteAndLogin(profileName, username);
            
            mainPage.AddTodoItemNameInput.Text = "foo";
            mainPage.AddTodoItemButton.Click();
            browser.Wait.Until(_ => mainPage.TodoItemNames.Count == 1);
            
            CollectionAssert.AreEqual(new [] {"foo"}, mainPage.TodoItemNames);
        }

        [Test]
        public async Task AddTest02()
        {
            var username = "kevin";
            var profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync(username);
            using var browser = new Browser();
            var mainPage = browser.OpenSiteAndLogin(profileName, username);
            
            mainPage.AddTodoItemNameInput.Text = "foo";
            mainPage.AddTodoItemButton.Click();
            browser.Wait.Until(_ => mainPage.TodoItemNames.Count == 1);
            mainPage.AddTodoItemNameInput.Text = "bar";
            mainPage.AddTodoItemButton.Click();
            browser.Wait.Until(_ => mainPage.TodoItemNames.Count == 2);
            
            CollectionAssert.AreEqual(new [] {"foo", "bar"}, mainPage.TodoItemNames);
        }

        [Test]
        public async Task UpdateSummaryTest01()
        {
            var username = "kevin";
            var profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync(username);
            using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(profileName, username);
            await TestDataBuilder.CreateTodoItemAsync("foo", false, httpClient);
            using var browser = new Browser();
            var mainPage = browser.OpenSiteAndLogin(profileName, username);

            mainPage.TodoItemsDataGrid.Rows[0].Cells[0].Click();
            mainPage.RefreshPage();
            await Task.Delay(TimeSpan.FromSeconds(1));
                
            Assert.IsTrue(mainPage.TodoItemsDataGrid.Rows[0].Cells[0].AsCheckBox().Checked);
        }
    }
}