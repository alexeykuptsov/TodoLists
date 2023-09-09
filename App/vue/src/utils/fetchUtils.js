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
    .then(assertSuccess)
    .catch(error => notifyUtils.notifySystemError(`Failed to execute POST ${url}.`, JSON.stringify(error)));
}

export function patch(url, jsonObject) {
  let authToken = localStorage.getItem('authToken');
  return fetch(url, {
    method: 'PATCH',
    headers: {
      'Authorization': `Bearer ${authToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(jsonObject),
  })
    .then(assertSuccess)
    .then(response => response.json())
    .catch(error => notifyUtils.notifySystemError(`Failed to execute PATCH ${url}.`, error));
}

export function get(url) {
  let authToken = localStorage.getItem('authToken');
  return fetch(url, {
    headers: {
      'Accept': 'application/json',
      'Authorization': `Bearer ${authToken}`,
    },
  })
    .then(assertSuccess)
    .then(response => response.json())
    .catch(error => notifyUtils.notifySystemError(`Failed to execute GET ${url}.`, JSON.stringify(error)));
}

function assertSuccess(response) {
  if (response.status === 401) {
    localStorage.removeItem('authToken');
    window.location.reload();
  }
  if (!response.ok) {
    throw new Error("HTTP status " + response.status);
  }
  return response;
}
