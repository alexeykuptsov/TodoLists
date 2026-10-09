# Integration Testing Guide

All integration tests are Selenium UI tests. There are no unit tests and no mocks — tests hit a real running app and a real database.

## Project layout

```
IntegrationTests/Tests.Integration/
├── Arranging/
│   └── TestDataBuilder.cs        # HTTP helpers to set up test data via API
├── PageObject/
│   ├── Browser.cs                # WebDriver wrapper + WaitAndAssertThat
│   ├── MainPage.cs               # Page object for the main Tasks page
│   ├── Elements/                 # Reusable element wrappers
│   │   ├── DataGridElement.cs
│   │   ├── DataGridRowElement.cs
│   │   ├── ButtonElement.cs
│   │   ├── TextBoxElement.cs
│   │   ├── LabelElement.cs
│   │   └── Dialogs/
│   │       ├── DeleteDialogElement.cs
│   │       └── NewProjectDialogElement.cs
├── Tests/
│   └── MainPage/
│       ├── ProjectsTests.cs
│       ├── ProjectRenameTests.cs
│       ├── ProjectDragAndDropTests.cs
│       ├── TodoItemsTests.cs
│       └── TodoItemRenameTests.cs
└── TestsDecorators.cs            # Test lifecycle wrapper
```

## How a test is structured

Every test calls `TestsDecorators.Default(...)`. It:
1. Creates a fresh isolated profile via super-user API
2. Optionally runs `SetUpAsync` (arrange test data via HTTP)
3. Opens the app in Chrome and logs in as `kevin` in that profile
4. Runs the test body
5. Disposes the browser

```csharp
[Test]
public async Task Example()
{
    await TestsDecorators.Default(new TestDecoratorOptions<PageObject.MainPage>
    {
        SetUpAsync = async tsc =>
        {
            using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(
                tsc.ProfileName, TestDataBuilder.DefaultUserName);
            tsc.CompositeDisposable.Add(httpClient);
            await TestDataBuilder.CreateProjectAsync("Foo", httpClient);
        },
        Test = tc =>
        {
            // tc.Page is the MainPage page object
            // tc.Browser.Wait is a WebDriverWait
            tc.Browser.WaitAndAssertThat(
                () => tc.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text),
                Is.EqualTo(new[] { "Inbox", "Foo" }));
        },
    });
}
```

For simple tests with no setup, use the shorthand:
```csharp
await TestsDecorators.Default(tc => { /* test body */ });
```

## TestDataBuilder helpers

All helpers take an authenticated `HttpClient` from `CreateHttpClientAndAuthenticateAsync`.

| Method | Purpose |
|--------|---------|
| `CreateProjectAsync(name, client)` | Creates a project, returns its `long` ID |
| `CreateTodoItemAsync(projectId, name, isComplete, client)` | Creates a to-do item |
| `RenameProjectAsync(source, target, client)` | Renames a project by name |
| `RenameTodoItemAsync(projectId, source, target, client)` | Renames a to-do item |
| `CreateProjectTemplateAsync(name, client)` | Creates a template, returns its `long` ID |
| `AddProjectTemplateTaskAsync(templateId, name, client)` | Adds a task to a template |

## Page Object pattern

Page objects wrap DOM elements using a **locator chain** — an ordered list of `By` selectors that are resolved from root → parent → element at interaction time. This avoids stale element references.

### Element types

| Class | Wraps |
|-------|-------|
| `ButtonElement` | Clickable buttons |
| `TextBoxElement` | `<input>` fields; `.Text = "value"` calls `Clear()` + `SendKeys(value)` + `Enter` |
| `LabelElement` | Read-only text; `.Text` returns `element.Text` |
| `DataGridElement` | Table; has `.Rows`, `.AddRowButton`, `.CloneButton`, `.TextEditor` |
| `DataGridRowElement` | A table row; `.Cells[n]`, `.DeleteButton`, `.Click()`, drag helpers |
| `DeleteDialogElement` | Confirmation dialog; `.Message.Text`, `.YesButton` |
| `NewProjectDialogElement` | New project modal; `.ProjectNameInput`, `.SubmitButton`, `.SelectTemplate(name)` |

### CSS class conventions

All Selenium-targeted elements must have a `se-*` CSS class. Style classes are separate — never use style classes as test selectors.

| `se-*` class | Element |
|---|---|
| `se-projects-data-grid` | Projects DataTable container |
| `se-todo-items-data-grid` | Todo items DataTable container |
| `se-data-row` | Data rows inside DataTables |
| `se-add-row-button` | "+" / "New project" button in a grid |
| `se-clone-button` | Clone project button |
| `se-delete-item-button` | Delete button on a row |
| `se-text-editor-input` | Inline text editor input |
| `se-new-project-dialog` | New project modal root |
| `se-new-project-name-input` | Name input inside new project modal |
| `se-template-select` | Template dropdown inside new project modal |
| `se-new-project-submit-button` | Submit button in new project modal |
| `se-dialog` | Generic delete confirmation dialog |
| `se-toast-message` | Toast notification (error messages) |
| `se-add-todo-item-button` | Add todo item button |

### Interacting with PrimeVue Select (dropdown)

PrimeVue renders dropdown options in a portal appended to `<body>`, outside the dialog DOM. Always locate options from the root driver:

```csharp
element.FindElement(By.CssSelector(".se-template-select")).Click();
Browser.Wait.Until(d =>
{
    var options = d.FindElements(By.CssSelector(".p-select-option")); // from driver root
    var option = options.FirstOrDefault(o => o.Text == name);
    if (option == null || !option.Displayed) return false;
    option.Click();
    return true;
});
```

## Adding a new test

1. Add `se-*` classes to any new HTML elements you need to target.
2. Add element properties to the relevant page object (e.g., `MainPage.cs`) using the locator chain pattern.
3. If you need to create test data programmatically, add a method to `TestDataBuilder.cs`.
4. Write the test using `TestsDecorators.Default(...)`.
5. Use `tc.Browser.WaitAndAssertThat(() => <getter>, Is.<constraint>)` instead of bare `Assert.That` — it retries until the condition passes or times out.

## Adding a new Page Object

For a new page (e.g., a Settings page):
1. Create `PageObject/SettingsPage.cs` extending `BasePage`.
2. Define element properties in the constructor using `By.CssSelector(".se-*")`.
3. Override `WaitUntilLoaded()` to wait for a key element to appear.
4. Expose it from `Browser` or navigate to it via an existing page object.
