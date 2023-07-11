<template>
  <div>
    <div id="main-header">
      <h1>To-Do Lists</h1>
    </div>
    <splitpanes id="main-splitpanes" class="default-theme">
      <pane min-size="20" size="30">
        <h3>Projects</h3>

        <DxDataGrid
          :class="{ 'se-projects-data-grid': true }"
          :ref="projectsDataGridRefKey"
          :data-source="projects"
          :remote-operations="false"
          :allow-column-reordering="true"
          :row-alternation-enabled="true"
          :show-borders="true"
          :show-column-headers="false"
          :focused-row-enabled="true"
          :auto-navigate-to-focused-row="true"
          v-model:focused-row-key="focusedRowKey"
          @focused-row-changed="onFocusedRowChanged"
          @saving="projectsDataGrid_onSaving"
        >
          <DxEditing
            :allow-updating="true"
            :allow-deleting="true"
            :allow-adding="true"
            mode="row"
          />
          <DxRowDragging
            :allow-reordering="true"
          />
          <DxColumn data-field="name" />
        </DxDataGrid>
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
            :class="{ 'se-todo-item-data-grid': true }"
            :ref="todoItemsDataGridRefKey"
            :data-source="todoItems"
            :remote-operations="false"
            :allow-column-reordering="true"
            :row-alternation-enabled="true"
            :show-borders="true"
            :show-column-headers="false"
            v-model:focused-row-key="focusedRowKey"
            @saving="onSaving"
        >
          <DxEditing
              :allow-updating="true"
              mode="cell"
          />
          <DxColumn data-field="isComplete" data-type="boolean" :width="40" />
          <DxColumn data-field="title" />
        </DxDataGrid>
      </pane>
    </splitpanes>
  </div>
</template>

<script>
import $ from 'jquery';
import {Pane, Splitpanes} from 'splitpanes'
import 'splitpanes/dist/splitpanes.css'
import {DxColumn, DxDataGrid, DxEditing, DxRowDragging} from 'devextreme-vue/data-grid';
import ArrayStore from 'devextreme/data/array_store';
import DataSource from 'devextreme/data/data_source';
import * as notifyUtils from '../utils/notifyUtils';
import * as fetchUtils from '../utils/fetchUtils';

const todoItemsDataGridRefKey = 'todo-items-data-grid';
const projectsDataGridRefKey = 'projects-data-grid';

export default {
  name: 'MainPanel',
  props: {
    userName: String
  },
  components: {
    Splitpanes,
    Pane,
    DxDataGrid,
    DxColumn,
    DxEditing,
    DxRowDragging,
  },
  data() {
    return {
      projects: [],
      todoItems: [],
      todoItemsDataGridRefKey,
      projectsDataGridRefKey,
      uri: 'api/TodoItems',
      projectsUri: 'api/Projects',
      focusedRowKey: null,
      projectName: null,
    };
  },
  computed: {
    todoItemsDataGrid: function() {
      return this.$refs[todoItemsDataGridRefKey].instance;
    },
    projectsDataGrid: function() {
      return this.$refs[projectsDataGridRefKey].instance;
    },
  },
  mounted() {
    this.$nextTick(function () {
      this.refreshPageData();
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
        isComplete: false,
        name: addNameTextBox.value.trim()
      };

      fetchUtils.post(this.uri, item)
        .then(() => {
          this.refreshPageData();
          addNameTextBox.value = '';
        });
    },
    refreshPageData() {
      fetchUtils.get(this.uri).then(data => {
        this._displayItems(data);
      });
      fetchUtils.get(this.projectsUri).then(data => {
        this._displayProjects(data);
      });
    },
    deleteItem(id) {
      let authToken = localStorage.getItem('authToken');
      fetch(`${this.uri}/${id}`, {method: 'DELETE', headers: {'Authorization': 'Bearer ' + authToken}})
          .then(() => this.refreshPageData())
          .catch(error => notifyUtils.notifyError('Unable to delete item.', error));
    },
    onSaving(e) {
      fetch(this.uri, {
        method: 'PATCH',
        headers: {'Authorization': 'Bearer ' + localStorage.getItem('authToken'), 'Content-Type': 'application/json'},
        body: JSON.stringify(e.changes),
      })
        .catch(error => notifyUtils.notifyError('Unable to patch item.', error));
    },
    projectsDataGrid_onSaving(e) {
      fetch(this.projectsUri, {
        method: 'PATCH',
        headers: {'Authorization': 'Bearer ' + localStorage.getItem('authToken'), 'Content-Type': 'application/json'},
        body: JSON.stringify(e.changes),
      })
        .catch(error => notifyUtils.notifyError('Unable to patch item.', error));
    },
    _displayItems(data) {
      _displayCount(data.length);

      this.todoItems.splice(0);
      data.forEach(item => {
        this.todoItems.push({
          id: item.id,
          title: item.name,
          isComplete: item.isComplete,
        });
      });

      this.todoItemsDataGrid.refresh()
        .done(() => {
          document.getElementById('se-ajax-load-status').innerText = 'complete';
        });
    },
    _displayProjects(data) {

      this.projects.splice(0);
      data.forEach(item => {
        this.projects.push({
          id: item.id,
          name: item.name,
        });
      });
      // this.projectsDataSource.store().load();
      this.projectsDataGrid.refresh()
        .done(() => {
          this.focusedRowKey = data[0];
        });
    },
    onFocusedRowChanged(e) {
      this.focusedRowKey = e.component.option('focusedRowKey');
      this.projectName = this.projects.filter(x => x.id === this.focusedRowKey.id)[0].name;
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
