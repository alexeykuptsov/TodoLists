# T-1: Migrate from DevExtreme to PrimeVue

## Summary
Remove the paid DevExtreme UI library from the Vue SPA at `App/vue` and replace it with PrimeVue (MIT-licensed). Keep all existing screens functionally equivalent. Also delete the leftover DevExtreme static assets under `wwwroot/lib/`.

## Context / Background
The frontend at `App/vue` currently uses DevExtreme (DevExpress) — see `App/vue/src/components/MainPanel.vue`, `App/vue/src/components/LoginButton.vue`, and `App/vue/src/components/MainPage/ProjectsPanel.vue`. DevExtreme is a paid, heavyweight component library. The project's stated purpose (see `README.md`) is practicing with programming languages, technologies and libraries. Both the license cost and the runtime/bundle weight of DevExtreme are pain points that block casual open-source use of the app.

The migration touches only the frontend. Backend contracts (see the endpoints in `TECHNICAL_SPECIFICATION.md` §4) do not change; the replacement library must adapt to the existing REST responses rather than reshaping them. Related work: [[T-2]] (updating .NET) is independent and can proceed in parallel.

Screens/components currently backed by DevExtreme:
- `MainPanel.vue` — DevExtreme `DataGrid` for todo items with inline editing.
- `ProjectsPanel.vue` — DevExtreme `DataGrid` with row drag-and-drop reordering, inline edit, delete, clone.
- `LoginButton.vue` — DevExtreme `Form` + `Popover` for profile/username/password entry, plus `notify` for user feedback.

## Acceptance Criteria
- [x] No `devextreme*` npm packages remain in `App/vue/package.json` (or `package-lock.json`).
- [x] Every screen that used a DevExtreme component renders equivalently with PrimeVue — todo items table with inline editing, projects table with drag-and-drop reordering + inline edit + delete + clone, and the login popover/form.
- [x] All existing integration tests in `Tests.Integration` pass locally with the web app running.
- [x] `App/vue` builds cleanly (`npm run build`) with no references to `devextreme`, `devextreme-vue`, or DevExtreme CSS bundles.
- [x] PrimeVue and its CSS are imported once in a central place (`main.js`); no leftover DevExtreme stylesheet imports.
- [x] `wwwroot/lib/dx-23.1.15/` directory is deleted.
- [x] `wwwroot/lib/` directory is deleted (it contains only DevExtreme assets).

## Implementation Notes / Decisions

### Library survey
Considered specifically for this project: Vue 3 SPA, tables + inline-editable rows for todo items and projects, row drag-and-drop, a small login form in a popover, dev-only auth, small surface area, hobby/practice project (bundle size and license friction matter more than enterprise support).

- **Vuetify 3** — Material Design system with a broad component set including data tables, forms, and dialogs.
  - Pros: mature, huge community, batteries-included theming, well-documented.
  - Cons: opinionated Material look; data table lacks first-class inline editing and row drag-and-drop out of the box (needs custom code or vuedraggable).
- **PrimeVue** — Component-rich library from PrimeTek.
  - Pros: `DataTable` supports cell/row edit modes and row reorder natively — closest feature parity with DevExtreme DataGrid; multiple themes; free MIT.
  - Cons: styling can feel busy without a theme pass; larger API surface to learn.
- **Quasar Framework** — Full app framework (not just components) built on Vue 3.
  - Pros: excellent components, Material-ish theming, strong CLI.
  - Cons: overkill for a plain Vue SPA — expects you to adopt Quasar's build/CLI stack; migration cost is higher than swapping component libraries.
- **Element Plus** — Vue 3 port of the popular Element UI.
  - Pros: clean desktop-oriented look that fits admin/data screens; solid `el-table` with editable slots.
  - Cons: row drag-and-drop needs an add-on (Sortable.js integration); Chinese-first docs occasionally lag English translations.
- **Naive UI** — Modern TypeScript-first Vue 3 library.
  - Pros: small, elegant, tree-shakeable; excellent DX; MIT.
  - Cons: `n-data-table` supports column drag but row drag-and-drop is not built in — reordering would need `vuedraggable`; smaller component ecosystem than Vuetify/PrimeVue.
