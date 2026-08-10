# T-3: Project templates for pre-populated todo lists

## Summary
Let a user create a new project from a pre-defined **template** that pre-populates todo items, with an initial built-in "Shopping list" template (`Milk`, `Eggs`, `Bread`).

## Context / Background
Today a new project is always created empty (see the `POST /api/Projects` endpoint in `TECHNICAL_SPECIFICATION.md` §4.2 and the "new project" flow in `App/vue/src/components/MainPage/ProjectsPanel.vue`). Our persona *Kevin* (see `README.md`) repeatedly creates the same kinds of lists — shopping, packing, weekly chores — and each time he re-types the same items. Templates remove that friction.

The first template ships with the code: **"Shopping list"** with items `Milk`, `Eggs`, `Bread` (in that order). Future templates should be easy to add. This is independent of [[T-1]] (UI library migration) and [[T-2]] (.NET upgrade), though if T-1 lands first the picker UI will be built in the new component library.

## Acceptance Criteria
- [ ] At least one built-in template exists: **"Shopping list"** with items `Milk`, `Eggs`, `Bread` in that exact order.
- [ ] The web UI has a "New from template" flow: user picks a template from a list, names the project, and receives a new project pre-populated with the template's items.
- [ ] The existing "new empty project" flow still works and is still reachable from the same place in the UI.
- [ ] Backend API supports creating a project from a template (either via an extended `POST /api/Projects` or a dedicated endpoint — see decision below).
- [ ] Integration tests in `Tests.Integration` cover: creating a project from the Shopping list template, and verifying the project exists with items `Milk`, `Eggs`, `Bread` in that order.
- [ ] Template-created items are indistinguishable from manually-added items after creation (same rename/delete/complete flows work on them with no special-casing).

## Implementation Notes / Decisions

### Where templates live — three options
- **Hardcoded seed in C#** — a `TemplateCatalog` static class with a list of `Template { Id, Name, Items[] }`.
  - Pros: zero infrastructure; trivially unit-testable; templates are version-controlled with the code they support.
  - Cons: adding a template requires a code change and rebuild.
- **Database table** (`Templates`, `TemplateItems`) seeded via EF migration.
  - Pros: templates could later be edited at runtime by an admin.
  - Cons: adds two entities, a migration, and admin UI surface area for a feature that has no runtime-editing requirement yet.
- **JSON resource file** (e.g. `App/Resources/templates.json`) loaded at startup.
  - Pros: edit templates without recompiling; still fully static per deployment.
  - Cons: another file format to maintain; adds a small runtime loader; less type safety than the C# option.

**Recommendation: hardcoded seed in C#.** No runtime-editing requirement exists, the template count is tiny, and the C# option is the smallest possible change consistent with the acceptance criteria. Revisit if runtime-editable templates are ever required.

### API shape — two options
- **Extend `POST /api/Projects`** with an optional `templateId` field. When present, the server clones the template's items into the new project.
  - Pros: one endpoint for creation; keeps the surface area small; the "empty project" path is `templateId == null`.
  - Cons: a single endpoint now has two modes; slightly harder to swagger-document cleanly.
- **Dedicated `POST /api/Projects/FromTemplate`** with `{ name, templateId }`.
  - Pros: unambiguous; each endpoint has one job; parallels the existing `POST /api/Projects/Clone`.
  - Cons: two endpoints to keep in sync as the project-creation flow evolves.

**Recommendation: dedicated `POST /api/Projects/FromTemplate`**, mirroring the existing `POST /api/Projects/Clone` pattern (§4.2 of the spec). Keeps `POST /api/Projects` unchanged and preserves the "empty project" flow untouched, which the acceptance criteria explicitly requires. Also add `GET /api/Templates` returning the list of `{ id, name }` for the picker.

### Interaction with existing rename/delete flows
Template-created items must be plain `TodoItem` rows with no template linkage — the template is a factory, not an ongoing relationship. That means:
- No `TemplateId` foreign key on `TodoItem`.
- After creation, `PATCH /api/TodoItems` (rename, complete, delete) works exactly as today.
- Cloning a template-created project via `POST /api/Projects/Clone` also works with no changes.

### Vue UI — where the picker fits
- **Recommended: a small modal on the existing "new project" action.** The user clicks "New project" → a modal appears with a template dropdown (default: "Empty") and a project-name input. Submit creates via `POST /api/Projects` (Empty) or `POST /api/Projects/FromTemplate` (any other choice). This keeps the flow one click away from where it is today, satisfies the "empty project flow still reachable" criterion, and avoids a routed page for a small feature.
- Alternative considered: a dedicated `/new-project` page. Rejected because it adds routing/navigation for what is essentially a two-field form.

