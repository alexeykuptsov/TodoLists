import { createApp } from 'vue'
import App from './App.vue'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'
import 'primeicons/primeicons.css'

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
const app = createApp(App, data);
app.use(PrimeVue, { theme: { preset: Aura } });
app.use(ToastService);
app.mount('#app');