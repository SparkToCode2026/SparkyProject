// Shared API helper. Base URL should match your Web API's launch URL/port.
const API_BASE_URL = "https://localhost:7000/api"; // TODO: update to match your launchSettings.json

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
    throw new Error(`API request failed: ${response.status} ${response.statusText}`);
  }

  if (response.status === 204) return null; // no content (e.g. DELETE)
  return response.json();
}
