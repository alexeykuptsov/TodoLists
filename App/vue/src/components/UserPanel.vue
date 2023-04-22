<template>
  <div>
    <div style="display: inline-block;">{{ userName }}</div>
    <a onclick="this.$root.logout();" href='#'>Выйти</a>
  </div>
</template>

<script>
export default {
  name: 'UserPanel',
  props: {
    userName: String
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>


</style>
