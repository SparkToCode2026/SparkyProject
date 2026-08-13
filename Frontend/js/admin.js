// Admin console: data-driven CRUD for every entity in the Sparky API.
/* global apiRequest, authUser, logout, bootstrap */

const toastEl = () => document.getElementById("toast");
function adminToast(message, ok = true) {
  const t = toastEl();
  if (!t) return;
  const body = t.querySelector(".toast-body");
  body.textContent = message;
  t.classList.remove("bg-danger", "bg-success");
  t.classList.add(ok ? "bg-success" : "bg-danger", "text-white");
  bootstrap.Toast.getOrCreateInstance(t, { delay: 2500 }).show();
}

const fmtMoney = (n) =>
  Number(n).toLocaleString("en-US", { style: "currency", currency: "USD" });
const fmtDate = (iso) => (iso ? new Date(iso).toLocaleDateString() : "");
const inputDate = (iso) => (iso ? new Date(iso).toISOString().split("T")[0] : "");
const toIso = (v) => (v ? new Date(v).toISOString() : null);

const STATUS_BADGE = (s) => {
  const cls =
    s === "Confirmed" || s === "Paid" || s === "Available" ? "status-confirmed" :
    s === "CheckedIn" ? "status-checkedin" :
    s === "Cancelled" || s === "Unavailable" || s === "Unpaid" ? "status-cancelled" : "status-pending";
  return `<span class="status-badge ${cls}">${s || "—"}</span>`;
};
const stars = (n) => "★".repeat(Math.max(0, Math.round(n))) + "☆".repeat(Math.max(0, 5 - Math.round(n)));

// Field type helpers
const fieldControl = (f, value) => {
  const v = value ?? (f.type === "date" ? "" : f.default ?? "");
  const id = `f_${f.key}`;
  const required = f.required ? "required" : "";
  const opts = f.type === "date" && value ? inputDate(value) : v;
  if (f.type === "select") {
    const options = (f.options || [])
      .map((o) => `<option value="${o}" ${String(v) === String(o) ? "selected" : ""}>${o}</option>`)
      .join("");
    return `<select class="form-select" id="${id}" ${required}>${options}</select>`;
  }
  const inputType = f.type === "date" ? "date" : f.type === "number" ? "number" : f.type === "email" ? "email" : "text";
  const step = f.type === "number" && f.step ? ` step="${f.step}"` : "";
  return `<input type="${inputType}" class="form-control" id="${id}" value="${opts ?? ""}" ${step} ${required} />`;
};

const fieldValue = (f) => {
  const el = document.getElementById(`f_${f.key}`);
  if (!el) return undefined;
  let v = el.value;
  if (f.type === "number") v = v === "" ? null : Number(v);
  if (f.type === "date") v = toIso(v);
  return v;
};

