<template>
  <Toast :pt="{ message: { class: 'se-toast-message' } }" />
  <user-panel id="top-toolbar" v-if="userName != null" :userName="userName"></user-panel>
  <login-button v-if="userName == null"></login-button>
  <no-access-panel v-if="userName == null"></no-access-panel>
  <main-panel v-if="userName != null" :userName="userName"></main-panel>
</template>

<script>
import { useToast } from 'primevue/usetoast';
import Toast from 'primevue/toast';
import UserPanel from './components/UserPanel.vue'
import MainPanel from './components/MainPanel.vue'
import NoAccessPanel from './components/NoAccessPanel.vue'
import LoginButton from './components/LoginButton.vue'
import * as notifyUtils from './utils/notifyUtils';

export default {
  name: 'App',
  components: {
    UserPanel,
    MainPanel,
    NoAccessPanel,
    LoginButton,
    Toast,
  },
  props: {
    userName: String,
  },
  setup() {
    const toast = useToast();
    notifyUtils.setToast(toast);
  },
}
</script>

<style>

</style>
