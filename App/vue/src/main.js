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

const data = {
    userName: userName,
};
createApp(App, data).mount('#app');