// ---------- Entity definitions ----------
const ENTITIES = {
  overview: {
    label: "Overview",
    rendover: true,
  },
  hotels: {
    label: "Hotels",
    list: { method: "GET", path: "/Hotel" },
    create: { method: "POST", path: "/Hotel" },
    update: { method: "PUT", path: (id) => `/Hotel/${id}` },
    quick: { label: "Change city", method: "PATCH", path: (id) => `/Hotel/${id}/city`, paramKey: "newCity" },
    remove: { method: "DELETE", path: (id) => `/Hotel/${id}` },
    idKey: "hotelId",
    columns: [
      { key: "hotelId", label: "#" },
      { key: "name", label: "Name", cell: (h) => `<strong>${h.name || "—"}</strong>` },
      { key: "city", label: "City" },
      { key: "address", label: "Address" },
      { key: "phone", label: "Phone" },
      { key: "roomCount", label: "Room types", cell: (h) => (h.roomTypes || []).length },
    ],
    filters: [
      { label: "By city", prompt: "City", method: "GET", path: (city) => `/Hotel/by-city/${encodeURIComponent(city)}`, param: "city" },
      { label: "Sorted by name", method: "GET", path: () => "/Hotel/sorted-by-name" },
    ],
    fields: [
      { key: "name", label: "Name", type: "text", required: true },
      { key: "city", label: "City", type: "text" },
      { key: "address", label: "Address", type: "text" },
      { key: "phone", label: "Phone", type: "text" },
      { key: "userId", label: "Owner user ID (optional)", type: "number", nullable: true },
    ],
  },
  rooms: {
    label: "Rooms",
    list: { method: "GET", path: "/Room" },
    create: { method: "POST", path: "/Room" },
    update: { method: "PUT", path: (id) => `/Room/${id}` },
    quick: { label: "Change status", method: "PATCH", path: (id) => `/Room/${id}/status`, paramKey: "newStatus" },
    remove: { method: "DELETE", path: (id) => `/Room/${id}` },
    idKey: "roomId",
    columns: [
      { key: "roomId", label: "#" },
      { key: "roomNumber", label: "Number" },
      { key: "status", label: "Status", cell: (r) => STATUS_BADGE(r.status) },
      { key: "roomTypeId", label: "Room type" },
    ],
    filters: [
      { label: "By status", prompt: "Status", method: "GET", path: (s) => `/Room/by-status/${encodeURIComponent(s)}`, param: "status" },
      { label: "Sorted by number", method: "GET", path: () => "/Room/sorted-by-number" },
    ],
    fields: [
      { key: "roomNumber", label: "Room number", type: "text", required: true },
      { key: "status", label: "Status", type: "select", required: true, options: ["Available", "Occupied", "Maintenance"] },
      { key: "roomTypeId", label: "Room type ID", type: "number", required: true },
    ],
  },
  roomtypes: {
    label: "Room types",
    list: { method: "GET", path: "/RoomType/GetAllRoomTypes" },
    create: { method: "POST", path: "/RoomType/CreateRoomType" },
    update: { method: "PUT", path: (id) => `/RoomType/UpdateRoomType?id=${id}` },
    quick: { label: "Move to hotel", method: "PATCH", path: (id) => `/RoomType/UpdateHotelIdForRoomType?id=${id}`, paramKey: "newHotelID" },
    remove: { method: "DELETE", path: (id) => `/RoomType/RemoveRoomType?id=${id}` },
    idKey: "roomTypeId",
    columns: [
      { key: "roomTypeId", label: "#" },
      { key: "roomName", label: "Name", cell: (t) => `<strong>${t.roomName || "—"}</strong>` },
      { key: "basePrice", label: "Base price", cell: (t) => fmtMoney(t.basePrice) },
      { key: "capacity", label: "Capacity" },
      { key: "hotelId", label: "Hotel" },
    ],
    filters: [
      { label: "Capacity ≥", prompt: "Min capacity", method: "GET", path: (c) => `/RoomType/GetRoomTypesByCapacity?minCapacity=${encodeURIComponent(c)}`, param: "minCapacity" },
      { label: "Sorted by price", method: "GET", path: () => "/RoomType/GetRoomTypesSortedByPrice" },
    ],
    fields: [
      { key: "roomName", label: "Room name", type: "text", required: true },
      { key: "basePrice", label: "Base price (USD)", type: "number", required: true, step: "0.01" },
      { key: "capacity", label: "Capacity (guests)", type: "number", required: true },
      { key: "hotelId", label: "Hotel ID", type: "number", required: true },
    ],
  },
  amenities: {
    label: "Amenities",
    list: { method: "GET", path: "/Amenity" },
    create: { method: "POST", path: "/Amenity" },
    update: { method: "PUT", path: (id) => `/Amenity/${id}` },
    quick: { label: "Change price", method: "PATCH", path: (id) => `/Amenity/${id}/price`, paramKey: "newPrice" },
    remove: { method: "DELETE", path: (id) => `/Amenity/${id}` },
    idKey: "id",
    columns: [
      { key: "id", label: "#" },
      { key: "name", label: "Name", cell: (a) => `<strong>${a.name || "—"}</strong>` },
      { key: "price", label: "Price", cell: (a) => fmtMoney(a.price) },
      { key: "hotelId", label: "Hotel" },
    ],
    filters: [
      { label: "By hotel", prompt: "Hotel ID", method: "GET", path: (id) => `/Amenity/by-hotel/${encodeURIComponent(id)}`, param: "hotelId" },
      { label: "Sorted by price", method: "GET", path: () => "/Amenity/sorted-by-price" },
    ],
    fields: [
      { key: "name", label: "Name", type: "text", required: true },
      { key: "price", label: "Price (USD)", type: "number", required: true, step: "0.01" },
      { key: "hotelId", label: "Hotel ID", type: "number", required: true },
    ],
  },
  promotions: {
    label: "Promotions",
    list: { method: "GET", path: "/Promotion/GetAllPromotions" },
    create: { method: "POST", path: "/Promotion/CreatePromotion" },
    update: { method: "PUT", path: (id) => `/Promotion/UpdatePromotion?id=${id}` },
    quick: { label: "Move to hotel", method: "PATCH", path: (id) => `/Promotion/UpdatePromotionHotel?id=${id}`, paramKey: "newHotelId" },
    remove: { method: "DELETE", path: (id) => `/Promotion/RemovePromotion?id=${id}` },
    idKey: "promotionId",
    columns: [
      { key: "promotionId", label: "#" },
      { key: "promotionCode", label: "Code", cell: (p) => `<strong>${p.promotionCode || "—"}</strong>` },
      { key: "discountPercentage", label: "Discount", cell: (p) => `${p.discountPercentage}%` },
      { key: "expiryDate", label: "Expires", cell: (p) => fmtDate(p.expiryDate) },
      { key: "hotelId", label: "Hotel" },
    ],
    filters: [
      { label: "Expiring after", prompt: "Date", method: "GET", path: (d) => `/Promotion/GetPromotionsByExpiryDate?expiryDate=${encodeURIComponent(d)}`, param: "expiryDate", type: "date" },
      { label: "Top discounts", method: "GET", path: () => "/Promotion/GetPromotionsSortedByDiscount" },
    ],
    fields: [
      { key: "promotionCode", label: "Promotion code", type: "text", required: true },
      { key: "discountPercentage", label: "Discount %", type: "number", required: true, step: "0.01" },
      { key: "expiryDate", label: "Expiry date", type: "date", required: true },
      { key: "hotelId", label: "Hotel ID", type: "number", required: true },
    ],
  },
  reviews: {
    label: "Reviews",
    list: { method: "GET", path: "/Review" },
    create: { method: "POST", path: "/Review" },
    update: { method: "PUT", path: (id) => `/Review/${id}` },
    quick: { label: "Change rating", method: "PATCH", path: (id) => `/Review/${id}/rating`, paramKey: "newRating" },
    remove: { method: "DELETE", path: (id) => `/Review/${id}` },
    idKey: "reviewId",
    columns: [
      { key: "reviewId", label: "#" },
      { key: "hotelId", label: "Hotel" },
      { key: "rating", label: "Rating", cell: (r) => `<span class="stars">${stars(r.rating)}</span>` },
      { key: "comment", label: "Comment", cell: (r) => (r.comment ? r.comment.slice(0, 60) : "—") },
      { key: "createdAt", label: "Date", cell: (r) => fmtDate(r.createdAt) },
    ],
    filters: [
      { label: "By rating", prompt: "Rating (1-5)", method: "GET", path: (n) => `/Review/by-rating/${encodeURIComponent(n)}`, param: "rating" },
      { label: "Sorted by rating", method: "GET", path: () => "/Review/sorted-by-rating" },
    ],
    fields: [
      { key: "userId", label: "User ID", type: "number", required: true },
      { key: "hotelId", label: "Hotel ID (optional)", type: "number", nullable: true },
      { key: "rating", label: "Rating (1–5)", type: "number", required: true, min: 1, max: 5 },
      { key: "comment", label: "Comment", type: "text" },
    ],
  },
  payments: {
    label: "Payments",
    list: { method: "GET", path: "/Payment" },
    create: { method: "POST", path: "/Payment" },
    update: { method: "PUT", path: (id) => `/Payment/${id}` },
    quick: { label: "Change amount", method: "PATCH", path: (id) => `/Payment/${id}/amount`, paramKey: "newAmount" },
    remove: { method: "DELETE", path: (id) => `/Payment/${id}` },
    idKey: "paymentId",
    columns: [
      { key: "paymentId", label: "#" },
      { key: "bookingId", label: "Booking" },
      { key: "amount", label: "Amount", cell: (p) => fmtMoney(p.amount) },
      { key: "method", label: "Method" },
      { key: "paidAt", label: "Paid at", cell: (p) => fmtDate(p.paidAt) },
    ],
    filters: [
      { label: "By method", prompt: "Method", method: "GET", path: (m) => `/Payment/by-method/${encodeURIComponent(m)}`, param: "method" },
      { label: "Sorted by amount", method: "GET", path: () => "/Payment/sorted-by-amount" },
    ],
    fields: [
      { key: "bookingId", label: "Booking ID", type: "number", required: true },
      { key: "amount", label: "Amount (USD)", type: "number", required: true, step: "0.01" },
      { key: "method", label: "Method", type: "select", required: true, options: ["Card", "Cash", "Bank transfer", "Wallet"] },
      { key: "paidAt", label: "Paid at", type: "date", required: true },
    ],
  },
  invoices: {
    label: "Invoices",
    list: { method: "GET", path: "/Invoice" },
    create: { method: "POST", path: "/Invoice" },
    update: { method: "PUT", path: (id) => `/Invoice/${id}` },
    quick: { label: "Change status", method: "PATCH", path: (id) => `/Invoice/${id}/status`, paramKey: "newStatus" },
    remove: { method: "DELETE", path: (id) => `/Invoice/${id}` },
    idKey: "invoiceId",
    columns: [
      { key: "invoiceId", label: "#" },
      { key: "bookingId", label: "Booking" },
      { key: "totalAmount", label: "Total", cell: (i) => fmtMoney(i.totalAmount) },
      { key: "status", label: "Status", cell: (i) => STATUS_BADGE(i.status) },
      { key: "issueDate", label: "Issued", cell: (i) => fmtDate(i.issueDate) },
    ],
    filters: [
      { label: "By status", prompt: "Status", method: "GET", path: (s) => `/Invoice/by-status/${encodeURIComponent(s)}`, param: "status" },
      { label: "Sorted by issue date", method: "GET", path: () => "/Invoice/sorted-by-issue-date" },
    ],
    fields: [
      { key: "bookingId", label: "Booking ID", type: "number", required: true },
      { key: "totalAmount", label: "Total (USD)", type: "number", required: true, step: "0.01" },
      { key: "status", label: "Status", type: "select", required: true, options: ["Unpaid", "Paid", "Cancelled"] },
      { key: "issueDate", label: "Issue date", type: "date", required: true },
    ],
  },
  bookings: {
    label: "Bookings",
    list: { method: "GET", path: "/Booking" },
    create: { method: "POST", path: "/Booking" },
    update: { method: "PUT", path: (id) => `/Booking/${id}` },
    quick: { label: "Change status", method: "PATCH", path: (id) => `/Booking/${id}/status`, paramKey: "newStatus" },
    remove: { method: "DELETE", path: (id) => `/Booking/${id}` },
    idKey: "bookingId",
    columns: [
      { key: "bookingId", label: "#" },
      { key: "userId", label: "Guest" },
      { key: "roomId", label: "Room" },
      { key: "checkInDate", label: "From", cell: (b) => fmtDate(b.checkInDate) },
      { key: "checkOutDate", label: "To", cell: (b) => fmtDate(b.checkOutDate) },
      { key: "status", label: "Status", cell: (b) => STATUS_BADGE(b.status) },
    ],
    filters: [
      { label: "By status", prompt: "Status", method: "GET", path: (s) => `/Booking/by-status/${encodeURIComponent(s)}`, param: "status" },
      { label: "Sorted by check-in", method: "GET", path: () => "/Booking/sorted-by-checkin" },
    ],
    fields: [
      { key: "userId", label: "Guest user ID", type: "number", required: true },
      { key: "roomId", label: "Room ID", type: "number", required: true },
      { key: "promotionId", label: "Promotion ID (optional)", type: "number", nullable: true },
      { key: "checkInDate", label: "Check-in", type: "date", required: true },
      { key: "checkOutDate", label: "Check-out", type: "date", required: true },
      { key: "status", label: "Status", type: "select", required: true, options: ["Pending", "Confirmed", "CheckedIn", "Cancelled"] },
    ],
  },
  guestprofiles: {
    label: "Guest profiles",
    list: { method: "GET", path: "/GuestProfile/GetAllGuestProfiles" },
    create: { method: "POST", path: "/GuestProfile/CreateGuestProfile" },
    update: { method: "PUT", path: (id) => `/GuestProfile/UpdateGuestProfile?id=${id}` },
    quick: { label: "Change address", method: "PATCH", path: (id) => `/GuestProfile/UpdateAddressForProfile?id=${id}`, paramKey: "newAddress" },
    remove: { method: "DELETE", path: (id) => `/GuestProfile/DeleteGuestProfile?id=${id}` },
    idKey: "guestProfileId",
    columns: [
      { key: "guestProfileId", label: "#" },
      { key: "userId", label: "User" },
      { key: "gustPhone", label: "Phone" },
      { key: "guestAddress", label: "Address" },
      { key: "dateOfBirth", label: "Birthday", cell: (g) => fmtDate(g.dateOfBirth) },
    ],
    filters: [
      { label: "By address", prompt: "Address", method: "GET", path: (a) => `/GuestProfile/GetGuestProfilesByAddress?address=${encodeURIComponent(a)}`, param: "address" },
      { label: "By birth date", method: "GET", path: () => "/GuestProfile/GetGuestProfilesByDateOfBirth" },
    ],
    fields: [
      { key: "gustPhone", label: "Phone", type: "text", required: true },
      { key: "guestAddress", label: "Address", type: "text", required: true },
      { key: "dateOfBirth", label: "Date of birth", type: "date", required: true },
      { key: "userId", label: "User ID", type: "number", required: true },
    ],
  },
  staff: {
    label: "Staff",
    list: { method: "GET", path: "/Staff" },
    create: { method: "POST", path: "/Staff" },
    update: { method: "PUT", path: (id) => `/Staff/${id}` },
    quick: { label: "Change position", method: "PATCH", path: (id) => `/Staff/${id}/position`, paramKey: "newPosition" },
    remove: { method: "DELETE", path: (id) => `/Staff/${id}` },
    idKey: "staffId",
    columns: [
      { key: "staffId", label: "#" },
      { key: "fullName", label: "Name", cell: (s) => `<strong>${s.fullName || "—"}</strong>` },
      { key: "position", label: "Position" },
      { key: "email", label: "Email" },
      { key: "phone", label: "Phone" },
      { key: "hireDate", label: "Hired", cell: (s) => fmtDate(s.hireDate) },
    ],
    filters: [
      { label: "By position", prompt: "Position", method: "GET", path: (p) => `/Staff/by-position/${encodeURIComponent(p)}`, param: "position" },
      { label: "By hire date", method: "GET", path: () => "/Staff/sorted-by-hire-date" },
    ],
    fields: [
      { key: "fullName", label: "Full name", type: "text", required: true },
      { key: "position", label: "Position", type: "text" },
      { key: "email", label: "Email", type: "email" },
      { key: "phone", label: "Phone", type: "text" },
      { key: "userId", label: "User ID (optional)", type: "number", nullable: true },
    ],
  },
  users: {
    label: "Users",
    list: { method: "GET", path: "/User/GetAllUsers" },
    create: { method: "POST", path: "/User/CreateUser" },
    update: { method: "PUT", path: (id) => `/User/UpdateUser?id=${id}` },
    quick: { label: "Change role", method: "PATCH", path: (id) => `/User/UpdateUserRole?id=${id}`, paramKey: "newRole" },
    remove: { method: "DELETE", path: (id) => `/User/DeleteUser?id=${id}` },
    idKey: "userId",
    columns: [
      { key: "userId", label: "#" },
      { key: "userName", label: "Name", cell: (u) => `<strong>${u.userName || "—"}</strong>` },
      { key: "userEmail", label: "Email" },
      { key: "role", label: "Role", cell: (u) => STATUS_BADGE(u.role) },
    ],
    filters: [
      { label: "By role", prompt: "Role", method: "GET", path: (r) => `/User/GetUsersByRole?role=${encodeURIComponent(r)}`, param: "role" },
      { label: "Counts by role", method: "GET", path: () => "/User/GetUserCountByRole" },
    ],
    fields: [
      { key: "userName", label: "Full name", type: "text", required: true },
      { key: "userEmail", label: "Email", type: "email", required: true },
      { key: "passwordHash", label: "Password hash", type: "text", required: true },
      { key: "role", label: "Role", type: "select", required: true, options: ["Guest", "Staff", "Admin"] },
    ],
  },
};

