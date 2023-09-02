using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            
            mainPage.AddTodoItemNameTextBox.Text = "foo";
            mainPage.AddTodoItemButton.Click();
            browser.Wait.Until(_ => mainPage.TodoItemNames.Count == 1);
            
            Assert.That(mainPage.TodoItemNames, Is.EqualTo(new [] {"foo"}));
        }

        [Test]
        public async Task AddTest02()
        {
            var username = "kevin";
            var profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync(username);
            using var browser = new Browser();
            var mainPage = browser.OpenSiteAndLogin(profileName, username);
            
            mainPage.AddTodoItemNameTextBox.Text = "foo";
            mainPage.AddTodoItemButton.Click();
            browser.Wait.Until(_ => mainPage.TodoItemNames.Count == 1);
            mainPage.AddTodoItemNameTextBox.Text = "bar";
            mainPage.AddTodoItemButton.Click();
            browser.Wait.Until(_ => mainPage.TodoItemNames.Count == 2);
            
            Assert.That(mainPage.TodoItemNames, Is.EqualTo(new [] {"foo", "bar"}));
        }

        [Test]
        public async Task UpdateTest01()
        {
            var username = "kevin";
            var profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync(username);
            using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(profileName, username);
            var projectId = await GetInboxProjectId(httpClient);
            await TestDataBuilder.CreateTodoItemAsync(projectId, "foo", false, httpClient);
            using var browser = new Browser();
            var mainPage = browser.OpenSiteAndLogin(profileName, username);
            browser.Wait.Until(_ => mainPage.TodoItemsDataGrid.Rows.Count == 1);

            mainPage.TodoItemsDataGrid.Rows[0].Cells[0].AsCheckBox().Click();
            mainPage.RefreshPage();

            browser.WaitAndAssertThat(() => mainPage.TodoItemsDataGrid.Rows[0].Cells[0].AsCheckBox().Checked, Is.True);
        }

        [Test]
        public async Task UpdateTest02()
        {
            var username = "kevin";
            var profileName = await TestDataBuilder.CreateProfileWithSingleUserAsync(username);
            using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(profileName, username);
            var projectId = await GetInboxProjectId(httpClient);
            await TestDataBuilder.CreateTodoItemAsync(projectId, "foo", false, httpClient);
            using var browser = new Browser();
            var mainPage = browser.OpenSiteAndLogin(profileName, username);
            browser.Wait.Until(_ => mainPage.TodoItemsDataGrid.Rows.Count == 1);
            
            var row = mainPage.TodoItemsDataGrid.Rows[0];
            row.Cells[1].Click();
            browser.Wait.Until(_ => mainPage.TodoItemsDataGrid.Rows[0].Cells[1].AsTextBox());
            mainPage.TodoItemsDataGrid.Rows[0].Cells[1].AsTextBox().Text = "bar";
            mainPage.RefreshPage();
            browser.Wait.Until(_ => mainPage.TodoItemsDataGrid.Rows.Count == 1);
                
            Assert.That(mainPage.TodoItemsDataGrid.Rows[0].Cells[1].Text, Is.EqualTo("bar"));
        }

        private async Task<long> GetInboxProjectId(HttpClient httpClient)
        {
            var response = await httpClient.GetAsync("api/Projects?projectName=Inbox");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JArray.Parse(responseContent)[0]["id"]!.Value<long>();
        }
    }
}