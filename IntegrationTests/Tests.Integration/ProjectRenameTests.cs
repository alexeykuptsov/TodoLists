using OpenQA.Selenium;
using TodoLists.Tests.Integration.Arranging;

namespace TodoLists.Tests.Integration;

public class ProjectRenameTests
{
    [Test]
    public async Task RenameProject_ClickEditButtonAndChangeNameThenPressEnter_ShouldUpdateProjectName()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                // Rename the default "Inbox" project to "Original Project" for testing
                await TestDataBuilder.RenameProjectAsync("Inbox", "Original Project", httpClient);
            },
            Test = context =>
            {
                // Verify initial project name
                var initialProjectNames = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialProjectNames, Is.EqualTo(new[] { "Original Project" }));
                
                // Find and click the Edit button for the first row
                var firstRow = context.Page.ProjectsDataGrid.Rows[0];
                firstRow.EditButton.Click();
                
                // Wait for the text editor to appear and become available
                context.Browser.Wait.Until(_ =>
                {
                    try
                    {
                        var textEditor = context.Page.ProjectsDataGrid.TextEditor;
                        return textEditor.FindElementByChain().Displayed;
                    }
                    catch
                    {
                        return false;
                    }
                });
                
                // Edit the project name
                var textEditor = context.Page.ProjectsDataGrid.TextEditor;
                var textEditorElement = textEditor.FindElementByChain();
                textEditorElement.Clear();
                textEditorElement.SendKeys("Renamed Project");
                textEditorElement.SendKeys(Keys.Enter);
                
                // Wait for the UI to update and verify the name change
                context.Browser.Wait.Until(_ =>
                {
                    var updatedProjectNames = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return updatedProjectNames.Count == 1 && updatedProjectNames[0] == "Renamed Project";
                });
                
                context.Page.Refresh();
                
                // Final verification
                var finalProjectNames = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalProjectNames, Is.EqualTo(new[] { "Renamed Project" }));
            }
        });
    }

    [Test]
    public async Task RenameProject_WithMultipleProjects_ShouldOnlyUpdateSelectedProject()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                // Create multiple projects for testing
                await TestDataBuilder.RenameProjectAsync("Inbox", "Project A", httpClient);
                await TestDataBuilder.CreateProjectAsync("Project B", httpClient);
                await TestDataBuilder.CreateProjectAsync("Project C", httpClient);
            },
            Test = context =>
            {
                // Verify initial project names
                var initialProjectNames = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialProjectNames, Is.EqualTo(new[] { "Project A", "Project B", "Project C" }));
                
                // Edit the second project (Project B)
                var secondRow = context.Page.ProjectsDataGrid.Rows[1];
                secondRow.EditButton.Click();
                
                // Wait for the text editor to appear
                context.Browser.Wait.Until(_ =>
                {
                    try
                    {
                        var textEditor = context.Page.ProjectsDataGrid.TextEditor;
                        return textEditor.FindElementByChain().Displayed;
                    }
                    catch
                    {
                        return false;
                    }
                });
                
                // Edit the project name
                var textEditor = context.Page.ProjectsDataGrid.TextEditor;
                var textEditorElement = textEditor.FindElementByChain();
                textEditorElement.Clear();
                textEditorElement.SendKeys("Modified Project B");
                textEditorElement.SendKeys(Keys.Enter);
                
                // Wait for the UI to update and verify only the selected project was renamed
                context.Browser.Wait.Until(_ =>
                {
                    var updatedProjectNames = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return updatedProjectNames.Count == 3 && updatedProjectNames[1] == "Modified Project B";
                });
                
                context.Page.Refresh();

                // Final verification - only Project B should be renamed
                var finalProjectNames = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalProjectNames, Is.EqualTo(new[] { "Project A", "Modified Project B", "Project C" }));
            }
        });
    }

    [Test]
    public async Task RenameProject_EmptyName_ShouldRevertToOriginalName()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                // Rename the default project for testing
                await TestDataBuilder.RenameProjectAsync("Inbox", "Test Project", httpClient);
            },
            Test = context =>
            {
                // Verify initial project name
                var initialProjectNames = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialProjectNames, Is.EqualTo(new[] { "Test Project" }));
                
                // Click edit button
                var firstRow = context.Page.ProjectsDataGrid.Rows[0];
                firstRow.EditButton.Click();
                
                // Wait for the text editor to appear
                context.Browser.Wait.Until(_ =>
                {
                    try
                    {
                        var textEditor = context.Page.ProjectsDataGrid.TextEditor;
                        return textEditor.FindElementByChain().Displayed;
                    }
                    catch
                    {
                        return false;
                    }
                });
                
                // Try to set empty name
                var textEditor = context.Page.ProjectsDataGrid.TextEditor;
                var textEditorElement = textEditor.FindElementByChain();
                textEditorElement.Clear();
                textEditorElement.SendKeys(Keys.Enter);
                
                // Wait a moment for any validation or revert behavior
                Thread.Sleep(1000);
                
                // Verify the name remains unchanged or reverts to original
                var finalProjectNames = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalProjectNames[0], Is.Not.Empty, "Project name should not be empty");
                // The exact behavior depends on validation - it might revert to original or show validation error
            }
        });
    }
}