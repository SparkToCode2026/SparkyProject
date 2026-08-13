// Shared API helper. Base URL should match your Web API's launch URL/port.
const API_BASE_URL = "http://localhost:57344/api"; // TODO: update to match your launchSettings.json

async function apiRequest(path, method = "GET", body = null) {
  const token = localStorage.getItem("jwtToken");

  const headers = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : null,
  });

  if (!response.ok) {
    let message = `${response.status} ${response.statusText}`;
    try {
      const text = await response.text();
      if (text) message = text.replace(/^"(.*)"$/, "$1");
    } catch {
      /* keep default message */
    }
    const err = new Error(message);
    err.status = response.status;
    throw err;
  }

  if (response.status === 204) return null; // no content (e.g. DELETE)
  return response.json();
}

function authUser() {
  try {
    return JSON.parse(localStorage.getItem("currentUser"));
  } catch {
    return null;
  }
}

function setAuth(token, user) {
  localStorage.setItem("jwtToken", token);
  localStorage.setItem("currentUser", JSON.stringify(user));
}

function clearAuth() {
  localStorage.removeItem("jwtToken");
  localStorage.removeItem("currentUser");
}