const modal = document.getElementById("entityModal");
const bsModal = () => bootstrap.Modal.getOrCreateInstance(modal);
let currentEntity = null;
let editingRow = null;

// ---------- Overview ----------
async function renderOverview() {
  const panel = document.getElementById("adminPanel");
  let revenue = 0, avgRating = 0, avgAmenity = 0, roleCounts = [];
  try { revenue = (await apiRequest("/Invoice/total-revenue")) || 0; } catch { /* ignore */ }
  try { avgRating = (await apiRequest("/Review/average-rating")) || 0; } catch { /* ignore */ }
  try { avgAmenity = (await apiRequest("/Amenity/average-price")) || 0; } catch { /* ignore */ }
  try { roleCounts = (await apiRequest("/User/GetUserCountByRole")) || []; } catch { /* ignore */ }

  const roleSummary = roleCounts.length
    ? `<p class="stat-note">${roleCounts.map((r) => `${r.role}: <strong>${r.count}</strong>`).join(" &middot; ")}</p>`
    : "";

  panel.innerHTML = `
    <div class="admin-panel-head">
      <div>
        <h2>Overview</h2>
        <p class="sub">Live totals pulled from the aggregate endpoints.</p>
      </div>
    </div>
    <div class="stat-grid">
      <div class="stat-card"><div class="stat-label">Total revenue</div><div class="stat-value">${fmtMoney(revenue)}</div></div>
      <div class="stat-card"><div class="stat-label">Average rating</div><div class="stat-value">${Number(avgRating).toFixed(1)}<span class="unit"> / 5</span></div></div>
      <div class="stat-card"><div class="stat-label">Avg amenity price</div><div class="stat-value">${fmtMoney(avgAmenity)}</div></div>
      <div class="stat-card"><div class="stat-label">Users</div><div class="stat-value">${roleCounts.reduce((s, r) => s + r.count, 0)}</div>${roleSummary}</div>
    </div>
    <div class="admin-empty">Open a section from the sidebar to manage records, add new entries, and run the filter &amp; sort endpoints.</div>`;
}

