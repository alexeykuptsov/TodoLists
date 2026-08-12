<template>
  <div>
    <a id="loginPopoverLink" style="cursor: pointer;" @click="toggle">Sign in</a>
    <Popover ref="popover">
      <div style="padding: 16px; min-width: 300px; display: flex; flex-direction: column; gap: 8px;">
        <InputText name="profile" v-model="loginData.profile" placeholder="Profile" style="width: 100%;" />
        <InputText name="username" v-model="loginData.username" placeholder="Username" style="width: 100%;" />
        <InputText name="password" type="password" v-model="loginData.password" placeholder="Password" style="width: 100%;" />
        <Button id="login-button" label="Sign in" @click="login" />
      </div>
    </Popover>
  </div>
</template>

<script>
/* eslint-disable vue/no-reserved-component-names */
import Popover from 'primevue/popover';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import { getConfig } from '@/config';

const config = getConfig();

export default {
  name: 'LoginButton',
  components: { Popover, InputText, Button },
  data() {
    return {
      loginData: {
        profile: localStorage.getItem('loginForm.profile') || '',
        username: '',
        password: '',
      },
    };
  },
  mounted() {
    this.$nextTick(() => {
      document.getElementById('se-ajax-load-status').innerText = 'complete';
    });
  },
  methods: {
    toggle(event) {
      this.$refs.popover.toggle(event);
    },
    async login() {
      const { profile, username, password } = this.loginData;
      try {
        const response = await fetch(config.apiBaseUrl + 'api/Auth/Login', {
          method: 'POST',
          headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ profile, username, password }),
        });

        if (response.status === 401) {
          return;
        }
        if (!response.ok) {
          throw new Error('HTTP status ' + response.status);
        }

        const authResponse = await response.json();
        localStorage.setItem('loginForm.profile', profile);
        localStorage.setItem('authToken', authResponse.accessToken);
        localStorage.setItem('refreshToken', authResponse.refreshToken);
        document.location.reload();
      } catch (error) {
        console.error('Login failed:', error);
      }
    },
  },
}
</script>

<style scoped>
</style>
