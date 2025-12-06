import * as notifyUtils from "./notifyUtils";
import { getConfig } from '@/config';

const config = getConfig();

// Token storage keys
const ACCESS_TOKEN_KEY = 'authToken';
const REFRESH_TOKEN_KEY = 'refreshToken';

// Token refresh buffer time (5 minutes in milliseconds)
const TOKEN_REFRESH_BUFFER = 5 * 60 * 1000;

/**
 * Parse JWT payload from token string
 * @param {string} token - JWT token
 * @returns {object|null} - Parsed payload or null if invalid
 */
export function parseJwtPayload(token) {
  try {
    if (!token || typeof token !== 'string') {
      return null;
    }

    const parts = token.split('.');
    if (parts.length !== 3) {
      return null;
    }

    // Decode base64 payload (second part)
    const payload = parts[1];
    // Add padding if needed for proper base64 decoding
    const paddedPayload = payload + '='.repeat((4 - payload.length % 4) % 4);
    const decodedPayload = atob(paddedPayload);
    
    return JSON.parse(decodedPayload);
  } catch (error) {
    console.error('Error parsing JWT payload:', error);
    return null;
  }
}

/**
 * Check if token is expired
 * @param {string} token - JWT token
 * @returns {boolean} - True if token is expired
 */
export function isTokenExpired(token) {
  const payload = parseJwtPayload(token);
  if (!payload || !payload.exp) {
    return true;
  }

  const currentTime = Math.floor(Date.now() / 1000);
  return payload.exp <= currentTime;
}

/**
 * Check if token expires within the buffer time (5 minutes)
 * @param {string} token - JWT token
 * @returns {boolean} - True if token expires soon
 */
export function isTokenExpiringSoon(token) {
  const payload = parseJwtPayload(token);
  if (!payload || !payload.exp) {
    return true;
  }

  const currentTime = Math.floor(Date.now() / 1000);
  const bufferTime = Math.floor(TOKEN_REFRESH_BUFFER / 1000);
  return payload.exp <= (currentTime + bufferTime);
}

/**
 * Get tokens from localStorage
 * @returns {object} - Object with accessToken and refreshToken
 */
function getTokens() {
  return {
    accessToken: localStorage.getItem(ACCESS_TOKEN_KEY),
    refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY)
  };
}

/**
 * Store tokens in localStorage
 * @param {string} accessToken - Access token
 * @param {string} refreshToken - Refresh token
 */
function storeTokens(accessToken, refreshToken) {
  if (accessToken) {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  }
  if (refreshToken) {
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  }
}

/**
 * Clear all tokens from localStorage
 */
function clearTokens() {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

/**
 * Refresh access token using refresh token
 * @returns {Promise<string|null>} - New access token or null if refresh failed
 */
async function refreshTokens() {
  try {
    const { refreshToken } = getTokens();
    
    if (!refreshToken) {
      console.warn('No refresh token available');
      return null;
    }

    console.log('Attempting to refresh access token...');
    
    const response = await fetch(config.apiBaseUrl + '/api/Auth/Refresh', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) {
      console.error('Token refresh failed:', response.status, response.statusText);
      
      // If refresh fails, clear tokens and redirect to login
      clearTokens();
      window.location.reload();
      return null;
    }

    const authResponse = await response.json();
    
    if (!authResponse.accessToken || !authResponse.refreshToken) {
      console.error('Invalid auth response format:', authResponse);
      clearTokens();
      window.location.reload();
      return null;
    }

    // Store new tokens
    storeTokens(authResponse.accessToken, authResponse.refreshToken);
    console.log('Tokens refreshed successfully');
    
    return authResponse.accessToken;
  } catch (error) {
    console.error('Error during token refresh:', error);
    clearTokens();
    window.location.reload();
    return null;
  }
}

/**
 * Enhanced fetch function with automatic token refresh
 * @param {string} urlPath - API endpoint path
 * @param {object} options - Fetch options
 * @returns {Promise<Response>} - Fetch response
 */
export async function fetchWithAuth(urlPath, options = {}) {
  let { accessToken } = getTokens();
  
  // Check if token needs refresh before making the request
  if (!accessToken || isTokenExpiringSoon(accessToken)) {
    console.log('Access token missing or expiring soon, attempting refresh...');
    accessToken = await refreshTokens();
    
    if (!accessToken) {
      throw new Error('Unable to obtain valid access token');
    }
  }

  // Prepare headers with authorization
  const headers = {
    'Accept': 'application/json',
    'Authorization': `Bearer ${accessToken}`,
    ...options.headers
  };

  // Make the request
  const requestOptions = {
    ...options,
    headers
  };

  try {
    const response = await fetch(config.apiBaseUrl + urlPath, requestOptions);
    
    // Handle 401 - token might have expired during the request
    if (response.status === 401) {
      console.log('Received 401, attempting token refresh...');
      
      // Try to refresh token
      const newAccessToken = await refreshTokens();
      
      if (newAccessToken) {
        // Retry the original request with new token
        const retryHeaders = {
          ...headers,
          'Authorization': `Bearer ${newAccessToken}`
        };
        
        const retryOptions = {
          ...requestOptions,
          headers: retryHeaders
        };
        
        console.log('Retrying request with new token...');
        const retryResponse = await fetch(config.apiBaseUrl + urlPath, retryOptions);
        
        if (retryResponse.status === 401) {
          // Still 401 after refresh, clear tokens and reload
          console.error('Still unauthorized after token refresh');
          clearTokens();
          window.location.reload();
          throw new Error('Authentication failed');
        }
        
        return retryResponse;
      } else {
        // Refresh failed, tokens already cleared, page will reload
        throw new Error('Token refresh failed');
      }
    }
    
    return response;
  } catch (error) {
    console.error('Error in fetchWithAuth:', error);
    throw error;
  }
}

/**
 * Assert response success and handle common errors
 * @param {Response} response - Fetch response
 * @returns {Response} - Response if successful
 */
function assertSuccess(response) {
  if (!response.ok) {
    throw new Error("HTTP status " + response.status);
  }
  return response;
}

/**
 * POST request with authentication
 * @param {string} urlPath - API endpoint path
 * @param {object} jsonObject - Request body object
 * @returns {Promise<Response>} - Fetch response
 */
export function post(urlPath, jsonObject) {
  return fetchWithAuth(urlPath, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(jsonObject),
  })
    .then(assertSuccess)
    .catch(error => notifyUtils.notifySystemError(`Failed to execute POST ${urlPath}.`, JSON.stringify(error)));
}

/**
 * PATCH request with authentication
 * @param {string} urlPath - API endpoint path
 * @param {object} jsonObject - Request body object
 * @returns {Promise<Response>} - Fetch response
 */
export function patch(urlPath, jsonObject) {
  return fetchWithAuth(urlPath, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(jsonObject),
  })
    .then(assertSuccess)
    .catch(error => notifyUtils.notifySystemError(`Failed to execute PATCH ${urlPath}.`, error));
}

/**
 * GET request with authentication
 * @param {string} urlPath - API endpoint path
 * @returns {Promise<object>} - Parsed JSON response
 */
export function get(urlPath) {
  return fetchWithAuth(urlPath, {
    method: 'GET'
  })
    .then(assertSuccess)
    .then(response => response.json())
    .catch(error => notifyUtils.notifySystemError(`Failed to execute GET ${urlPath}.`, JSON.stringify(error)));
}

// Export token management utilities for external use
export { getTokens, storeTokens, clearTokens };
