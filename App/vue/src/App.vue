<template>
  <Toast :pt="{ message: { class: 'se-toast-message' } }" />
  <user-panel id="top-toolbar" v-if="userName != null" :userName="userName"></user-panel>
  <login-button v-if="userName == null"></login-button>
  <no-access-panel v-if="userName == null"></no-access-panel>
  <div v-if="userName != null" id="se-nav-bar" style="padding: 4px 8px; border-bottom: 1px solid #ddd; display: flex; gap: 4px;">
    <Button
      class="se-nav-tasks-button"
      label="Tasks"
      :outlined="currentPage !== 'main'"
      @click="currentPage = 'main'"
    />
    <Button
      class="se-nav-templates-button"
      label="Project Templates"
      :outlined="currentPage !== 'templates'"
      @click="currentPage = 'templates'"
    />
  </div>
  <main-panel v-if="userName != null && currentPage === 'main'" :userName="userName"></main-panel>
  <project-template-page v-if="userName != null && currentPage === 'templates'"></project-template-page>
</template>

<script>
/* eslint-disable vue/no-reserved-component-names */
import { useToast } from 'primevue/usetoast';
import Toast from 'primevue/toast';
import Button from 'primevue/button';
import UserPanel from './components/UserPanel.vue';
import MainPanel from './components/MainPanel.vue';
import NoAccessPanel from './components/NoAccessPanel.vue';
import LoginButton from './components/LoginButton.vue';
import ProjectTemplatePage from './components/ProjectTemplatePage.vue';
import * as notifyUtils from './utils/notifyUtils';

export default {
  name: 'App',
  components: {
    UserPanel,
    MainPanel,
    NoAccessPanel,
    LoginButton,
    Toast,
    Button,
    ProjectTemplatePage,
  },
  props: {
    userName: String,
  },
  data() {
    return {
      currentPage: 'main',
    };
  },
  setup() {
    const toast = useToast();
    notifyUtils.setToast(toast);
  },
};
</script>

<style>

</style>