- **Ant Design Vue** — Vue 3 port of Ant Design.
  - Pros: enterprise-grade table with editable cells; polished design system.
  - Cons: heavy bundle; row drag needs `vuedraggable` or manual wiring; icon set and locale handling add weight.

**Decision: PrimeVue 4.**

PrimeVue's `DataTable` supports cell-mode inline editing and row reorder natively, giving the closest feature parity with DevExtreme DataGrid. `Toast` replaces DevExtreme `notify`; `Dialog` replaces the delete-confirm dialog; `Popover` replaces the login popover. The library is MIT-licensed and actively maintained.

**Drag-and-drop:** PrimeVue's built-in `rowReorder` uses HTML5 drag-and-drop, which Selenium's `ClickAndHold` simulation cannot trigger. Therefore `vuedraggable` (SortableJS wrapper for Vue 3) is still added as an additional runtime dependency for row reordering in the projects grid, with `forceFallback: true` to force mouse-event drag.

**CSS selectors:** The integration test page objects use `dx-*` class names throughout. These are updated to `se-*` semantic classes. PrimeVue components expose a `pt` (passthrough) prop that injects arbitrary attributes onto their inner elements — use it to set `se-data-row` on `<tr>` rows, `se-toast-message` on toast content, `se-dialog` / `se-dialog-message` on the confirm dialog, etc.

### Files/areas to touch
- `App/vue/package.json` — remove `devextreme`, `devextreme-vue`; add `primevue`, `@primevue/themes`, `primeicons`, `vuedraggable`.
- `App/vue/src/main.js` — register PrimeVue plugin with Aura theme preset; register `ToastService`; remove DevExtreme imports.
- `App/vue/src/components/MainPanel.vue` — replace `DxDataGrid` with PrimeVue `DataTable` (cell edit mode).
- `App/vue/src/components/MainPage/ProjectsPanel.vue` — replace `DxDataGrid`; implement vuedraggable for reorder; use PrimeVue `Dialog` for delete confirm.
- `App/vue/src/components/LoginButton.vue` — replace `DxForm`, `DxPopover`, and `notify` with PrimeVue `Popover` + `InputText` + `Button`.
- `App/vue/src/utils/notifyUtils.js` — replace DevExtreme `notify` with PrimeVue `Toast` (via `useToast` composable or injected service).
- `wwwroot/lib/dx-23.1.15/` and `wwwroot/lib/` — delete both directories.
- Search for stray `devextreme` imports and CSS references and remove them.

### Gotchas
- The projects grid's reorder maps to `POST /api/Projects/Reorder` with `{ projectIds: [...] }` (see `TECHNICAL_SPECIFICATION.md` §4.2 and §5.5). Preserve the immediate-UI-update + server-persist behavior.
- The PATCH endpoints for `/api/Projects` and `/api/TodoItems` currently accept DevExtreme's bulk change format. Since only DevExtreme produces that shape, translate on the frontend to that same format when calling the API.
- JWT auth stored in `localStorage` and the popover-based login flow must keep working; only the visual shell changes.
- PrimeVue 4 uses `@primevue/themes` for preset-based styling — import and register the Aura (or Lara) preset in `main.js`.

---

## Prompt (paste into a clean Claude context)

````
You are working on the To-Do Lists project at C:\Code\ak\TodoLists on Windows. Use PowerShell for all shell commands (no `&&`, no Bash-isms). Start by reading `AGENTS.md`, `README.md`, and `TECHNICAL_SPECIFICATION.md` to load project context.

## Task
Remove DevExtreme from the Vue SPA at `App/vue` and replace it with PrimeVue 4 (MIT-licensed). Keep every screen functionally equivalent. Also delete the leftover DevExtreme static-asset directories `wwwroot/lib/dx-23.1.15/` and `wwwroot/lib/`.

## Decision already made
**Do not reconsider the library choice.** The decision is PrimeVue 4, plus `vuedraggable` (SortableJS wrapper) for drag-and-drop only.

