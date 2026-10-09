# T-4: Rename ProjectTemplateItem to ProjectTemplateTask

## Summary
Rename the `ProjectTemplateItem` concept to `ProjectTemplateTask` everywhere — C# class, DTO, controller, DbSet, DB table, navigation properties, migration, and Vue frontend API calls.

## Context / Background
During the implementation of [[T-3]] the type was named `ProjectTemplateItem`, but `ProjectTemplateTask` is more consistent with the domain language (the things inside a project are called "todo items" / tasks, not "items"). This is a pure rename — no logic changes.

The migration (`20261004214852_AddProjectTemplates`) was created in T-3 and its files are currently untracked, so the approach is to delete those files, do all renames, then regenerate the migration from scratch so the new table name (`project_template_tasks`) is in the initial migration rather than a separate rename migration.

## Acceptance Criteria
- [ ] The class `ProjectTemplateItem` is renamed to `ProjectTemplateTask` and its file renamed to `ProjectTemplateTask.cs`.
- [ ] The DTO `ProjectTemplateItemDto` is renamed to `ProjectTemplateTaskDto` and its file renamed to `ProjectTemplateTaskDto.cs`.
- [ ] The controller `ProjectTemplateItemsController` is renamed to `ProjectTemplateTasksController` and its file renamed to `ProjectTemplateTasksController.cs`. The route becomes `api/project-template-tasks` automatically via `[Route("api/[controller]")]`.
- [ ] The `DbSet` property in `TodoListsDbContext` is renamed from `ProjectTemplateItems` to `ProjectTemplateTasks`.
- [ ] The navigation property `Items` on `ProjectTemplate` (if present) is renamed to `Tasks`.
- [ ] The old migration files (`20261004214852_AddProjectTemplates.cs`, `20261004214852_AddProjectTemplates.Designer.cs`) are deleted and a fresh migration is generated. The new migration creates a table named `project_template_tasks` (not `project_template_items`).
- [ ] `TodoListsDbContextModelSnapshot.cs` reflects the new table/entity names.
- [ ] The Vue component `ProjectTemplatePage.vue` calls `api/project-template-tasks` instead of `api/project-template-items`.
- [ ] `TECHNICAL_SPECIFICATION.md` and `docs/testing-guide.md` are updated to use the new names where they reference `ProjectTemplateItem` / `project_template_items`.
- [ ] Integration tests in `TestDataBuilder.cs` and `ProjectsTests.cs` compile and pass with the new names.
- [ ] The application builds (`dotnet build`) and the integration test suite passes.

## Implementation Notes / Decisions
- Delete the two untracked migration files before renaming anything, to avoid EF Core confusion. After all renames, run `dotnet ef migrations add AddProjectTemplates` (reuse the same migration name) from the `App` directory.
- The controller uses `[Route("api/[controller]")]`, so renaming the class to `ProjectTemplateTasksController` automatically changes the route to `api/project-template-tasks` — no manual route attribute change needed.
- Do a global search for `ProjectTemplateItem` (case-sensitive) and `project_template_item` (snake_case) to catch every reference before declaring done.
- There is no need for a data migration since this is a fresh branch where the migration has never been applied to any shared database.

---

## Prompt (paste into a clean Claude context)

````
You are working on the TodoLists application (ASP.NET Core 10 backend + Vue 3 frontend, PostgreSQL via EF Core with snake_case naming).

## Goal
Rename `ProjectTemplateItem` → `ProjectTemplateTask` everywhere: C# entity, DTO, controller, DbSet, navigation properties, migration, and Vue frontend calls.

## Files affected (based on a global search for `ProjectTemplateItem`)
- `App/Entities/ProjectTemplateItem.cs` — rename class + file
- `App/Models/ProjectTemplateItemDto.cs` — rename class + file
- `App/Controllers/ProjectTemplateItemsController.cs` — rename class + file
- `App/Entities/ProjectTemplate.cs` — update navigation property if named `Items`
- `App/Entities/TodoListsDbContext.cs` — rename DbSet property `ProjectTemplateItems` → `ProjectTemplateTasks`
- `App/Migrations/20261004214852_AddProjectTemplates.cs` — DELETE this file
- `App/Migrations/20261004214852_AddProjectTemplates.Designer.cs` — DELETE this file
- `App/Migrations/TodoListsDbContextModelSnapshot.cs` — will be regenerated
- `App/vue/src/components/ProjectTemplatePage.vue` — update API call URL
- `TECHNICAL_SPECIFICATION.md` — update references
- `docs/testing-guide.md` — update references
- `IntegrationTests/Tests.Integration/Arranging/TestDataBuilder.cs` — update references
- `IntegrationTests/Tests.Integration/Tests/MainPage/ProjectsTests.cs` — update references

## Step-by-step plan

1. **Delete old migration files** (they are untracked):
   ```powershell
   Remove-Item App/Migrations/20261004214852_AddProjectTemplates.cs
   Remove-Item App/Migrations/20261004214852_AddProjectTemplates.Designer.cs
   ```

2. **Rename entity file and class**: rename `App/Entities/ProjectTemplateItem.cs` to `App/Entities/ProjectTemplateTask.cs`; change `public class ProjectTemplateItem` to `public class ProjectTemplateTask`.

3. **Rename DTO file and class**: rename `App/Models/ProjectTemplateItemDto.cs` to `App/Models/ProjectTemplateTaskDto.cs`; change `public class ProjectTemplateItemDto` to `public class ProjectTemplateTaskDto`.

4. **Rename controller file and class**: rename `App/Controllers/ProjectTemplateItemsController.cs` to `App/Controllers/ProjectTemplateTasksController.cs`; change the class name to `ProjectTemplateTasksController`; update all internal references from `ProjectTemplateItem`/`ProjectTemplateItemDto`/`ProjectTemplateItems` to their `Task` equivalents.

5. **Update `ProjectTemplate.cs`**: if there is a navigation property `public ICollection<ProjectTemplateItem> Items`, rename it to `public ICollection<ProjectTemplateTask> Tasks`.

6. **Update `TodoListsDbContext.cs`**: rename `DbSet<ProjectTemplateItem> ProjectTemplateItems` → `DbSet<ProjectTemplateTask> ProjectTemplateTasks`. Update any `OnModelCreating` references. EF Core snake_case naming convention will automatically map `ProjectTemplateTasks` → `project_template_tasks`.

7. **Update Vue frontend** (`App/vue/src/components/ProjectTemplatePage.vue`): change every occurrence of `project-template-items` in API URL strings to `project-template-tasks`.

8. **Update docs** (`TECHNICAL_SPECIFICATION.md`, `docs/testing-guide.md`): replace `ProjectTemplateItem` → `ProjectTemplateTask` and `project_template_items` → `project_template_tasks` (both the C# names and DB table names).

9. **Update integration tests** (`TestDataBuilder.cs`, `ProjectsTests.cs`): rename any references to `ProjectTemplateItem`/`ProjectTemplateItemDto`/`ProjectTemplateItems`.

10. **Regenerate migration** from the `App` directory:
    ```powershell
    dotnet ef migrations add AddProjectTemplates
    ```
    Confirm the generated migration creates a table named `project_template_tasks`.

11. **Build and verify**:
    ```powershell
    dotnet build
    ```
    Fix any remaining compile errors (do a global search for `ProjectTemplateItem` to find stragglers).

## Constraints
- The controller uses `[Route("api/[controller]")]`, so renaming the class automatically changes the HTTP route to `api/project-template-tasks`.
- No data migration is needed — this branch's migration has never been applied to a shared DB.
- Keep all existing behavior identical; this is a pure rename, no logic changes.
````
