// ---------- Shared / nav ----------
function logout() {
  clearAuth();
  const isSub = window.location.pathname.split("/").includes("pages");
  window.location.href = isSub ? "../index.html" : "index.html";
}

function requireLogin() {
  const user = authUser();
  if (user) return user;

  const isSubPage = window.location.pathname.split("/").includes("pages");
  window.location.href = isSubPage ? "login.html" : "pages/login.html";
  return null;
}

// Update launcher nav based on auth (index.html only).
function updateNav() {
  const user = authUser();
  const loggedIn = document.getElementById("navLoggedIn");
  const loggedOut = document.getElementById("navLoggedOut");
  const userNameEl = document.getElementById("navUserName");
  const navBookings = document.getElementById("navBookings");
  const navProfile = document.getElementById("navProfile");

  if (user) {
    if (loggedIn) loggedIn.classList.remove("d-none");
    if (loggedOut) loggedOut.classList.add("d-none");
    if (navBookings) navBookings.classList.remove("d-none");
    if (navProfile) navProfile.classList.remove("d-none");
    const navAdmin = document.getElementById("navAdmin");
    if (navAdmin) navAdmin.classList.remove("d-none");
    if (userNameEl) userNameEl.textContent = `Hi, ${user.userName}`;
  } else {
    if (loggedIn) loggedIn.classList.add("d-none");
    if (loggedOut) loggedOut.classList.remove("d-none");
    if (navBookings) navBookings.classList.add("d-none");
    if (navProfile) navProfile.classList.add("d-none");
    const navAdmin = document.getElementById("navAdmin");
    if (navAdmin) navAdmin.classList.add("d-none");
    if (userNameEl) userNameEl.textContent = "";
  }
}