### Files/areas likely to touch
- Backend:
  - `App/Templates/` (new folder) — `Template.cs`, `TemplateCatalog.cs` (static list of built-in templates).
  - `App/Controllers/TemplatesController.cs` (new) — `GET /api/Templates`.
  - `App/Controllers/ProjectsController.cs` — add `POST /api/Projects/FromTemplate`.
  - `App/Dtos/` — `TemplateDto`, `CreateProjectFromTemplateRequest`.
- Frontend:
  - `App/vue/src/components/MainPage/ProjectsPanel.vue` — wire "New project" button to a modal component.
  - `App/vue/src/components/MainPage/NewProjectModal.vue` (new) — dropdown of templates + name input + submit.
  - `App/vue/src/utils/` — add a `templates` fetch helper.
- Tests:
  - `IntegrationTests/Tests.Integration/` — a new test that logs in, creates a project from "Shopping list", then asserts the project appears with items `Milk`, `Eggs`, `Bread` in that order.

---

## Prompt (paste into a clean Claude context)

````
You are working on the To-Do Lists project at C:\Code\ak\TodoLists on Windows. Use PowerShell for all shell commands (no `&&`, no Bash-isms). Start by reading `AGENTS.md`, `README.md`, and `TECHNICAL_SPECIFICATION.md` to load project context.

## Task
Add a "create project from template" feature. Ship one built-in template — "Shopping list" with items `Milk`, `Eggs`, `Bread` in that order — and a Vue UI + backend API to create a new project pre-populated from a chosen template. The existing empty-project flow must still work.

## Context you need
- Current project creation: `POST /api/Projects` (see TECHNICAL_SPECIFICATION.md §4.2, and `App/Controllers/ProjectsController.cs`). The Vue flow lives in `App/vue/src/components/MainPage/ProjectsPanel.vue`.
- The related `POST /api/Projects/Clone` endpoint already exists — mirror its style for the new endpoint.
- Persona: Kevin (see README) — a local Windows user; the feature is aimed at avoiding retyping the same list of items repeatedly.

## Design decisions (already made — implement as specified)
1. **Where templates live**: hardcoded seed in C#. Create `App/Templates/TemplateCatalog.cs` with a static list of `Template { Id, Name, Items: string[] }`. No DB table, no JSON file. Rationale: no runtime-editing requirement; smallest change that satisfies the AC.
2. **API shape**: a dedicated endpoint `POST /api/Projects/FromTemplate` with body `{ name: string, templateId: string }`, returning the created project (or its id) in the same shape as `POST /api/Projects` today. Also add `GET /api/Templates` returning `[{ id, name }]` for the picker. Leave `POST /api/Projects` unchanged so the empty-project path is untouched.
3. **Template-created items are plain `TodoItem` rows** — no `TemplateId` foreign key. After creation, rename/complete/delete work exactly as today. `POST /api/Projects/Clone` must continue to work on template-created projects without special-casing.
4. **UI**: on the existing "New project" action in `ProjectsPanel.vue`, open a modal (`NewProjectModal.vue`) with a template dropdown defaulting to "Empty" and a project-name input. "Empty" calls the existing `POST /api/Projects`; any other choice calls `POST /api/Projects/FromTemplate`.

## Definition of done — all must hold
- Built-in template "Shopping list" exists with items `Milk`, `Eggs`, `Bread` in that exact order.
- `GET /api/Templates` returns at least that template.
- `POST /api/Projects/FromTemplate` creates a project owned by the current profile, pre-populated with the template's items in order.
- Vue UI: clicking "New project" opens a modal; picking "Empty" → creates an empty project; picking "Shopping list" and entering a name → creates a project with the three items.
- The existing empty-project flow is still reachable and still works (the "Empty" option in the modal counts).
- New integration test in `Tests.Integration` covers: create-from-template flow → assert project exists with items `Milk`, `Eggs`, `Bread` in that order.
- Full test suite passes:
  ```powershell
  cd IntegrationTests\Tests.Integration
  dotnet test
  ```
  (with the web app running per README).
- `dotnet build` succeeds; `npm run build` in `App\vue` succeeds.

## Report back
- List of files added and changed with a one-line description each.
- Output of `dotnet build`, `npm run build`, and `dotnet test`.
- Confirmation that the "empty project" flow is still reachable from the UI (screenshot or textual walk-through).
- Any deviations from the design decisions above, with justification.
````
