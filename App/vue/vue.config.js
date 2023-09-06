const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  outputDir: '../../wwwroot',
  configureWebpack: {
    cache: {
      type: 'filesystem'
    },
    stats: 'errors-warnings',
  },
})