// ---------- Home page ----------
async function loadHome() {
  const [hotels, rooms, roomTypes, amenities, promotions, reviews, currentUser] = await Promise.all([
    apiRequest("/Hotel"),
    apiRequest("/Room"),
    apiRequest("/RoomType/GetAllRoomTypes"),
    apiRequest("/Amenity"),
    apiRequest("/Promotion/GetAllPromotions"),
    apiRequest("/Review"),
    Promise.resolve(authUser()),
  ]);

  renderPromoStrip(promotions);
  window.__reviewState = { reviews, rooms, roomTypes, hotels, amenities, currentUser };

  let bookingModal = document.getElementById("bookModal");
  const bookedHotelIds = new Set(getBookedHotelIds());
  const searchBox = document.getElementById("searchBox");

  const refreshBookedState = () => {
    const searchValue = searchBox ? searchBox.value : "";
    render(searchValue);
  };

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
        <button type="submit" class="btn-ink w-100 btn-ink-lg">Confirm booking</button>
      </form>`;
    bookingModal = document.getElementById("bookModal");
    bootstrap.Modal.getOrCreateInstance(bookingModal).show();

    document.getElementById("bookForm").addEventListener("submit", async (e) => {
      e.preventDefault();
      const roomId = parseInt(document.getElementById("bookRoom").value, 10);
      const checkInDate = document.getElementById("bookCheckIn").value;
      const checkOutDate = document.getElementById("bookCheckOut").value;
      const bookingError = document.getElementById("bookingError");

      if (!Number.isInteger(roomId) || roomId <= 0) {
        bookingError.textContent = "Please select a valid room.";
        return;
      }

      if (!checkInDate || !checkOutDate) {
        bookingError.textContent = "Please choose both dates.";
        return;
      }

      const checkIn = new Date(checkInDate);
      const checkOut = new Date(checkOutDate);

      if (Number.isNaN(checkIn.getTime()) || Number.isNaN(checkOut.getTime())) {
        bookingError.textContent = "The selected dates are invalid.";
        return;
      }

      if (checkOut <= checkIn) {
        bookingError.textContent = "Check-out must be later than check-in.";
        return;
      }

      try {
        const response = await apiRequest("/Booking", "POST", {
          userId: currentUser.userId,
          roomId,
          checkInDate: checkIn.toISOString(),
          checkOutDate: checkOut.toISOString(),
          status: "Confirmed",
        });

        if (response && response.roomId) {
          const room = rooms.find((r) => r.roomId === response.roomId);
          if (room) {
            const roomType = roomTypes.find((t) => t.roomTypeId === room.roomTypeId);
            if (roomType) {
              const hotelId = Number(roomType.hotelId);
              const updated = [...new Set([...getBookedHotelIds(), hotelId])];
              setBookedHotelIds(updated);
              bookedHotelIds.add(hotelId);
              refreshBookedState();
            }
          }
        }

        bootstrap.Modal.getOrCreateInstance(bookingModal).hide();
      } catch (err) {
        bookingError.textContent = err.message;
      }
    });
  };

  window.closeBook = () => {
    if (bookingModal) bootstrap.Modal.getOrCreateInstance(bookingModal).hide();
  };

  const grid = document.getElementById("hotelGrid");
  const noResults = document.getElementById("noResults");

  if (grid) {
    grid.onclick = (event) => {
      const button = event.target instanceof HTMLElement ? event.target.closest(".book-room-btn") : null;
      const detailBtn = event.target instanceof HTMLElement ? event.target.closest(".hotel-detail-btn") : null;
      if (detailBtn) {
        window.openDetails(Number(detailBtn.dataset.hotelId), detailBtn.dataset.hotelName || "Hotel");
        return;
      }
      if (!button) return;

      const hotelId = Number(button.dataset.hotelId);
      const hotelName = button.dataset.hotelName || "Hotel";
      window.openBook(hotelId, hotelName);
    };
  }

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
      .map((h) => {
        const isBooked = bookedHotelIds.has(Number(h.hotelId));
        const hotelId = Number(h.hotelId);
        const hotelAmenities = amenities.filter((a) => Number(a.hotelId) === hotelId);
        const hotelReviews = reviews.filter((r) => Number(r.hotelId) === hotelId);
        const avgRating = hotelReviews.length
          ? hotelReviews.reduce((s, r) => s + Number(r.rating), 0) / hotelReviews.length
          : null;
        const promos = promotions.filter((p) => Number(p.hotelId) === hotelId);
        return `
        <div class="col-md-6 col-lg-4">
          <div class="hotel-card">
            <div class="p-4 d-flex flex-column h-100">
              <p class="city mb-0">${h.city || "Hotel"}</p>
              <h3 class="hotel-name">${h.name || "Hotel"}</h3>
              ${h.address ? `<p class="address mb-1">${h.address}</p>` : ""}
              ${h.phone ? `<p class="phone mb-0">${h.phone}</p>` : ""}
              <div class="card-meta">
                ${avgRating !== null ? `<span class="meta-item"><span class="rating-dot">★</span> ${avgRating.toFixed(1)} (${hotelReviews.length})</span>` : `<span class="meta-item">New</span>`}
                ${hotelAmenities.length ? `<span class="meta-item">${hotelAmenities.slice(0, 3).map((a) => a.name).join(" · ")}</span>` : ""}
              </div>
              ${promos.length ? `<div class="card-meta"><span class="meta-item promo-code" style="color: var(--forest); font-weight:700;">${promos.map((p) => `${p.promotionCode} −${p.discountPercentage}%`).join(" · ")}</span></div>` : ""}
              <div class="d-flex gap-3 mt-auto pt-3">
                <button
                  type="button"
                  class="btn-underline book-room-btn ${isBooked ? "text-success" : ""}"
                  data-hotel-id="${hotelId}"
                  data-hotel-name="${(h.name || "Hotel").replace(/"/g, '&quot;')}"
                  ${isBooked ? "disabled" : ""}
                >
                  ${isBooked ? "Booked" : "Book a room"}
                </button>
                <button
                  type="button"
                  class="btn-underline hotel-detail-btn"
                  data-hotel-id="${hotelId}"
                  data-hotel-name="${(h.name || "Hotel").replace(/"/g, '&quot;')}"
                >
                  Details &amp; reviews
                </button>
              </div>
            </div>
          </div>
        </div>`;
      })
      .join("");
  };

  if (searchBox) {
    searchBox.addEventListener("input", (e) => render(e.target.value));
  }
  render("");

  const reviewParam = new URLSearchParams(window.location.search).get("review");
  if (reviewParam) {
    const target = hotels.find((h) => Number(h.hotelId) === Number(reviewParam));
    if (target) {
      setTimeout(() => {
        window.openDetails(Number(target.hotelId), target.name || "Hotel");
        window.history.replaceState({}, "", "index.html");
      }, 150);
    }
  }

  window.openDetails = (hotelId, hotelName) => {
    const st = window.__reviewState;
    const hotel = (st.hotels || []).find((h) => Number(h.hotelId) === Number(hotelId));
    const hotelReviews = (st.reviews || []).filter((r) => Number(r.hotelId) === Number(hotelId));
    const hotelAmenities = (st.amenities || []).filter((a) => Number(a.hotelId) === Number(hotelId));
    const hotelRooms = (st.roomTypes || [])
      .filter((t) => Number(t.hotelId) === Number(hotelId))
      .map((t) => ({ type: t, rooms: (st.rooms || []).filter((r) => Number(r.roomTypeId) === Number(t.roomTypeId)) }));

    const nameEl = document.getElementById("hotelDetailTitle");
    const bodyEl = document.getElementById("hotelDetailBody");

    if (nameEl) nameEl.textContent = hotel ? `${hotel.name}, ${hotel.city || "City"}` : hotelName || "Hotel";
    if (!bodyEl) return;

    const avg = hotelReviews.length
      ? hotelReviews.reduce((s, r) => s + Number(r.rating), 0) / hotelReviews.length
      : null;

    bodyEl.innerHTML = `
      <div class="mb-4">
        ${hotel?.address ? `<p class="mb-0" style="color: var(--ink-soft);">${hotel.address}${hotel?.phone ? " · " + hotel.phone : ""}</p>` : ""}
        ${avg !== null ? `<p class="mb-0 mt-1"><span class="rating-dot">★</span> <strong>${avg.toFixed(1)}</strong> <span style="color:var(--ink-soft);">from ${hotelReviews.length} review${hotelReviews.length === 1 ? "" : "s"}</span></p>` : `<p class="mb-0 mt-1" style="color:var(--ink-soft);">No reviews yet — be the first.</p>`}
      </div>

      <h6 class="text-uppercase" style="letter-spacing:.1em;font-size:.74rem;font-weight:700;color:var(--ink-soft);">Rooms</h6>
      ${hotelRooms.length
        ? `<div class="mb-3">${hotelRooms.map(({ type, rooms: rms }) => `
          <div class="d-flex justify-content-between align-items-center border-bottom py-2" style="border-color: var(--line);">
            <div>
              <strong>${type.roomName}</strong> <span style="color: var(--ink-soft);">· ${type.capacity} guests</span><br />
              <small style="color: var(--ink-soft);">${rms.length} room${rms.length === 1 ? "" : "s"} ${rms.length ? "(" + rms.map((r) => r.roomNumber).join(", ") + ")" : ""}</small>
            </div>
            <div style="font-weight:700;">${Number(type.basePrice).toLocaleString("en-US", { style: "currency", currency: "USD" })}<small style="color:var(--ink-soft);font-weight:500;"> /night</small></div>
          </div>`).join("")}</div>`
        : `<p style="color: var(--ink-soft);">No rooms listed for this hotel yet.</p>`}

      <h6 class="text-uppercase" style="letter-spacing:.1em;font-size:.74rem;font-weight:700;color:var(--ink-soft);">Amenities</h6>
      ${hotelAmenities.length
        ? `<div class="d-flex flex-wrap gap-2 mb-3">${hotelAmenities.map((a) => `<span class="promo-pill" style="color:var(--forest);border-color:var(--line);background:transparent;">${a.name} · ${Number(a.price).toLocaleString("en-US", { style: "currency", currency: "USD" })}</span>`).join("")}</div>`
        : `<p style="color: var(--ink-soft);">No amenities listed.</p>`}

      <h6 class="text-uppercase" style="letter-spacing:.1em;font-size:.74rem;font-weight:700;color:var(--ink-soft);">Reviews</h6>
      <div id="hotelReviewsList" class="mb-3">
        ${hotelReviews.length ? hotelReviews.map((r) => `
          <div class="review-item mb-2">
            <div class="review-head"><span class="review-name">${r.user?.userName || `Guest #${r.userId}`}</span><span class="stars">${"★".repeat(Math.max(0, Math.round(r.rating)))}</span></div>
            <p class="review-comment">${r.comment || ""}</p>
          </div>`).join("") : `<p style="color: var(--ink-soft);">No reviews yet.</p>`}
      </div>

      ${st.currentUser ? `
      <h6 class="text-uppercase" style="letter-spacing:.1em;font-size:.74rem;font-weight:700;color:var(--ink-soft);">Leave a review</h6>
      <form id="reviewForm" class="review-form">
        <div class="mb-2">
          <label class="form-label">Rating</label>
          <select class="form-select" id="reviewRating">
            ${[5, 4, 3, 2, 1].map((n) => `<option value="${n}">${n} star${n === 1 ? "" : "s"}</option>`).join("")}
          </select>
        </div>
        <div class="mb-2">
          <label class="form-label">Comment</label>
          <textarea class="form-control" id="reviewComment" placeholder="Share your experience…"></textarea>
        </div>
        <div id="reviewError" class="text-danger small mb-2"></div>
        <button type="submit" class="btn-ink btn-ink-lg">Post review</button>
      </form>` : `
      <p class="mb-0 small" style="color: var(--ink-soft);"><a href="pages/login.html">Sign in</a> to leave a review.</p>`}`;

    const reviewForm = document.getElementById("reviewForm");
    if (reviewForm) reviewForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      const errEl = document.getElementById("reviewError");
      if (errEl) errEl.textContent = "";
      const rating = Number(document.getElementById("reviewRating").value);
      const comment = document.getElementById("reviewComment").value.trim();
      try {
        const created = await apiRequest("/Review", "POST", {
          userId: st.currentUser.userId,
          hotelId: Number(hotelId),
          rating,
          comment,
        });
        (st.reviews || []).push(created);
        if (grid) grid.innerHTML = "";
        render("");
        window.openDetails(hotelId, hotelName);
      } catch (err) {
        if (errEl) errEl.textContent = err.message;
      }
    });

    const detailModal = document.getElementById("hotelDetailModal");
    if (detailModal) bootstrap.Modal.getOrCreateInstance(detailModal).show();
  };
}

