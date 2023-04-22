<template>
  <div>
    <a id="loginPopoverLink" style="cursor: pointer;">Войти</a>
    <div id="loginPopover">
      <div id="login-form"></div>
      <div id="login-button"></div>
    </div>
  </div>
</template>

<script>
import $ from 'jquery';
import notify from 'devextreme/ui/notify';
import Form from 'devextreme/ui/form';
import Button from 'devextreme/ui/button';
import Popover from 'devextreme/ui/popover';

export default {
  name: 'LoginButton',
  mounted: function () {
    this.$nextTick(function () {
      try {
        initPage(this);
      } catch (e) {
        notify(e.toString() + '\n', 'error', 5000);
        throw e;
      }
    })
  },
}

function initPage(thisComponent) {
  let loginFormData = {
    profile: null,
    username: null,
    password: null,
  };

  let previouslyEnteredProfile = localStorage.getItem('loginForm.profile')
  if (previouslyEnteredProfile !== null) {
    loginFormData.profile = previouslyEnteredProfile
  }

  const loginForm = new Form($('#login-form'), {
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
  });

  new Button($('#login-button'), {
    stylingMode: 'contained',
    text: 'Войти',
    type: 'default',
    width: 120,
    onClick() {
      let userDto = loginForm.option('formData');
      let validationResult = loginForm.validate();
      if (!validationResult.isValid) {
        notify('Не удалось войти.', 'Проверьте введенные данные.');
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
              notify('Не удалось войти.', 'Проверьте введенные данные.');
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
          .catch(error => notify('Не удалось войти.', error));
    },
  });

  new Popover($('#loginPopover'), {
    target: '#loginPopoverLink',
    showEvent: 'dxclick',
    position: 'bottom',
    width: 500,
    shading: true,
    shadingColor: 'rgba(0, 0, 0, 0.5)',
  });
}

</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>


</style>
