# T-1: Migrate from DevExtreme to a free Vue component library

## Summary
Replace the paid DevExtreme UI library used by the Vue SPA at `App/vue` with a popular, free, open-source Vue 3 component library, keeping all existing screens functionally equivalent.

## Context / Background
The frontend at `App/vue` currently uses DevExtreme (DevExpress) — see `App/vue/src/components/MainPanel.vue`, `App/vue/src/components/LoginButton.vue`, and `App/vue/src/components/MainPage/ProjectsPanel.vue`. DevExtreme is a paid, heavyweight component library. The project's stated purpose (see `README.md`) is practicing with programming languages, technologies and libraries. Both the license cost and the runtime/bundle weight of DevExtreme are pain points that block casual open-source use of the app.

The migration touches only the frontend. Backend contracts (see the endpoints in `TECHNICAL_SPECIFICATION.md` §4) do not change; the replacement library must adapt to the existing REST responses rather than reshaping them. Related work: [[T-2]] (updating .NET) is independent and can proceed in parallel.

Screens/components currently backed by DevExtreme:
- `MainPanel.vue` — DevExtreme `DataGrid` for todo items with inline editing.
- `ProjectsPanel.vue` — DevExtreme `DataGrid` with row drag-and-drop reordering, inline edit, delete, clone.
- `LoginButton.vue` — DevExtreme `Form` + `Popover` for profile/username/password entry, plus `notify` for user feedback.

## Acceptance Criteria
- [ ] No `devextreme*` npm packages remain in `App/vue/package.json` (or `package-lock.json`).
- [ ] Every screen that used a DevExtreme component renders equivalently with the chosen replacement — todo items table with inline editing, projects table with drag-and-drop reordering + inline edit + delete + clone, and the login popover/form.
- [ ] All existing integration tests in `Tests.Integration` pass locally with the web app running.
- [ ] `App/vue` builds cleanly (`npm run build`) with no references to `devextreme`, `devextreme-vue`, or DevExtreme CSS bundles.
- [ ] The new library and its CSS are imported once in a central place; no leftover DevExtreme stylesheet imports.

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

**Open question — final library choice.** Do not pick a winner in this task file; the implementing session should decide based on current library versions and preference. Note the two hard constraints when choosing:
1. Row drag-and-drop for the projects list (currently handled by DevExtreme's `RowDragging`). If the chosen library lacks native support, plan to integrate `vuedraggable` (SortableJS wrapper for Vue 3).
2. Inline row editing for the todo items and projects grids.

### Files/areas to touch
- `App/vue/package.json` — remove `devextreme`, `devextreme-vue`, add the chosen library.
- `App/vue/src/main.js` (or equivalent entry) — swap CSS imports; register plugin/globals if needed.
- `App/vue/src/components/MainPanel.vue` — replace `DxDataGrid` for todo items.
- `App/vue/src/components/MainPage/ProjectsPanel.vue` — replace `DxDataGrid`; re-implement row drag-and-drop + reorder API call.
- `App/vue/src/components/LoginButton.vue` — replace `DxForm`, `DxPopover`, and `notify`.
- Any util files that call `notify` from DevExtreme — replace with the chosen library's toast/message API.
- Search for stray `devextreme` imports and CSS references and remove them.

### Gotchas
- The projects grid's `@reorder` event maps to `POST /api/Projects/Reorder` with `{ projectIds: [...] }` (see `TECHNICAL_SPECIFICATION.md` §4.2 and §5.5). Preserve the immediate-UI-update + server-persist behavior.
- The PATCH endpoints for `/api/Projects` and `/api/TodoItems` currently accept DevExtreme's bulk change format. Since only DevExtreme produces that shape, the new library will likely send single-row calls — either adapt the frontend to translate to the existing format or update the controller to accept the new shape (the backend change should be part of this task if needed).
- JWT auth stored in `localStorage` and the popover-based login flow must keep working; only the visual shell changes.

---

## Prompt (paste into a clean Claude context)

````
You are working on the To-Do Lists project at C:\Code\ak\TodoLists on Windows. Use PowerShell for all shell commands (no `&&`, no Bash-isms). Start by reading `AGENTS.md`, `README.md`, and `TECHNICAL_SPECIFICATION.md` to load project context.

## Task
Replace the paid DevExtreme UI library used by the Vue SPA at `App/vue` with a popular, free, open-source Vue 3 component library. Keep every screen functionally equivalent. The backend REST API does not change (unless the DevExtreme-specific PATCH bulk-change format needs adapting — see gotcha below).

## Context you need
- Frontend lives at `App/vue`; entry is `src/main.js`; components in `src/components/`.
- DevExtreme is used in exactly these components:
  - `src/components/MainPanel.vue` — `DxDataGrid` of todo items with inline editing.
  - `src/components/MainPage/ProjectsPanel.vue` — `DxDataGrid` with row drag-and-drop reordering, inline edit, delete, clone.
  - `src/components/LoginButton.vue` — `DxForm` + `DxPopover` + DevExtreme `notify`.
- Auth is dev-only, JWT stored in `localStorage`. See TECHNICAL_SPECIFICATION.md §4 for API contracts.
- Projects drag-and-drop currently calls `POST /api/Projects/Reorder` with `{ projectIds: [...] }` (immediate UI update, then server persist).
- The bulk-update endpoints `PATCH /api/Projects` and `PATCH /api/TodoItems` accept DevExtreme's change-array format. After migration, either translate to that shape on the client, or update the controller to accept the new library's shape. Pick one and be consistent.

## Choose a library
The final choice is intentionally open. Consider these candidates, all free/OSS Vue 3 libraries:
- Vuetify 3 — Material, huge; table lacks native row drag/inline edit.
- PrimeVue — DataTable has native row reorder + edit modes; closest feature match to DevExtreme.
- Quasar — full framework; adopting it is a bigger change than swapping components.
- Element Plus — clean admin look; row drag needs Sortable.js addon.
- Naive UI — small, TS-first; row drag needs `vuedraggable`.
- Ant Design Vue — enterprise table; heavier bundle; row drag needs `vuedraggable`.

Hard constraints:
1. Row drag-and-drop for the projects list must work (fall back to `vuedraggable` if the library lacks it).
2. Inline row editing for todo items and projects.

Decide, briefly justify in your final report, then implement.

## Definition of done — all must hold
- No `devextreme*` packages remain in `App/vue/package.json` or `package-lock.json`.
- Every previously-DevExtreme screen renders equivalently with the replacement: todo items table with inline editing; projects table with drag-and-drop reordering + inline edit + delete + clone; login popover/form.
- `App/vue` builds cleanly: `cd App\vue; npm run build` succeeds with no DevExtreme references.
- All existing `Tests.Integration` tests pass locally with the web app running (`cd IntegrationTests\Tests.Integration; dotnet test`, with the app running per README).
- New library CSS is imported once in a central place; no leftover DevExtreme stylesheet imports.

## Report back
- Which library you chose and why (2–3 sentences).
- The list of files changed.
- The result of `npm run build` and `dotnet test` for `Tests.Integration`.
- Any behavior differences you noticed vs. the DevExtreme originals.
````