Rationale:
- PrimeVue `DataTable` supports cell/row edit modes natively — closest feature parity with DevExtreme DataGrid.
- `Toast`, `Dialog`, and `Popover` replace the equivalent DevExtreme components cleanly.
- PrimeVue's built-in `rowReorder` uses HTML5 drag-and-drop, which Selenium's `ClickAndHold` cannot trigger — `vuedraggable` with `forceFallback: true` is needed regardless, just as it would have been with any other library.

## Context you need

### Frontend
- `App/vue`; entry `src/main.js`; components in `src/components/`.
- DevExtreme is used in:
  - `src/components/MainPanel.vue` — `DxDataGrid` for todo items, cell-mode inline editing.
  - `src/components/MainPage/ProjectsPanel.vue` — `DxDataGrid` with row drag-and-drop, row-mode editing, delete with confirm dialog, clone button.
  - `src/components/LoginButton.vue` — `DxForm` + `DxPopover` + DevExtreme `notify`.
  - `src/utils/notifyUtils.js` — wraps DevExtreme `notify`.

### API contracts (do not change the backend)
- `GET /api/TodoItems?projectId=N` → array of `{ id, name, isComplete }`
- `POST /api/TodoItems` → `{ projectId, name, isComplete }`
- `PATCH /api/TodoItems` → array of `[{ key: id, data: { field: value } }]` (DevExtreme bulk format — keep this; translate on the client)
- `GET /api/Projects` → array of `{ id, name, order }`
- `POST /api/Projects` → `{ name }`
- `PATCH /api/Projects` → array of `[{ type: "insert"|"update"|"remove", data?: {...}, key?: id }]` (DevExtreme bulk format — keep this; translate on the client)
- `POST /api/Projects/Clone` → `{ id }`
- `POST /api/Projects/Reorder` → `{ projectIds: [id, ...] }` (ids in new display order)
- `POST /api/Auth/Login` → `{ profile, username, password }` → `{ accessToken, refreshToken }`

### PrimeVue 4 setup
Install:
```
npm install primevue @primevue/themes primeicons vuedraggable@^4
npm uninstall devextreme devextreme-vue
```

`src/main.js` additions:
```js
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'
import 'primeicons/primeicons.css'

app.use(PrimeVue, { theme: { preset: Aura } })
app.use(ToastService)
```

Key PrimeVue components to use:
- `DataTable` + `Column` — todo and project tables (import from `primevue/datatable` and `primevue/column`)
- `Toast` — user feedback (import from `primevue/toast`; use `useToast()` composable)
- `Dialog` — delete confirmation (import from `primevue/dialog`)
- `Popover` — login overlay (import from `primevue/popover`)
- `InputText` — text inputs (import from `primevue/inputtext`)
- `Button` — action buttons (import from `primevue/button`)
- `Checkbox` — todo completion toggle (import from `primevue/checkbox`)

### Integration tests — CSS selector mapping
The existing C# page objects use DevExtreme CSS class names. You must **update the C# page objects** to use `se-*` semantic classes AND apply those same `se-*` classes to the new Vue components.

Use PrimeVue's `pt` (passthrough) prop to inject `se-*` classes onto the inner DOM elements of PrimeVue components. Example for DataTable rows:
```html
<DataTable :pt="{ bodyRow: { class: 'se-data-row' } }">
```

Required mapping (old → new):

| Purpose | Old selector | New selector |
|---|---|---|
| Data rows | `.dx-data-row` | `.se-data-row` |
| Add-row button | `.dx-datagrid-addrow-button` | `.se-add-row-button` |
| Text editor input | `.dx-texteditor-input` | `.se-text-editor-input` |
| Delete button per row | `.dx-link-delete` | `.se-delete-button` |
| Edit button per row | `.dx-link-edit` | `.se-edit-button` |
| Drag handle | `.dx-datagrid-drag-icon` | `.se-drag-handle` |
| Checkbox wrapper | `.dx-checkbox` | `.se-checkbox` (needs `role="checkbox"` + `aria-checked` attribute) |
| Cell text editor | `.dx-textbox input` | `.se-text-box input` |
| Toast message | `.dx-toast-content` | `.se-toast-message` |
| Delete dialog | `.dx-dialog` | `.se-dialog` |
| Dialog message text | `.dx-dialog-message` | `.se-dialog-message` |
| Dialog Yes button | `[aria-label="Yes"].dx-button` | `.se-confirm-yes-button` |