// ---------- Tables ----------
function tableRows(entity, rows) {
  const idKey = entity.idKey;
  return rows.map((row) => {
    const id = row[idKey];
    const cells = entity.columns
      .map((c) => `<td>${c.cell ? c.cell(row) : row[c.key] ?? "—"}</td>`)
      .join("");
    const actions = Number.isFinite(Number(id))
      ? `
      <div class="row-actions">
        ${entity.quick ? `<button class="action-btn" onclick="adminQuick('${entityNameKey(entity)}', ${id})">${entity.quick.label}</button>` : ""}
        <button class="action-btn" onclick="adminEdit('${entityNameKey(entity)}', ${id})">Edit</button>
        <button class="action-btn danger" onclick="adminDelete('${entityNameKey(entity)}', ${id})">Delete</button>
      </div>`
      : `<span style="color:var(--ink-soft);font-size:.8rem;">—</span>`;
    return `<tr>${cells}<td class="text-end">${actions}</td></tr>`;
  }).join("");
}

function entityNameKey(entity) {
  return Object.keys(ENTITIES).find((k) => ENTITIES[k] === entity) || "";
}

async function renderEntity(name) {
  const entity = ENTITIES[name];
  currentEntity = name;
  editingRow = null;
  const panel = document.getElementById("adminPanel");
  panel.innerHTML = `<div class="admin-empty">Loading ${entity.label.toLowerCase()}…</div>`;

  let rows;
  try {
    rows = await apiRequest(entity.list.path, entity.list.method);
  } catch (err) {
    panel.innerHTML = `<div class="admin-empty">Could not load ${entity.label.toLowerCase()}: ${err.message}</div>`;
    return;
  }
  if (rows && !Array.isArray(rows)) rows = [rows];

  const cols = entity.columns
    .map((c) => `<th>${c.label}</th>`)
    .join("") + `<th></th>`;

  const filterBar = entity.filters && entity.filters.length
    ? `<div class="d-flex flex-wrap gap-2 mt-3">
        ${entity.filters.map((f, i) => `
          <span class="d-inline-flex gap-1 align-items-center">
            <small class="text-muted">${f.label}:</small>
            ${f.prompt ? `<input class="form-control form-control-sm" style="width:130px" id="flt_${i}" ${f.type === "date" ? 'type="date"' : ""} />` : ""}
            <button class="action-btn" onclick="adminFilter('${name}', ${i})">Run</button>
          </span>`).join("")}
      </div>`
    : "";

  panel.innerHTML = `
    <div class="admin-panel-head">
      <div>
        <h2>${entity.label}</h2>
        <p class="sub">${rows.length} record${rows.length === 1 ? "" : "s"}</p>
      </div>
      <button class="btn-ink" onclick="adminCreate('${name}')">Add ${entity.label.replace(/s$/, "")}</button>
    </div>
    <div class="admin-table-wrap">
      <table class="admin-table">
        <thead><tr>${cols}</tr></thead>
        <tbody>${rows.length ? tableRows(entity, rows) : `<tr><td colspan="99"><div class="admin-empty">Nothing here yet — add a ${entity.label.replace(/s$/, "").toLowerCase()}.</div></td></tr>`}</tbody>
      </table>
    </div>
    ${filterBar}`;
}

