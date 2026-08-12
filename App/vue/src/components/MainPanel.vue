<template>
  <div>
    <div id="main-header">
      <h1>To-Do Lists</h1>
    </div>
    <splitpanes id="main-splitpanes" class="default-theme">
      <pane min-size="20" size="30">
        <ProjectsPanel
          @focused-project-changed="onFocusedProjectChanged"
        />
      </pane>
      <pane min-size="20">
        <div>
          <h3 id="project-name" style="display: inline-block;">{{projectName}}</h3>
          <p id="counter" style="display: inline-block; padding-left: 20px;"></p>
        </div>

        <div>
          <input type="text" id="add-name" placeholder="New to-do">
          <button :class="{'se-add-todo-item-button': true}" v-on:click="addItem">Add</button>
        </div>

        <div class="se-todo-items-data-grid">
          <div style="margin-bottom: 4px;">
            <Button class="se-add-row-button" @click="addNewItem">+</Button>
          </div>
          <DataTable
            :value="todoItems"
            :pt="{ bodyRow: { class: 'se-data-row' } }"
          >
            <Column>
              <template #body="{ data }">
                <div
                  class="se-checkbox"
                  role="checkbox"
                  :aria-checked="data.isComplete ? 'true' : 'false'"
                  @click.stop="toggleComplete(data)"
                  style="width: 20px; height: 20px; border: 1px solid #ccc; cursor: pointer; background: white;"
                  :style="data.isComplete ? { background: '#4CAF50' } : {}"
                ></div>
              </template>
            </Column>
            <Column field="name">
              <template #body="{ data }">
                <template v-if="data._isNew">
                  <div class="se-text-box">
                    <InputText
                      ref="newItemInput"
                      class="se-text-editor-input"
                      v-model="data.name"
                      @keydown.enter.stop="saveNewItem(data)"
                      @keydown.escape.stop="cancelNewItem(data)"
                      @click.stop
                      style="width: 100%;"
                    />
                  </div>
                </template>
                <template v-else-if="editingItemId === data.id">
                  <div class="se-text-box">
                    <InputText
                      class="se-text-editor-input"
                      v-model="editingName"
                      @keydown.enter.stop="saveItemEdit(data)"
                      @keydown.escape.stop="cancelItemEdit"
                      @click.stop
                      style="width: 100%;"
                    />
                  </div>
                </template>
                <template v-else>
                  <div @click.stop="startItemEdit(data)" style="cursor: pointer; min-height: 1.5rem; width: 100%;">{{ data.name }}</div>
                </template>
              </template>
            </Column>
          </DataTable>
        </div>
      </pane>
    </splitpanes>
  </div>
</template>

<script>
/* eslint-disable vue/no-reserved-component-names */
import $ from 'jquery';
import {Pane, Splitpanes} from 'splitpanes'
import 'splitpanes/dist/splitpanes.css'
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import * as fetchUtils from '../utils/fetchUtils';
import ProjectsPanel from "@/components/MainPage/ProjectsPanel.vue";
import * as notifyUtils from "@/utils/notifyUtils";

export default {
  name: 'MainPanel',
  props: {
    userName: String
  },
  components: {
    ProjectsPanel,
    Splitpanes,
    Pane,
    DataTable,
    Column,
    InputText,
    Button,
  },
  data() {
    return {
      todoItems: [],
      projectId: null,
      projectName: null,
      todoItemsUri: 'api/TodoItems',
      editingItemId: null,
      editingName: '',
    };
  },
  mounted() {
    this.$nextTick(function () {
      adjustElementSizes();
      window.addEventListener("resize", this.windowResizeHandler);
    });
  },
  unmounted() {
    window.removeEventListener("resize", this.windowResizeHandler);
  },
  methods: {
    windowResizeHandler() {
      adjustElementSizes();
    },
    addItem() {
      const addNameTextBox = document.getElementById('add-name');
      const name = addNameTextBox.value.trim();
      if (!name) return;

      const item = {
        projectId: this.projectId,
        name: name,
        isComplete: false,
      };

      fetchUtils.post(this.todoItemsUri, item)
        .then(() => {
          this.refreshTodoItems();
          addNameTextBox.value = '';
        });
    },
    addNewItem() {
      const newItem = { id: null, name: '', isComplete: false, _isNew: true };
      this.todoItems.push(newItem);
      this.$nextTick(() => {
        const input = this.$refs.newItemInput;
        if (input) {
          const el = Array.isArray(input) ? input[input.length - 1] : input;
          const domEl = el.$el || el;
          if (domEl && domEl.focus) domEl.focus();
        }
      });
    },
    async saveNewItem(item) {
      if (!item.name.trim()) {
        this.cancelNewItem(item);
        return;
      }
      try {
        await fetchUtils.post(this.todoItemsUri, {
          projectId: this.projectId,
          name: item.name.trim(),
          isComplete: false,
        });
        this.refreshTodoItems();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to add item.', error);
      }
    },
    cancelNewItem(item) {
      const idx = this.todoItems.indexOf(item);
      if (idx !== -1) this.todoItems.splice(idx, 1);
    },
    startItemEdit(item) {
      this.editingItemId = item.id;
      this.editingName = item.name;
    },
    async saveItemEdit(item) {
      if (!this.editingName.trim()) {
        this.cancelItemEdit();
        return;
      }
      const name = this.editingName.trim();
      this.editingItemId = null;
      this.editingName = '';
      try {
        await fetchUtils.patch(this.todoItemsUri, [{ key: item.id, data: { name: name } }]);
        this.refreshTodoItems();
      } catch (error) {
        notifyUtils.notifySystemError('Unable to update item.', error);
      }
    },
    cancelItemEdit() {
      this.editingItemId = null;
      this.editingName = '';
    },
    async toggleComplete(item) {
      const newValue = !item.isComplete;
      item.isComplete = newValue;
      try {
        await fetchUtils.patch(this.todoItemsUri, [{ key: item.id, data: { isComplete: newValue } }]);
      } catch (error) {
        item.isComplete = !newValue;
        notifyUtils.notifySystemError('Unable to update item.', error);
      }
    },
    refreshTodoItems() {
      if (this.projectId == null) return;
      fetchUtils.get(this.todoItemsUri + `?projectId=${this.projectId}`).then(data => {
        if (data) this._displayItems(data);
      });
    },
    _displayItems(data) {
      _displayCount(data.length);

      this.todoItems = data.map(item => ({
        id: item.id,
        name: item.name,
        isComplete: item.isComplete,
      }));
      this.editingItemId = null;
      this.editingName = '';

      document.getElementById('se-ajax-load-status').innerText = 'complete';
    },
    onFocusedProjectChanged(e) {
      this.projectId = e.row != null && e.row.data != null ? e.row.data.id : null;
      this.projectName = e.row != null && e.row.data != null ? e.row.data.name : null;
      this.refreshTodoItems();
    },
  }
}

function adjustElementSizes() {
  const paneElements = $('#main-splitpanes > .splitpanes__pane');
  for (const paneElement of paneElements) {
    const height = Math.max(window.innerHeight - paneElement.getBoundingClientRect().top, 200);
    $(paneElement).css('height', height);
  }
}

function _displayCount(itemCount) {
  const name = (itemCount === 1) ? 'to-do' : 'to-dos';
  document.getElementById('counter').innerText = `${itemCount} ${name}`;
}
</script>
