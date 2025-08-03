const { defineConfig } = require('@vue/cli-service')
const webpack = require('webpack')

module.exports = defineConfig({
  transpileDependencies: true,
  outputDir: '../../wwwroot',
  
  productionSourceMap: false,
  css: {
    sourceMap: true
  },
  
  configureWebpack: {
    devtool: process.env.NODE_ENV === 'development' ? 'eval-source-map' : false,
    cache: {
      type: 'filesystem'
    },
    stats: 'errors-warnings',
    plugins: [
      new webpack.DefinePlugin({
        // Vue 3 feature flags for DevExtreme compatibility
        __VUE_OPTIONS_API__: JSON.stringify(true),
        __VUE_PROD_DEVTOOLS__: JSON.stringify(false),
        __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: JSON.stringify(false),
        // Additional compatibility flags
        'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV || 'development')
      })
    ]
  }
})
