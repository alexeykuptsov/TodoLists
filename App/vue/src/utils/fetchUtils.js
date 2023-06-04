import * as notifyUtils from "./notifyUtils";

export function post(url, jsonObject) {
  let authToken = localStorage.getItem('authToken');
  return fetch(url, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${authToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(jsonObject),
  })
    .then(response => {
      if (response.status === 401) {
        localStorage.removeItem('authToken');
        window.location.reload();
      }
      if (response.ok) {
        return;
      }
      throw new Error('HTTP status ' + response.status);
    })
    .catch(error => notifyUtils.notifyError(`Failed to execute POST ${url}.`, error));
}

export function get(url) {
  let authToken = localStorage.getItem('authToken');
  return fetch(url, {
    headers: {
      'Accept': 'application/json',
      'Authorization': `Bearer ${authToken}`,
    },
  })
    .then(response => {
      if (response.status === 401) {
        localStorage.removeItem('authToken');
        window.location.reload();
      }
      if (!response.ok) {
        throw new Error("HTTP status " + response.status);
      }
      return response.json();
    })
    .catch(error => notifyUtils.notifyError(`Failed to execute GET ${url}.`, error));
}