function renderPromoStrip(promotions) {
  const strip = document.getElementById("promoStrip");
  if (!strip) return;
  const active = (promotions || []).filter((p) => !p.expiryDate || new Date(p.expiryDate) >= new Date());
  if (!active.length) {
    strip.classList.add("d-none");
    return;
  }
  strip.classList.remove("d-none");
  strip.innerHTML = `
    <div>
      <div class="promo-kicker">Current offers</div>
      <div class="promo-code">${active.map((p) => `${p.promotionCode} — save ${p.discountPercentage}%`).join("  ·  ")}</div>
    </div>
    <div class="ms-lg-auto d-flex flex-wrap gap-2">${active.map((p) => `<span class="promo-pill">Valid until ${new Date(p.expiryDate).toLocaleDateString()}</span>`).join("")}</div>`;
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

// ---------- Profile page ----------
async function bindProfile() {
  const user = requireLogin();
  if (!user) return;
  const nameEl = document.getElementById("navUserName");
  if (nameEl) nameEl.textContent = `Hi, ${user.userName}`;

  const form = document.getElementById("profileForm");
  if (!form) return;

  let existing = null;
  try {
    const profiles = await apiRequest("/GuestProfile/GetAllGuestProfiles");
    existing = (profiles || []).find((p) => Number(p.userId) === Number(user.userId)) || null;
  } catch { /* no profile yet */ }

  if (existing) {
    const phone = document.getElementById("profilePhone");
    const address = document.getElementById("profileAddress");
    const dob = document.getElementById("profileDob");
    if (phone) phone.value = existing.gustPhone || "";
    if (address) address.value = existing.guestAddress || "";
    if (dob && existing.dateOfBirth) dob.value = new Date(existing.dateOfBirth).toISOString().split("T")[0];
  }

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const phone = document.getElementById("profilePhone").value.trim();
    const address = document.getElementById("profileAddress").value.trim();
    const dob = document.getElementById("profileDob").value;
    const submitBtn = document.getElementById("profileSubmit");
    if (!phone || !address || !dob) {
      showAlert("Please fill in all fields.", true);
      return;
    }
    if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = "Saving…"; }
    try {
      const payload = { gustPhone: phone, guestAddress: address, dateOfBirth: new Date(dob).toISOString(), userId: user.userId };
      if (existing) {
        await apiRequest(`/GuestProfile/UpdateGuestProfile?id=${existing.guestProfileId}`, "PUT", payload);
      } else {
        await apiRequest("/GuestProfile/CreateGuestProfile", "POST", payload);
      }
      showAlert("Profile saved.", false);
      setTimeout(() => window.location.reload(), 900);
    } catch (err) {
      showAlert(err.message, true);
      if (submitBtn) { submitBtn.disabled = false; submitBtn.textContent = "Save profile"; }
    }
  });
}

