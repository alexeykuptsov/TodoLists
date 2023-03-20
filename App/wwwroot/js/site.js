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
                        .attr('onclick', `navigator.clipboard.writeText(${toastMessage});`)
                        .attr('style', 'color: #99ddff;')
                        .text('Копировать текст ошибки');
                    $rootDiv.append($copyLink);

                    element.append($rootDiv);
                }
            };
            DevExpress.ui.notify(notifyToastOptions, { position: "bottom", direction: "up-push" });
        });
}

function addItem() {
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
        .catch(error => DevExpress.ui.notify('Error: ' + error));
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

let vueApp;

function initPage() {
    Vue.component('UserPanel', {
        data: function () {
            return {
                userName: this.$root.userName,
                foo: 'bar',
            }
        },
        template:
            `<div>
                <div style="display: inline-block;">{{ userName }}</div>
                <a onclick="vueApp.logout();" href='#'>Выйти</a>
            </div>`
    });
    Vue.component('LoginButton', {
        template:
            `<div>
                <a id="loginPopoverLink" style="cursor: pointer;">Войти</a>
                <div id="loginPopover">
                    <div id="login-form"></div>
                    <div id="login-button"></div>
                </div>
            </div>`,
        
    });
    Vue.component('MainPanel', {
        template:
            `<div>
                <h1>To-do CRUD</h1>
                <h3>Add</h3>
                <form action="javascript:void(0);" method="POST" onsubmit="addItem()">
                    <input type="text" id="add-name" placeholder="New to-do">
                    <input type="submit" value="Add">
                </form>
                
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
            </div>`
    });
    Vue.component('NoAccessPanel', {
        template:
            `<div>
                <h1>Нет доступа</h1>
                <div>Войдите, чтобы воспользоватться приложением.</div>
            </div>`
    });
    let authToken = localStorage.getItem('authToken');
    let userName = null;
    if (authToken !== null) {
        try {
            let authTokenJson = JSON.parse(atob(authToken.split('.')[1]));
            userName = authTokenJson['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
        } catch (e) {
            localStorage.removeItem('authToken')
            authToken = null;
        }
    }
    vueApp = new Vue({
        el: '#vapp',
        data: {
            display: 'redbox',
            userName: userName,
        },
        methods: {
            logout() {
                localStorage.removeItem("authToken");
                this.isAuthenticated = false;
                document.location.reload();
            }
        }
    });

    let loginFormData = {
        profile: null,
        username: null,
        password: null,
    };

    let previouslyEnteredProfile = localStorage.getItem('loginForm.profile')
    if (previouslyEnteredProfile !== null) {
        loginFormData.profile = previouslyEnteredProfile
    }

    const loginForm = $('#login-form').dxForm({
        colCount: 2,
        labelMode: 'floating',
        formData: loginFormData,
        items: [{
            dataField: 'profile',
            label: 'Профиль',
            validationRules: [{
                type: 'required',
            }],
        }, {
            dataField: 'username',
            label: 'Логин',
            editorOptions: {
                inputAttr: {
                    type: 'username',
                    autocomplete: 'on',
                },
            },
            validationRules: [{
                type: 'required',
            }],
        }, {
            dataField: 'password',
            label: 'Пароль',
            editorOptions: {
                mode: 'password',
                inputAttr: {
                    type: 'password',
                    autocomplete: 'on',
                },
            },
            validationRules: [{
                type: 'required',
                message: 'Password is required',
            }],
        }],
    }).dxForm('instance');

    $('#login-button').dxButton({
        stylingMode: 'contained',
        text: 'Войти',
        type: 'default',
        width: 120,
        onClick() {
            let userDto = loginForm.option('formData');
            let validationResult = loginForm.validate();
            if (!validationResult.isValid) {
                DevExpress.ui.notify('Не удалось войти.', 'Проверьте введенные данные.');
                return;
            }

            localStorage.setItem('loginForm.profile', userDto.profile);

            fetch('https://localhost:7147/api/Auth/Login', {
                method: 'POST',
                headers: {
                    'Accept': 'text',
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(userDto),
            })
                .then(response => {
                    if (response.status === 401) {
                        DevExpress.ui.notify('Не удалось войти.', 'Проверьте введенные данные.');
                    }
                    if (!response.ok) {
                        throw new Error("HTTP status " + response.status);
                    }
                    return response.text();
                })
                .then(token => {
                    localStorage.setItem('authToken', token);
                    document.location.reload();
                })
                .catch(error => DevExpress.ui.notify('Не удалось войти.', error));
        },
    });

    $('#loginPopover').dxPopover({
        target: '#loginPopoverLink',
        showEvent: 'dxclick',
        position: 'bottom',
        width: 500,
        shading: true,
        shadingColor: 'rgba(0, 0, 0, 0.5)',
    });

    if (userName !== null) {
        refreshPageData();
    }
}

$(() => {
    try {
        initPage();
    } catch (e) {
        DevExpress.ui.notify(e.toString() + '\n', 'error', 5000);
    }
});