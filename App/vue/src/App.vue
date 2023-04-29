<template>
  <user-panel id="top-toolbar" v-if="userName != null" :userName="userName"></user-panel>
  <login-button v-if="userName == null"></login-button>
  <no-access-panel v-if="userName == null"></no-access-panel>
  <main-panel v-if="userName != null" :userName="userName"></main-panel>
</template>

<script>
import UserPanel from './components/UserPanel.vue'
import MainPanel from './components/MainPanel.vue'
import NoAccessPanel from './components/NoAccessPanel.vue'
import LoginButton from './components/LoginButton.vue'

export default {
  name: 'App',
  components: {
    UserPanel,
    MainPanel,
    NoAccessPanel,
    LoginButton,
  },
  props: {
    userName: String,
  },
}
</script>

<style>

</style>

