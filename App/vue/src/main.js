import { createApp } from 'vue'
import App from './App.vue'

let authToken = localStorage.getItem('authToken');
let userName = null;
if (authToken !== null) {
    try {
        let authTokenJson = JSON.parse(atob(authToken.split('.')[1]));
        userName = authTokenJson['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    } catch (e) {
        localStorage.removeItem('authToken')
        authToken = null;
    }
}

// Vue.loadScript('lib/jquery-3.5.1/js/jquery-3.5.1.min.js');
// Vue.loadScript('lib/dx-22.1.6/js/dx.all.js');

const data = {
    userName: userName,
};
createApp(App, data).mount('#app');
