<template>
  <div>
    <div id="main-header">
      <h1>To-Do Lits</h1>
    </div>
    <splitpanes id="main-splitpanes" class="default-theme">
      <pane min-size="20" size="30">
        <ul>
          <li v-for="project in projects" :key="project">{{ project }}</li>
        </ul>
      </pane>
      <pane min-size="20">
        <h3>Add</h3>

        <div>
          <input type="text" id="add-name" placeholder="New to-do">
          <button :class="{'se-add-todo-item-button': true}" v-on:click="addItem">Add</button>
        </div>

        <div id="editForm">
          <h3>Edit</h3>
          <form action="javascript:void(0);" onsubmit="updateItem()">
            <input type="hidden" id="edit-id">
            <input type="checkbox" id="edit-isComplete">
            <input type="text" id="edit-name">
            <input type="submit" value="Save">
            <a onclick="closeInput()" aria-label="Close">&#10006;</a>
          </form>
        </div>

        <p id="counter"></p>

        <DxDataGrid
            :ref="todoItemsDataGridRefKey"
            :data-source="todoItems"
            :remote-operations="false"
            :allow-column-reordering="true"
            :row-alternation-enabled="true"
            :show-borders="true"
        >
          <DxColumn data-field="isComplete" data-type="boolean" :width="40" />
          <DxColumn data-field="title" />
        </DxDataGrid>
      </pane>
    </splitpanes>
  </div>
</template>

<script>
import $ from 'jquery';
import notify from 'devextreme/ui/notify';
import {Pane, Splitpanes} from 'splitpanes'
import 'splitpanes/dist/splitpanes.css'
import {DxDataGrid, DxColumn} from 'devextreme-vue/data-grid';
import ArrayStore from 'devextreme/data/array_store';
import DataSource from 'devextreme/data/data_source';

const todoItemsDataGridRefKey = 'todo-items-data-grid';

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
  },
  data() {
    return {
      projects: [ 'Inbox', 'Project 1', 'Project 2'],
      todoItems: [],
      todoItemsDataGridRefKey,
      dataSource: new DataSource({
        store: new ArrayStore({
          key: 'id',
          data: this.todoItems,
        }),
      }),
      uri: 'api/todoitems',

    };
  },
  computed: {
    todoItemsDataGrid: function() {
      return this.$refs[todoItemsDataGridRefKey].instance;
    }
  },
  mounted() {
    this.$nextTick(function () {
      try {
        $('#todo-items-data-grid')
        
        adjustElementSizes();

        if (this.userName !== null) {
          this.refreshPageData();
        }
      } catch (e) {
        notify(e.toString() + '\n', 'error', 5000);
        throw e;
      }
      window.addEventListener("resize", this.windowResizeHandler);
    });
  },
  unmounted() {
    window.removeEventListener("resize", this.windowResizeHandler);
  },
  methods: {
    windowResizeHandler(e) {
      adjustElementSizes();
    },
    addItem() {
      const addNameTextbox = document.getElementById('add-name');

      const item = {
        isComplete: false,
        name: addNameTextbox.value.trim()
      };

      let authToken = localStorage.getItem('authToken');
      fetch(this.uri, {
        method: 'POST',
        headers: {
          'Accept': 'application/json',
          'Authorization': 'Bearer ' + authToken,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(item)
      })
          .then(response => {
            if (response.ok) {
              return response.json();
            }
            throw new Error('Response is not OK (status=' + response.status + ')');
          })
          .then(() => {
            this.refreshPageData();
            addNameTextbox.value = '';
          })
          .catch(error => notify(error, 'error', 10_000));
    },
    refreshPageData() {
      let authToken = localStorage.getItem('authToken');
    
      if (authToken === null) {
        throw new Error('Unexpected auth token value. It is asserted that it is not null.');
      }
    
      fetch(this.uri, {
        headers: {
          'Authorization': 'Bearer ' + authToken,
        }
      })
          .then(response => {
            if (!response.ok) {
              throw new Error("HTTP status " + response.status);
            }
            return response.json();
          })
          .then(data => this._displayItems(data))
          .catch(error => {
            if (error.message === 'HTTP status 401') {
              localStorage.removeItem('authToken');
              window.location.reload();
            }
    
            let toastMessage = 'Unknown error on loading items. ' + error.message;
            let notifyToastOptions = {
              type: 'error',
              displayTime: 5000,
              contentTemplate(element) {
                const $rootDiv = $('<div>')
    
                const $text = $('<div>').text(toastMessage);
                $rootDiv.append($text);
    
                const $copyLink = $('<a>')
                    .attr('href', '#')
                    .attr('onclick', `navigator.clipboard.writeText('${toastMessage}');`)
                    .attr('style', 'color: #99ddff;')
                    .text('Копировать текст ошибки');
                $rootDiv.append($copyLink);
    
                element.append($rootDiv);
              }
            };
            notify(notifyToastOptions, { position: "bottom", direction: "up-push" });
            throw error;
          });
    },
    deleteItem(id) {
      let authToken = localStorage.getItem('authToken');
      fetch(`${this.uri}/${id}`, {method: 'DELETE', headers: {'Authorization': 'Bearer ' + authToken}})
          .then(() => this.refreshPageData())
          .catch(error => console.error('Unable to delete item.', error));
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
    
      this.todoItemsDataGrid.refresh();
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

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

</style>