async function adminFilter(name, i) {
  const entity = ENTITIES[name];
  const f = entity.filters[i];
  const input = document.getElementById(`flt_${i}`);
  const value = input ? input.value : "";
  const path = f.path(value);
  try {
    const res = await apiRequest(path, f.method);
    const rows = Array.isArray(res) ? res : [res];
    adminToast(`Filter returned ${rows.length} result${rows.length === 1 ? "" : "s"}`);
    const panel = document.getElementById("adminPanel");
    const head = panel.querySelector(".admin-panel-head");
    const filterBar = panel.querySelector(".d-flex.flex-wrap.gap-2");
    const cols = entity.columns.map((c) => `<th>${c.label}</th>`).join("") + "<th></th>";
    const newTable = document.createElement("div");
    newTable.innerHTML = `<table class="admin-table"><thead><tr>${cols}</tr></thead><tbody>${rows.length ? tableRows(entity, rows) : `<tr><td colspan="99"><div class="admin-empty">No matches.</div></td></tr>`}</tbody></table>`;
    if (head) head.insertAdjacentElement("afterend", newTable.firstElementChild);
    if (filterBar) filterBar.remove();
  } catch (err) {
    adminToast(err.message, false);
  }
}

// ---------- Create / edit ----------
function formHtml(entity, values) {
  return entity.fields.map((f) => `
    <div class="mb-3">
      <label class="form-label" for="f_${f.key}">${f.label}</label>
      ${fieldControl(f, values ? values[f.key] : undefined)}
    </div>`).join("");
}

