// ===== SIGNALR HUB CONNECTIONS =====

// --- TurnstileHub ---
const turnstileConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/turnstile")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

// --- CameraHub ---
const cameraConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/camera")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

// --- AlertHub ---
const alertConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/alert")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

// --- DashboardHub ---
const dashboardConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/dashboard")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

// ===== TURNSTILE EVENTS =====
turnstileConnection.on("TurnstileStatusChanged", function (turnstileId, status) {
    // Status dot yangilash
    const dot = document.getElementById(`status-dot-${turnstileId}`);
    if (dot) {
        dot.className = 'status-dot';
        if (status === 'Online') dot.classList.add('status-online');
        else if (status === 'Blocked') dot.classList.add('status-blocked');
        else dot.classList.add('status-offline');
    }

    // Karta yangilash
    const card = document.getElementById(`turnstile-${turnstileId}`);
    if (card) {
        showToast(`Turniket #${turnstileId} holati: ${status}`, status === 'Online' ? 'success' : status === 'Blocked' ? 'danger' : 'warning');
    }
});

turnstileConnection.on("EmergencyOpen", function () {
    showToast("🚨 FAVQULODDA: Barcha turniketlar ochildi!", 'danger');
    // Barcha dot-larni yashil qilish
    document.querySelectorAll('[id^="status-dot-"]').forEach(dot => {
        dot.className = 'status-dot status-online';
    });
});

turnstileConnection.on("PassageEvent", function (data) {
    // Dashboard activity feed yangilash
    const feed = document.getElementById('activityFeed');
    if (feed) {
        const type = data.result === 'Granted' ? 'good' : data.result === 'Denied' ? 'deny' : 'warn';
        const item = document.createElement('div');
        item.className = `activity-item activity-${type}`;
        item.innerHTML = `
            <div class="activity-dot"></div>
            <div class="activity-info">
                <span class="activity-name">${data.userName}</span>
                <span class="activity-action">${data.method} orqali ${data.result === 'Granted' ? "o'tdi" : 'rad etildi'}</span>
            </div>
            <span class="activity-time">${data.time}</span>
        `;
        feed.insertBefore(item, feed.firstChild);
        // Eski elementlarni o'chirish (max 15 ta)
        while (feed.children.length > 15) {
            feed.removeChild(feed.lastChild);
        }
    }
});

turnstileConnection.on("TurnstileLog", function (turnstileId, message, time) {
    console.log(`[Turnstile ${turnstileId}] ${message} @ ${time}`);
});

// ===== CAMERA EVENTS =====
cameraConnection.on("CameraStatusChanged", function (cameraId, status) {
    const card = document.querySelector(`.camera-card[data-id="${cameraId}"]`);
    if (card) {
        if (status === 'Offline') {
            card.classList.add('camera-offline');
        } else {
            card.classList.remove('camera-offline');
        }
        showToast(`Kamera #${cameraId} holati: ${status}`, status === 'Online' ? 'success' : 'warning');
    }
});

cameraConnection.on("FaceDetected", function (data) {
    if (data.isUnknown) {
        showToast(`⚠️ Noma'lum yuz aniqlandi! Kamera #${data.cameraId}`, 'warning');
    }
});

cameraConnection.on("MotionDetected", function (data) {
    console.log(`[Motion] Kamera #${data.cameraId}: ${data.location} @ ${data.time}`);
});

// ===== ALERT EVENTS =====
alertConnection.on("NewAlert", function (data) {
    const typeMap = { 'info': 'info', 'warning': 'warning', 'danger': 'danger', 'success': 'success' };
    showToast(`${data.title}: ${data.message}`, typeMap[data.type] || 'info');

    // Alert badge yangilash
    const badge = document.getElementById('alertBadge');
    if (badge) {
        const count = parseInt(badge.textContent) + 1;
        badge.textContent = count;
    }
});

alertConnection.on("BlockedAccessAttempt", function (data) {
    showToast(`🚫 Bloklangan kirish: ${data.userName} — ${data.turnstileName}`, 'danger');
});

// ===== DASHBOARD EVENTS =====
dashboardConnection.on("StatsUpdated", function (data) {
    const todayPass = document.getElementById('todayPassCount');
    const alertCount = document.getElementById('alertCount');
    if (todayPass) todayPass.textContent = data.todayPass;
    if (alertCount) alertCount.textContent = data.alerts;
});

dashboardConnection.on("NewActivity", function (data) {
    const feed = document.getElementById('activityFeed');
    if (feed) {
        const item = document.createElement('div');
        item.className = `activity-item activity-${data.type}`;
        item.innerHTML = `
            <div class="activity-dot"></div>
            <div class="activity-info">
                <span class="activity-name">${data.userName}</span>
                <span class="activity-action">${data.action}</span>
            </div>
            <span class="activity-time">${data.time}</span>
        `;
        feed.insertBefore(item, feed.firstChild);
        while (feed.children.length > 15) {
            feed.removeChild(feed.lastChild);
        }
    }
});

// ===== CONNECTION STATUS =====
function updateConnectionStatus(connected) {
    const statusEl = document.getElementById('signalr-status');
    if (statusEl) {
        const dot = statusEl.querySelector('.status-dot');
        const text = statusEl.querySelector('span:last-child');
        if (connected) {
            dot.className = 'status-dot status-online';
            text.textContent = 'Ulanish: Faol';
        } else {
            dot.className = 'status-dot status-offline';
            text.textContent = 'Ulanish: Uzilgan';
        }
    }
}

// ===== START ALL CONNECTIONS =====
async function startConnections() {
    const connections = [
        { conn: turnstileConnection, name: 'Turnstile' },
        { conn: cameraConnection, name: 'Camera' },
        { conn: alertConnection, name: 'Alert' },
        { conn: dashboardConnection, name: 'Dashboard' }
    ];

    for (const { conn, name } of connections) {
        try {
            await conn.start();
            console.log(`✓ ${name}Hub ulandi`);
        } catch (err) {
            console.error(`✗ ${name}Hub xatolik:`, err);
        }

        conn.onreconnecting(() => {
            console.log(`↻ ${name}Hub qayta ulanmoqda...`);
            updateConnectionStatus(false);
        });

        conn.onreconnected(() => {
            console.log(`✓ ${name}Hub qayta ulandi`);
            updateConnectionStatus(true);
        });

        conn.onclose(() => {
            console.log(`✗ ${name}Hub uzildi`);
            updateConnectionStatus(false);
        });
    }

    updateConnectionStatus(true);
}

// Sahifa yuklanganda barcha hublarni ulash
document.addEventListener('DOMContentLoaded', startConnections);
