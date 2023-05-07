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
          <button v-on:click="addItem">Add</button>
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

        <table>
          <tr>
            <th>Is Complete?</th>
            <th>Name</th>
            <th></th>
            <th></th>
          </tr>
          <tbody id="todos"></tbody>
        </table>
      </pane>
    </splitpanes>
  </div>
</template>

<script>
import $ from 'jquery';
import notify from 'devextreme/ui/notify';
import { Splitpanes, Pane } from 'splitpanes'
import 'splitpanes/dist/splitpanes.css'

export default {
  name: 'MainPanel',
  props: {
    userName: String
  },
  components: {
    Splitpanes, Pane
  },
  data() {
    return {
      projects: [ 'Inbox', 'Project 1', 'Project 2'],
    };
  },  
  mounted() {
    this.$nextTick(function () {
      try {
        initPage(this);
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
      fetch(uri, {
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
            refreshPageData();
            addNameTextbox.value = '';
          })
          .catch(error => notify(error, 'error', 10_000));
    },
  }
}

function initPage(thisComponent) {
  adjustElementSizes();

  if (thisComponent.userName !== null) {
    refreshPageData();
  }
}

const uri = 'api/todoitems';
let todos = [];

function refreshPageData() {
  let authToken = localStorage.getItem('authToken');

  if (authToken === null) {
    throw new Error('Unexpected auth token value. It is asserted that it is not null.');
  }

  fetch(uri, {
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
      .then(data => _displayItems(data))
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
}

function adjustElementSizes() {
  const paneElements = $('#main-splitpanes > .splitpanes__pane');
  for (const paneElement of paneElements) {
    const height = Math.max(window.innerHeight - paneElement.getBoundingClientRect().top, 200);
    $(paneElement).css('height', height);
  }
}

function deleteItem(id) {
  let authToken = localStorage.getItem('authToken');
  fetch(`${uri}/${id}`, {
    method: 'DELETE',
    headers: {
      'Authorization': 'Bearer ' + authToken,
    },
  })
      .then(() => refreshPageData())
      .catch(error => console.error('Unable to delete item.', error));
}

function displayEditForm(id) {
  const item = todos.find(item => item.id === id);

  document.getElementById('edit-name').value = item.name;
  document.getElementById('edit-id').value = item.id;
  document.getElementById('edit-isComplete').checked = item.isComplete;
  document.getElementById('editForm').style.display = 'block';
}

function updateItem() {
  const itemId = document.getElementById('edit-id').value;
  const item = {
    id: parseInt(itemId, 10),
    isComplete: document.getElementById('edit-isComplete').checked,
    name: document.getElementById('edit-name').value.trim()
  };

  let authToken = localStorage.getItem('authToken');
  fetch(`${uri}/${itemId}`, {
    method: 'PUT',
    headers: {
      'Accept': 'application/json',
      'Authorization': 'Bearer ' + authToken,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(item)
  })
      .then(() => refreshPageData())
      .catch(error => console.error('Unable to update item.', error));

  closeInput();

  return false;
}

function closeInput() {
  document.getElementById('editForm').style.display = 'none';
}

function _displayCount(itemCount) {
  const name = (itemCount === 1) ? 'to-do' : 'to-dos';

  document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function _displayItems(data) {
  const tBody = document.getElementById('todos');
  tBody.innerHTML = '';

  _displayCount(data.length);

  const button = document.createElement('button');

  data.forEach(item => {
    let isCompleteCheckbox = document.createElement('input');
    isCompleteCheckbox.type = 'checkbox';
    isCompleteCheckbox.disabled = true;
    isCompleteCheckbox.checked = item.isComplete;

    let editButton = button.cloneNode(false);
    editButton.innerText = 'Edit';
    editButton.setAttribute('onclick', `displayEditForm(${item.id})`);

    let deleteButton = button.cloneNode(false);
    deleteButton.innerText = 'Delete';
    deleteButton.setAttribute('onclick', `deleteItem(${item.id})`);

    let tr = tBody.insertRow();

    let td1 = tr.insertCell(0);
    td1.appendChild(isCompleteCheckbox);

    let td2 = tr.insertCell(1);
    let textNode = document.createTextNode(item.name);
    td2.appendChild(textNode);

    let td3 = tr.insertCell(2);
    td3.appendChild(editButton);

    let td4 = tr.insertCell(3);
    td4.appendChild(deleteButton);
  });

  todos = data;
}

</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

</style>