async function adminCreate(name) {
  const entity = ENTITIES[name];
  editingRow = null;
  document.getElementById("entityModalTitle").textContent = `Add ${entity.label.replace(/s$/, "")}`;
  document.getElementById("entityModalBody").innerHTML = `
    <form id="entityForm">${formHtml(entity)}<div id="entityError" class="text-danger small mb-2"></div>
    <button type="submit" class="btn-ink w-100 btn-ink-lg">Create</button></form>`;
  bsModal().show();
  document.getElementById("entityForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    await submitForm(entity, name);
  });
}

async function adminEdit(name, id) {
  const entity = ENTITIES[name];
  let row;
  try {
    row = (await apiRequest(entity.list.path.replace(/\/$/, ""), "GET"))?.find((r) => r[entity.idKey] === Number(id));
  } catch { row = null; }
  if (!row) { adminToast("Could not load this record.", false); return; }

  editingRow = { id, row };
  document.getElementById("entityModalTitle").textContent = `Edit ${entity.label.replace(/s$/, "")} #${id}`;
  document.getElementById("entityModalBody").innerHTML = `
    <form id="entityForm">${formHtml(entity, row)}<div id="entityError" class="text-danger small mb-2"></div>
    <button type="submit" class="btn-ink w-100 btn-ink-lg">Save changes</button></form>`;
  bsModal().show();
  document.getElementById("entityForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    await submitForm(entity, name, true);
  });
}

