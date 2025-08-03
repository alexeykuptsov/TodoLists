const path = require("path");

module.exports = {
  presets: [
    ['@vue/cli-plugin-babel/preset', {
      // Configure preset for DevExtreme compatibility
      useBuiltIns: 'entry',
      corejs: 3
    }]
  ],
  // Add source map support for better debugging
  sourceMaps: true,
  // Ensure compatibility with DevExtreme's older JavaScript patterns
  assumptions: {
    setPublicClassFields: true,
    privateFieldsAsProperties: true
  }
}