function getBookedHotelIds() {
  try {
    const saved = JSON.parse(localStorage.getItem("bookedHotelIds") || "[]");
    return Array.isArray(saved) ? saved.map(Number).filter((id) => Number.isFinite(id)) : [];
  } catch {
    return [];
  }
}

function setBookedHotelIds(ids) {
  localStorage.setItem("bookedHotelIds", JSON.stringify([...new Set(ids.map(Number).filter((id) => Number.isFinite(id)))]));
}

async function cancelBooking(bookingId) {
  try {
    await apiRequest(`/Booking/${bookingId}/status?newStatus=Cancelled`, "PATCH");
    window.location.reload();
  } catch (err) {
    alert(err.message || "Unable to cancel this booking right now.");
  }
}

// ---------- Bookings page ----------
async function loadBookings() {
  const user = requireLogin();
  if (!user) return;
  const [bookings, rooms, roomTypes, hotels, invoices, payments] = await Promise.all([
    apiRequest("/Booking"),
    apiRequest("/Room"),
    apiRequest("/RoomType/GetAllRoomTypes"),
    apiRequest("/Hotel"),
    apiRequest("/Invoice"),
    apiRequest("/Payment"),
  ]);

  const roomName = (id) => {
    const r = rooms.find((x) => x.roomId === id);
    if (!r) return `Room #${id}`;
    const t = roomTypes.find((y) => y.roomTypeId === r.roomTypeId);
    return `Room ${r.roomNumber}${t ? ` - ${t.roomName}` : ""}`;
  };

  const mine = bookings.filter((b) => b.userId === user.userId);
  const list = document.getElementById("bookingsList");

  const invoiceFor = (bookingId) => invoices.find((i) => i.bookingId === bookingId);
  const paidTotal = (bookingId) =>
    payments.filter((p) => p.bookingId === bookingId).reduce((s, p) => s + Number(p.amount || 0), 0);

  const statusClass = (s) =>
    s === "Confirmed" ? "status-confirmed" :
    s === "CheckedIn" ? "status-checkedin" :
    s === "Cancelled" ? "status-cancelled" : "status-pending";

  if (!mine.length) {
    list.innerHTML = `<div class="empty-msg">You have no stays yet.<br /><a href="../index.html">Find a hotel</a></div>`;
    return;
  }

  list.innerHTML = mine
    .slice()
    .sort((a, b) => new Date(b.checkInDate) - new Date(a.checkInDate))
    .map((b) => {
      const inv = invoiceFor(b.bookingId);
      const paid = paidTotal(b.bookingId);
      const due = inv && inv.status !== "Cancelled" ? Math.max(0, Number(inv.totalAmount) - paid) : 0;
      const room = rooms.find((x) => x.roomId === b.roomId);
      const rt = room && roomTypes.find((y) => y.roomTypeId === room.roomTypeId);
      const h = rt && hotels.find((z) => z.hotelId === rt.hotelId);
      const canPay = inv && inv.status !== "Paid" && b.status !== "Cancelled" && due > 0;
      const canReview = (b.status === "CheckedIn" || b.status === "Confirmed") && h;
      return `
      <div class="booking-card mb-3">
        <div class="d-flex justify-content-between flex-wrap gap-2 align-items-start">
          <div>
            <h3 class="hotel-name mb-0">${h ? h.name : "Hotel"}</h3>
            <p class="room-line mb-0">${roomName(b.roomId)}</p>
            <p class="date-line mb-1">
              ${new Date(b.checkInDate).toLocaleDateString()} &rarr; ${new Date(b.checkOutDate).toLocaleDateString()}
            </p>
            <small class="booking-id">Booking #${b.bookingId}</small>
            ${inv ? `
              <p class="room-line mb-1 mt-2">
                Invoice #${inv.invoiceId} — ${Number(inv.totalAmount).toLocaleString("en-US", { style: "currency", currency: "USD" })}
                <span class="status-badge ${inv.status === "Paid" ? "status-confirmed" : "status-pending"}">${inv.status}</span>
                ${paid > 0 && due === 0 ? `<small class="text-success fw-semibold"> · Paid ${paid.toLocaleString("en-US", { style: "currency", currency: "USD" })}</small>` : ""}
              </p>` : ""}
          </div>
          <div class="d-flex flex-column align-items-end gap-2">
            <span class="status-badge ${statusClass(b.status)}">${b.status}</span>
            ${b.status === "Cancelled" ? "" : `<button type="button" class="btn btn-sm btn-outline-danger cancel-booking-btn" data-booking-id="${b.bookingId}">Cancel booking</button>`}
            ${canPay ? `<button type="button" class="btn btn-sm btn-ink pay-invoice-btn" data-invoice-id="${inv.invoiceId}" data-booking-id="${b.bookingId}">Pay ${due.toLocaleString("en-US", { style: "currency", currency: "USD" })}</button>` : ""}
            ${canReview ? `<a class="btn btn-sm btn-underline" href="../index.html?review=${h.hotelId}">Leave a review</a>` : ""}
          </div>
        </div>
      </div>`;
    })
    .join("");

  document.querySelectorAll(".cancel-booking-btn").forEach((button) => {
    button.addEventListener("click", async () => {
      const bookingId = Number(button.dataset.bookingId);
      if (!Number.isFinite(bookingId)) return;
      await cancelBooking(bookingId);
    });
  });

  document.querySelectorAll(".pay-invoice-btn").forEach((button) => {
    button.addEventListener("click", async () => {
      const invoiceId = Number(button.dataset.invoiceId);
      const bookingId = Number(button.dataset.bookingId);
      if (!Number.isFinite(invoiceId)) return;
      const inv = invoiceFor(bookingId);
      if (!inv) return;
      const due = Math.max(0, Number(inv.totalAmount) - paidTotal(bookingId));
      button.disabled = true;
      button.textContent = "Processing…";
      try {
        await apiRequest("/Payment", "POST", {
          bookingId,
          amount: due,
          method: "Card",
          paidAt: new Date().toISOString(),
        });
        await apiRequest(`/Invoice/${invoiceId}/status?newStatus=Paid`, "PATCH");
        adminFriendlyToast("Payment recorded. Invoice marked as paid.");
        window.location.reload();
      } catch (err) {
        button.disabled = false;
        button.textContent = "Pay again";
        alert(err.message || "Payment failed. Please try again.");
      }
    });
  });
}

function adminFriendlyToast(message) {
  try {
    const t = document.getElementById("bookingsToast");
    if (t) {
      t.textContent = message;
      t.classList.remove("d-none");
      setTimeout(() => t.classList.add("d-none"), 3000);
    }
  } catch { /* noop */ }
}

// ---------- Boot by page ----------
document.addEventListener("DOMContentLoaded", () => {
  updateNav();
  const page = window.location.pathname.split("/").pop();

  if (page === "index.html" || page === "") loadHome();
  else if (page === "login.html") bindLogin();
  else if (page === "register.html") bindRegister();
  else if (page === "bookings.html") loadBookings();
  else if (page === "profile.html") bindProfile();
});