async function submitForm(entity, name, isEdit) {
  const errEl = document.getElementById("entityError");
  if (errEl) errEl.textContent = "";
  const payload = {};
  let valid = true;
  for (const f of entity.fields) {
    const v = fieldValue(f);
    if (f.required && (v === "" || v === null || v === undefined)) { valid = false; }
    payload[f.key] = v;
  }
  if (!valid) {
    if (errEl) errEl.textContent = "Please fill in all required fields.";
    return;
  }

  try {
    let method, path;
    if (isEdit) {
      method = entity.update.method;
      path = entity.update.path(editingRow.id);
    } else {
      method = entity.create.method;
      path = entity.create.path;
    }
    await apiRequest(path, method, payload);
    adminToast(isEdit ? `${entity.label} updated.` : `${entity.label.replace(/s$/, "")} created.`);
    bsModal().hide();
    renderEntity(name);
  } catch (err) {
    if (errEl) errEl.textContent = err.message;
  }
}

async function adminQuick(name, id) {
  const entity = ENTITIES[name];
  if (!entity.quick) return;
  editingRow = { id, quick: true };
  const q = entity.quick;
  document.getElementById("entityModalTitle").textContent = `${q.label} — #${id}`;
  document.getElementById("entityModalBody").innerHTML = `
    <form id="entityForm">
      <div class="mb-3"><label class="form-label" for="q_value">${q.label}</label>
      <input class="form-control" id="q_value" ${q.type === "date" ? 'type="date"' : ""} /></div>
      <div id="entityError" class="text-danger small mb-2"></div>
      <button type="submit" class="btn-ink w-100 btn-ink-lg">Apply</button>
    </form>`;
  bsModal().show();
  document.getElementById("entityForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const value = document.getElementById("q_value").value;
    if (!value) {
      document.getElementById("entityError").textContent = "Please enter a value.";
      return;
    }
    try {
      const basePath = q.path(id);
      const sep = basePath.includes("?") ? "&" : "?";
      await apiRequest(`${basePath}${sep}${q.paramKey}=${encodeURIComponent(value)}`, q.method);
      adminToast(`${q.label} saved.`);
      bsModal().hide();
      renderEntity(name);
    } catch (err) {
      document.getElementById("entityError").textContent = err.message;
    }
  });
}

