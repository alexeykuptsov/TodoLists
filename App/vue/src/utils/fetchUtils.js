import * as notifyUtils from "./notifyUtils";
import { getConfig } from '@/config';

const config = getConfig();

export function post(urlPath, jsonObject) {
  let authToken = localStorage.getItem('authToken');
  return fetch(config.apiBaseUrl + urlPath, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${authToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(jsonObject),
  })
    .then(assertSuccess)
    .catch(error => notifyUtils.notifySystemError(`Failed to execute POST ${urlPath}.`, JSON.stringify(error)));
}

export function patch(urlPath, jsonObject) {
  let authToken = localStorage.getItem('authToken');
  return fetch(config.apiBaseUrl + urlPath, {
    method: 'PATCH',
    headers: {
      'Authorization': `Bearer ${authToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(jsonObject),
  })
    .then(assertSuccess)
    .then(response => response.json())
    .catch(error => notifyUtils.notifySystemError(`Failed to execute PATCH ${urlPath}.`, error));
}

export function get(urlPath) {
  let authToken = localStorage.getItem('authToken');
  return fetch(config.apiBaseUrl + urlPath, {
    headers: {
      'Accept': 'application/json',
      'Authorization': `Bearer ${authToken}`,
    },
  })
    .then(assertSuccess)
    .then(response => response.json())
    .catch(error => notifyUtils.notifySystemError(`Failed to execute GET ${urlPath}.`, JSON.stringify(error)));
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
