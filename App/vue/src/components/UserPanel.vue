<template>
  <div>
    <div id="se-user-name" style="display: inline-block;">{{ userName }}</div>
    <a v-on:click="logout" href='#'>Sign out</a>
  </div>
</template>

<script>
import { getConfig } from '@/config';
import { clearTokens } from '@/utils/fetchUtils';

const config = getConfig();

export default {
  name: 'UserPanel',
  props: {
    userName: String
  },
  methods: {
    async logout() {
      try {
        // Get refresh token for logout API call
        const refreshToken = localStorage.getItem('refreshToken');
        
        if (refreshToken) {
          // Call logout API to revoke refresh token on server
          await fetch(config.apiBaseUrl + 'api/Auth/Logout', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({ refreshToken }),
          });
        }
      } catch (error) {
        console.warn('Error during logout API call:', error);
        // Continue with local logout even if API call fails
      }
      
      // Clear all tokens from localStorage
      clearTokens();
      document.location.reload();
    }
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>


</style>
