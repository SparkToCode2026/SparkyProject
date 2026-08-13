// ---------- Shared / nav ----------
function updateNav() {
  const user = authUser();
  const loggedIn = document.getElementById("navLoggedIn");
  const loggedOut = document.getElementById("navLoggedOut");
  const userNameEl = document.getElementById("navUserName");
  const navBookings = document.getElementById("navBookings");

  if (user) {
    if (loggedIn) loggedIn.classList.remove("d-none");
    if (loggedOut) loggedOut.classList.add("d-none");
    if (navBookings) navBookings.classList.remove("d-none");
    if (userNameEl) userNameEl.textContent = `Hi, ${user.userName}`;
  } else {
    if (loggedIn) loggedIn.classList.add("d-none");
    if (loggedOut) loggedOut.classList.remove("d-none");
    if (navBookings) navBookings.classList.add("d-none");
  }
}

function logout() {
  clearAuth();
  window.location.href = "index.html";
}

function requireLogin() {
  const user = authUser();
  if (user) return user;
  window.location.href = "login.html";
  return null;
}

// ---------- Home page ----------
async function loadHome() {
  const [hotels, rooms, roomTypes, currentUser] = await Promise.all([
    apiRequest("/Hotel"),
    apiRequest("/Room"),
    apiRequest("/RoomType/GetAllRoomTypes"),
    Promise.resolve(authUser()),
  ]);

  let bookingModal = null;

  window.openBook = (hotelId, hotelName) => {
    if (!currentUser) {
      window.location.href = "pages/login.html";
      return;
    }
    const hotelRooms = rooms.filter((r) => {
      const rt = roomTypes.find((t) => t.roomTypeId === r.roomTypeId);
      return rt && rt.hotelId === hotelId;
    });
    const available = hotelRooms.filter((r) => r.status === "Available");
    const typeName = (id) => {
      const t = roomTypes.find((x) => x.roomTypeId === id);
      return t ? `${t.roomName} (${t.capacity} guests)` : `Room ${id}`;
    };

    let options = available.map(
      (r) => `<option value="${r.roomId}">Room ${r.roomNumber} - ${typeName(r.roomTypeId)}</option>`
    );
    if (!options.length) options = [`<option>No rooms available</option>`];

    const today = new Date().toISOString().split("T")[0];
    const tomorrow = new Date(Date.now() + 86400000).toISOString().split("T")[0];

    document.getElementById("bookModalTitle").textContent = `Book at ${hotelName}`;
    document.getElementById("bookModalBody").innerHTML = `
      <form id="bookForm">
        <div class="mb-3">
          <label class="form-label">Room</label>
          <select class="form-select" id="bookRoom">${options.join("")}</select>
        </div>
        <div class="mb-3">
          <label class="form-label">Check-in</label>
          <input type="date" class="form-control" id="bookCheckIn" value="${today}" />
        </div>
        <div class="mb-3">
          <label class="form-label">Check-out</label>
          <input type="date" class="form-control" id="bookCheckOut" value="${tomorrow}" />
        </div>
        <div id="bookingError" class="text-danger small mb-2"></div>
        <button type="submit" class="btn btn-primary w-100">Confirm booking</button>
      </form>`;
    bookingModal = document.getElementById("bookModal");
    bookingModal.style.display = "block";

    document.getElementById("bookForm").addEventListener("submit", async (e) => {
      e.preventDefault();
      const roomId = parseInt(document.getElementById("bookRoom").value, 10);
      const checkInDate = document.getElementById("bookCheckIn").value;
      const checkOutDate = document.getElementById("bookCheckOut").value;
      try {
        await apiRequest("/Booking", "POST", {
          userId: currentUser.userId,
          roomId,
          checkInDate,
          checkOutDate,
          status: "Confirmed",
        });
        bookingModal.style.display = "none";
        window.location.href = "pages/bookings.html";
      } catch (err) {
        document.getElementById("bookingError").textContent = err.message;
      }
    });
  };

  window.closeBook = () => {
    if (bookingModal) bookingModal.style.display = "none";
  };

  const grid = document.getElementById("hotelGrid");
  const noResults = document.getElementById("noResults");
  const searchBox = document.getElementById("searchBox");

  const render = (query) => {
    const q = (query || "").toLowerCase().trim();
    const filtered = hotels.filter(
      (h) =>
        !q ||
        (h.name && h.name.toLowerCase().includes(q)) ||
        (h.city && h.city.toLowerCase().includes(q))
    );
    noResults.classList.toggle("d-none", filtered.length > 0);
    grid.innerHTML = filtered
      .map(
        (h) => `
        <div class="col-md-6 col-lg-4">
          <div class="card h-100 hotel-card">
            <div class="card-body d-flex flex-column">
              <h5 class="card-title">${h.name || "Hotel"}</h5>
              <p class="card-text text-muted mb-1">${h.city || ""}${h.address ? " &middot; " + h.address : ""}</p>
              ${h.phone ? `<p class="card-text small mb-2"><strong>Phone:</strong> ${h.phone}</p>` : ""}
              <button class="btn btn-primary mt-auto" onclick="openBook(${h.hotelId}, '${h.name.replace(/'/g, "\\'")}')">
                Book now
              </button>
            </div>
          </div>
        </div>`
      )
      .join("");
  };

  if (searchBox) {
    searchBox.addEventListener("input", (e) => render(e.target.value));
  }
  render("");

  document.addEventListener("click", (e) => {
    if (bookingModal && bookingModal.style.display === "block" && !bookingModal.contains(e.target)) {
      bookingModal.style.display = "none";
    }
  });
}

