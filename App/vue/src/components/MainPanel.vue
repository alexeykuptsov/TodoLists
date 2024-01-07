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

        <p id="counter"></p>

        <DxDataGrid
            :class="{ 'se-todo-items-data-grid': true }"
            :ref="todoItemsDataGridRefKey"
            :data-source="todoItems"
            :remote-operations="false"
            :allow-column-reordering="true"
            :row-alternation-enabled="true"
            :show-borders="true"
            :show-column-headers="false"
            @saving="onSaving"
        >
          <DxEditing
              :allow-updating="true"
              mode="cell"
          />
          <DxColumn data-field="isComplete" data-type="boolean" :width="40" />
          <DxColumn data-field="name" />
        </DxDataGrid>
      </pane>
    </splitpanes>
  </div>
</template>

<script>
import $ from 'jquery';
import {Pane, Splitpanes} from 'splitpanes'
import 'splitpanes/dist/splitpanes.css'
import {DxColumn, DxDataGrid, DxEditing} from 'devextreme-vue/data-grid';
import * as fetchUtils from '../utils/fetchUtils';
import ProjectsPanel from "@/components/MainPage/ProjectsPanel.vue";

const todoItemsDataGridRefKey = 'todo-items-data-grid';

export default {
  name: 'MainPanel',
  props: {
    userName: String
  },
  components: {
    ProjectsPanel,
    Splitpanes,
    Pane,
    DxDataGrid,
    DxColumn,
    DxEditing,
  },
  data() {
    return {
      todoItems: [],
      todoItemsDataGridRefKey,
      projectId: null,
      projectName: null,
    };
  },
  computed: {
    todoItemsDataGrid: function() {
      return this.$refs[todoItemsDataGridRefKey].instance;
    },
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

      const item = {
        projectId: this.projectId,
        name: addNameTextBox.value.trim(),
        isComplete: false,
      };

      fetchUtils.post('api/TodoItems', item)
        .then(() => {
          this.refreshTodoItems();
          addNameTextBox.value = '';
        });
    },
    refreshTodoItems() {
      fetchUtils.get(`api/TodoItems?projectId=${this.projectId}`).then(data => {
        this._displayItems(data);
      });
    },
    onSaving(e) {
      fetchUtils.patch(`api/TodoItems`, e.changes);
    },
    _displayItems(data) {
      _displayCount(data.length);

      this.todoItems.splice(0);
      data.forEach(item => {
        this.todoItems.push({
          id: item.id,
          name: item.name,
          isComplete: item.isComplete,
        });
      });

      this.todoItemsDataGrid.refresh()
        .done(() => {
          document.getElementById('se-ajax-load-status').innerText = 'complete';
        });
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