Also update `MainPage.cs`:
- `TodoItemNames` selector: `.se-todo-items-data-grid .se-data-row td:nth-child(2)` (was `.se-todo-items-data-grid .dx-datagrid .dx-data-row td[aria-colindex='2']`)
- `ErrorMessages` selector: `.se-toast-message` (was `.dx-toast-content`)
- `DeleteDialog`: `.se-dialog` (was `.dx-dialog`)

**Also update `Browser.cs`** login flow — the wait for `By.ClassName("dx-popup-content")` must change to match the new login popover structure (see LoginButton section below).

### Required DOM structure

#### Projects table (`se-projects-data-grid`)
Wrap the entire projects section in `<div class="se-projects-data-grid">`.

Use `vuedraggable` (not PrimeVue's rowReorder) to render the `<tbody>`. Each `<tr class="se-data-row">` must have cells in this column order:
- `td:nth-child(1)` — drag handle: contains `<span class="se-drag-handle">`
- `td:nth-child(2)` — project name (plain text when viewing; `<div class="se-text-box"><InputText class="se-text-editor-input"></div>` when editing)
- `td:nth-child(3)` — actions: `<Button class="se-edit-button">` and `<Button class="se-delete-button">` when viewing; save/cancel when editing

Toolbar above the table: `<Button class="se-clone-button">Clone</Button>` and `<Button class="se-add-row-button">+</Button>`.

When adding a new project, a new `<tr class="se-data-row">` appears immediately (row count increments) with an `<InputText class="se-text-editor-input">` in column 2. Pressing Enter saves (POST to backend then refresh); Escape cancels.

When editing an existing row (after clicking `.se-edit-button`), column 2 changes to `<div class="se-text-box"><InputText class="se-text-editor-input"></div>`. Pressing Enter saves (`PATCH /api/Projects` with `[{ type: "update", data: { id, name } }]` then refresh); Escape cancels (reverts without saving). Empty name on Enter also cancels without saving.

Delete dialog: use PrimeVue `Dialog` with passthrough classes:
```html
<Dialog :pt="{ root: { class: 'se-dialog' }, content: { class: 'se-dialog-content' } }">
  <div class="se-dialog-message">...</div>
  <Button class="se-confirm-yes-button">Yes</Button>
</Dialog>
```
The dialog must be `Displayed = true` (visible) when shown. On confirm: send `PATCH /api/Projects` with `[{ type: "remove", key: id }]` then refresh. If only 1 project remains, show a toast instead (no dialog).

Drag-and-drop: use `vuedraggable` with `forceFallback: true` and `handle=".se-drag-handle"`. On drop, call `POST /api/Projects/Reorder` with new id order, then refresh.

Clicking `td:nth-child(2)` (the name cell) selects that project and emits `focused-project-changed` to the parent.

When a project is selected/focused (on load: first project; after add/clone: refreshed list keeps first selected), emit event so MainPanel loads its todo items.

#### Todo items table (`se-todo-items-data-grid`)
Wrap in `<div class="se-todo-items-data-grid">`. Use PrimeVue `DataTable` with `editMode="cell"` and `pt={{ bodyRow: { class: 'se-data-row' } }}`.

Each row must have:
- `td:nth-child(1)` — checkbox: `<div class="se-checkbox" role="checkbox" :aria-checked="item.isComplete ? 'true' : 'false'" @click="toggleComplete(item)">`. Clicking toggles `isComplete` and immediately sends `PATCH /api/TodoItems` with `[{ key: id, data: { isComplete: newValue } }]`.
- `td:nth-child(2)` — name: plain text when viewing. Clicking the cell enters edit mode (PrimeVue cell edit), showing `<div class="se-text-box"><InputText class="se-text-editor-input"></div>`. Pressing Enter saves (`PATCH /api/TodoItems` with `[{ key: id, data: { name: newValue } }]` then refresh); Escape reverts without saving.

Toolbar above the table: `<Button class="se-add-row-button">+</Button>`.

When adding a new item, append a new row immediately with an active editor (`se-text-editor-input`). Enter saves (`POST /api/TodoItems`); Escape cancels.

#### Login (`LoginButton.vue`)
Replace with a PrimeVue `Popover`. Keep `id="loginPopoverLink"` on the trigger element. Inside the Popover:
- `<InputText name="profile">`, `<InputText name="username">`, `<InputText name="password" type="password">`
- `<Button id="login-button">` that calls `POST /api/Auth/Login`

Update `Browser.cs`: replace `Wait.Until(d => d.FindElement(By.ClassName("dx-popup-content")))` with a wait for the login form to be visible — e.g., `Wait.Until(d => d.FindElement(By.Name("profile")).Displayed)`.

Also update `SmokeTests.cs` similarly.

#### Notifications (`notifyUtils.js`)
Replace DevExtreme `notify` with PrimeVue `Toast`. Use `useToast()` composable inside the util (or pass the toast service as a parameter). `notifySystemError` and `notifyValidationError` call `toast.add({ severity: 'error', summary: message, life: 5000 })`.

Ensure the rendered toast element carries class `se-toast-message`. Use PrimeVue Toast's `pt` prop:
```html
<Toast :pt="{ message: { class: 'se-toast-message' } }" />
```
Place `<Toast />` once in `App.vue` or `MainPage.vue`.

### Static assets to delete
Delete these directories (they are DevExtreme CDN copies and are no longer referenced):
```powershell
Remove-Item -Recurse -Force "wwwroot\lib\dx-23.1.15"
Remove-Item -Recurse -Force "wwwroot\lib"
```
Verify no remaining HTML/cshtml/razor files reference `/lib/dx-` paths before deleting.

### Files to change

**Vue (frontend):**
- `App/vue/package.json` — remove `devextreme`, `devextreme-vue`; add `primevue`, `@primevue/themes`, `primeicons`, `vuedraggable`.
- `App/vue/src/main.js` — register PrimeVue plugin + Aura theme + ToastService; remove DevExtreme imports.
- `App/vue/src/components/MainPanel.vue` — replace DxDataGrid with PrimeVue DataTable (cell edit mode) + se-* passthrough.
- `App/vue/src/components/MainPage/ProjectsPanel.vue` — replace DxDataGrid; implement vuedraggable, PrimeVue Dialog for delete confirm, PrimeVue Button for actions.
- `App/vue/src/components/LoginButton.vue` — replace DxForm/DxPopover with PrimeVue Popover + InputText + Button.
- `App/vue/src/utils/notifyUtils.js` — replace DevExtreme notify with PrimeVue Toast.
- `App/vue/src/App.vue` or `MainPage.vue` — add `<Toast />` component once.

**C# tests:**
- `IntegrationTests/Tests.Integration/PageObject/MainPage.cs`
- `IntegrationTests/Tests.Integration/PageObject/Elements/DataGridElement.cs`
- `IntegrationTests/Tests.Integration/PageObject/Elements/DataGridRowElement.cs`
- `IntegrationTests/Tests.Integration/PageObject/Elements/DataGridCellElement.cs`
- `IntegrationTests/Tests.Integration/PageObject/Elements/Dialogs/DeleteDialogElement.cs`
- `IntegrationTests/Tests.Integration/PageObject/Browser.cs`
- `IntegrationTests/Tests.Integration/Tests/SmokeTests.cs`

**Static assets:**
- `wwwroot/lib/dx-23.1.15/` — delete entire directory.
- `wwwroot/lib/` — delete entire directory.

## Definition of done
- No `devextreme*` packages remain in `App/vue/package.json` or `package-lock.json`.
- Every screen renders equivalently: todo items table with inline editing; projects table with drag-and-drop + inline edit + delete + clone; login form in a popover.
- `cd App\vue; npm run build` succeeds with no DevExtreme references.
- All existing `Tests.Integration` tests pass locally with the web app running.
- `wwwroot/lib/` directory does not exist.

## Report back
- List of files changed.
- Result of `npm run build` and `dotnet test` for `Tests.Integration`.
- Any behavior differences vs. the DevExtreme originals.
````