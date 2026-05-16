// ===== TURNSTILE CONTROL =====

async function openTurnstile(id) {
    const result = await fetch(`/Turnstile/Open/${id}`, { method: 'POST' });
    const data = await result.json();
    if (data.success) {
        showToast('Turniket ochildi ✓', 'success');
        updateTurnstileUI(id, 'Online');
    } else {
        showToast('Xatolik yuz berdi', 'danger');
    }
}

async function closeTurnstile(id) {
    if (!confirm('Turniketni yopmoqchimisiz?')) return;
    const result = await fetch(`/Turnstile/Close/${id}`, { method: 'POST' });
    const data = await result.json();
    if (data.success) {
        showToast('Turniket yopildi', 'warning');
        updateTurnstileUI(id, 'Offline');
    } else {
        showToast('Xatolik yuz berdi', 'danger');
    }
}

async function blockTurnstile(id) {
    if (!confirm('Turniketni bloklaysizmi? Bu barcha o\'tishlarni to\'xtatadi.')) return;
    const result = await fetch(`/Turnstile/Block/${id}`, { method: 'POST' });
    const data = await result.json();
    if (data.success) {
        showToast('Turniket bloklandi 🔒', 'danger');
        updateTurnstileUI(id, 'Blocked');
    } else {
        showToast('Xatolik yuz berdi', 'danger');
    }
}

async function unblockTurnstile(id) {
    const result = await fetch(`/Turnstile/Unblock/${id}`, { method: 'POST' });
    const data = await result.json();
    if (data.success) {
        showToast('Turniket blokdan chiqarildi 🔓', 'success');
        updateTurnstileUI(id, 'Online');
    } else {
        showToast('Xatolik yuz berdi', 'danger');
    }
}

async function emergencyOpenAll() {
    if (!confirm('🚨 FAVQULODDA OCHISH\n\nBarcha turniketlar ochiladi!\nTasdiqlaysizmi?')) return;
    const result = await fetch('/Turnstile/EmergencyOpenAll', { method: 'POST' });
    const data = await result.json();
    if (data.success) {
        showToast('🚨 Barcha turniketlar favqulodda ochildi!', 'danger');
        // Barcha turniket UI larni yangilash
        document.querySelectorAll('.turnstile-card').forEach(card => {
            const id = card.getAttribute('data-id');
            updateTurnstileUI(id, 'Online');
        });
    }
}

// ===== TURNSTILE UI UPDATE =====
function updateTurnstileUI(id, status) {
    const dot = document.getElementById(`status-dot-${id}`);
    if (dot) {
        dot.className = 'status-dot';
        if (status === 'Online') dot.classList.add('status-online');
        else if (status === 'Blocked') dot.classList.add('status-blocked');
        else dot.classList.add('status-offline');
    }

    // Tugmalarni yangilash (sahifani qayta yuklash orqali)
    const card = document.getElementById(`turnstile-${id}`);
    if (card) {
        const actionsDiv = card.querySelector('.turnstile-actions');
        if (actionsDiv) {
            if (status === 'Online') {
                actionsDiv.innerHTML = `
                    <button class="btn btn-sm btn-warning" onclick="closeTurnstile(${id})">⏸ Yopish</button>
                    <button class="btn btn-sm btn-danger" onclick="blockTurnstile(${id})">🔒 Bloklash</button>
                    <a href="/Turnstile/Details/${id}" class="btn btn-sm btn-secondary">Batafsil</a>
                `;
            } else if (status === 'Offline') {
                actionsDiv.innerHTML = `
                    <button class="btn btn-sm btn-success" onclick="openTurnstile(${id})">▶ Ochish</button>
                    <button class="btn btn-sm btn-danger" onclick="blockTurnstile(${id})">🔒 Bloklash</button>
                    <a href="/Turnstile/Details/${id}" class="btn btn-sm btn-secondary">Batafsil</a>
                `;
            } else if (status === 'Blocked') {
                actionsDiv.innerHTML = `
                    <button class="btn btn-sm btn-success" onclick="unblockTurnstile(${id})">🔓 Blokdan chiqarish</button>
                    <a href="/Turnstile/Details/${id}" class="btn btn-sm btn-secondary">Batafsil</a>
                `;
            }
        }
    }
}
