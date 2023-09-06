<template>
  <div>
    <div id="se-user-name" style="display: inline-block;">{{ userName }}</div>
    <a v-on:click="logout" href='#'>Sign out</a>
  </div>
</template>

<script>
export default {
  name: 'UserPanel',
  props: {
    userName: String
  },
  methods: {
    logout() {
      localStorage.removeItem("authToken");
      document.location.reload();
    }
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>


</style>