// ---------- Auth pages ----------
async function bindLogin() {
  const form = document.getElementById("loginForm");
  if (!form) return;
  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const email = document.getElementById("loginEmail").value.trim();
    const password = document.getElementById("loginPassword").value;
    try {
      const data = await apiRequest("/Auth/login", "POST", { email, password });
      setAuth(data.token, {
        userId: data.userId,
        userName: data.userName,
        userEmail: data.userEmail,
        role: data.role,
      });
      window.location.href = "../index.html";
    } catch (err) {
      showAlert(err.message, true);
    }
  });
}

async function bindRegister() {
  const form = document.getElementById("registerForm");
  if (!form) return;
  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const name = document.getElementById("regName").value.trim();
    const email = document.getElementById("regEmail").value.trim();
    const password = document.getElementById("regPassword").value;
    try {
      await apiRequest("/Auth/register", "POST", { name, email, password, role: "Guest" });
      showAlert("Account created! You can log in now.", false);
      setTimeout(() => (window.location.href = "login.html"), 800);
    } catch (err) {
      showAlert(err.message, true);
    }
  });
}

function showAlert(message, isError) {
  const box = document.getElementById("alertBox");
  if (!box) return;
  box.className = `alert ${isError ? "alert-danger" : "alert-success"}`;
  box.textContent = message;
}

// ---------- Bookings page ----------
async function loadBookings() {
  const user = requireLogin();
  if (!user) return;
  const [bookings, rooms, roomTypes, hotels] = await Promise.all([
    apiRequest("/Booking"),
    apiRequest("/Room"),
    apiRequest("/RoomType/GetAllRoomTypes"),
    apiRequest("/Hotel"),
  ]);

  const roomName = (id) => {
    const r = rooms.find((x) => x.roomId === id);
    if (!r) return `Room #${id}`;
    const t = roomTypes.find((y) => y.roomTypeId === r.roomTypeId);
    return `Room ${r.roomNumber}${t ? ` - ${t.roomName}` : ""}`;
  };
  const hotelName = (roomId) => {
    const r = rooms.find((x) => x.roomId === roomId);
    const t = r && roomTypes.find((y) => y.roomTypeId === r.roomTypeId);
    const h = t && hotels.find((z) => z.hotelId === t.hotelId);
    return h ? h.name : "";
  };

  const mine = bookings.filter((b) => b.userId === user.userId);
  const list = document.getElementById("bookingsList");

  if (!mine.length) {
    list.innerHTML = `<p class="text-muted">You have no bookings yet. <a href="../index.html">Book a room</a></p>`;
    return;
  }

  list.innerHTML = mine
    .slice()
    .sort((a, b) => new Date(b.checkInDate) - new Date(a.checkInDate))
    .map(
      (b) => `
      <div class="card mb-3 booking-card">
        <div class="card-body">
          <div class="d-flex justify-content-between flex-wrap gap-2">
            <div>
              <h5 class="mb-1">${hotelName(b.roomId) || "Hotel"}</h5>
              <p class="mb-1">${roomName(b.roomId)}</p>
              <p class="mb-1 text-muted">
                ${new Date(b.checkInDate).toLocaleDateString()} &rarr; ${new Date(b.checkOutDate).toLocaleDateString()}
              </p>
            </div>
            <span class="badge ${b.status === "Confirmed" ? "bg-success" : b.status === "CheckedIn" ? "bg-primary" : b.status === "Cancelled" ? "bg-secondary" : "bg-warning text-dark"} align-self-start">
              ${b.status}
            </span>
          </div>
          <small class="text-muted">Booking #${b.bookingId}</small>
        </div>
      </div>`
    )
    .join("");
}

// ---------- Boot by page ----------
document.addEventListener("DOMContentLoaded", () => {
  updateNav();
  const page = window.location.pathname.split("/").pop();

  if (page === "index.html" || page === "") loadHome();
  else if (page === "login.html") bindLogin();
  else if (page === "register.html") bindRegister();
  else if (page === "bookings.html") loadBookings();
});