async function adminDelete(name, id) {
  const entity = ENTITIES[name];
  if (!window.confirm(`Delete ${entity.label.replace(/s$/, "").toLowerCase()} #${id}?`)) return;
  try {
    await apiRequest(entity.remove.path(id), entity.remove.method);
    adminToast(`${entity.label.replace(/s$/, "")} deleted.`);
    renderEntity(name);
  } catch (err) {
    adminToast(err.message, false);
  }
}

// ---------- Sidebar ----------
async function renderNav() {
  const nav = document.getElementById("adminNav");
  nav.innerHTML = Object.keys(ENTITIES)
    .map((k) => `<button class="admin-link ${k === "overview" ? "active" : ""}" id="nav_${k}" data-entity="${k}">${ENTITIES[k].label}<span class="count" id="cnt_${k}"></span></button>`)
    .join("");

  nav.addEventListener("click", (e) => {
    const btn = e.target.closest(".admin-link");
    if (!btn) return;
    document.querySelectorAll(".admin-link").forEach((a) => a.classList.remove("active"));
    btn.classList.add("active");
    if (btn.dataset.entity === "overview") renderOverview();
    else renderEntity(btn.dataset.entity);
  });

  // Fill counts quietly.
  for (const k of Object.keys(ENTITIES)) {
    if (k === "overview") continue;
    const el = document.getElementById(`cnt_${k}`);
    try {
      const res = await apiRequest(ENTITIES[k].list.path, ENTITIES[k].list.method);
      const n = Array.isArray(res) ? res.length : res ? 1 : 0;
      if (el) el.textContent = n;
    } catch { /* no count */ }
  }
}

document.addEventListener("DOMContentLoaded", async () => {
  if (!requireLogin()) return;
  const user = authUser();
  if (user) {
    const name = document.getElementById("navUserName");
    if (name) name.textContent = `Hi, ${user.userName}`;
  }
  renderOverview();
  renderNav();
});