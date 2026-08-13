// Shared API helper. The backend can run on either local HTTP or HTTPS URL depending on how it was started.
const API_BASE_URLS = [
  "http://localhost:57344/api",
  "https://localhost:57343/api",
  "http://localhost:5000/api",
  "https://localhost:5001/api",
];

async function apiRequest(path, method = "GET", body = null) {
  const token = localStorage.getItem("jwtToken");
  const headers = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  let lastError = null;

  for (const baseUrl of API_BASE_URLS) {
    try {
      const response = await fetch(`${baseUrl}${path}`, {
        method,
        headers: { ...headers },
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

      if (response.status === 204) return null;
      return response.json();
    } catch (error) {
      lastError = error;
      if (error instanceof Error && error.name === "TypeError") {
        continue;
      }
      throw error;
    }
  }

  const fallbackMessage = lastError && lastError.message
    ? lastError.message
    : "Unable to connect to the backend. Start the API server and try again.";

  throw new Error(fallbackMessage);
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

// Shared currency formatter — Omani Rial.
function fmtMoney(n) {
  const value = Number(n);
  if (!Number.isFinite(value)) return "OMR 0";
  return `OMR ${value.toLocaleString("en-US", { minimumFractionDigits: 3, maximumFractionDigits: 3 })}`;
}