// ===== TOAST AUTO-HIDE =====
document.addEventListener('DOMContentLoaded', function () {
    const toast = document.getElementById('toast-auto');
    if (toast) {
        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(-10px)';
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }
});

// ===== SIDEBAR TOGGLE (MOBILE) =====
function toggleSidebar() {
    const sidebar = document.querySelector('.sidebar');
    sidebar.classList.toggle('show');
    sidebar.classList.toggle('collapsed');
}

// ===== GLOBAL SEARCH =====
const globalSearch = document.getElementById('globalSearch');
if (globalSearch) {
    globalSearch.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            const query = this.value.trim();
            if (query.length > 0) {
                // Hozirgi sahifada qidirish (agar Student sahifasida bo'lsa)
                const currentPath = window.location.pathname.toLowerCase();
                if (currentPath.includes('student')) {
                    window.location.href = `/Student?search=${encodeURIComponent(query)}`;
                } else if (currentPath.includes('teacher')) {
                    window.location.href = `/Teacher?search=${encodeURIComponent(query)}`;
                } else if (currentPath.includes('accesslog')) {
                    window.location.href = `/AccessLog?search=${encodeURIComponent(query)}`;
                } else {
                    // Default: Student sahifasiga yo'naltirish
                    window.location.href = `/Student?search=${encodeURIComponent(query)}`;
                }
            }
        }
    });
}

// ===== SHOW TOAST (JS orqali) =====
function showToast(message, type = 'success') {
    const icons = { success: '✓', warning: '⚠', danger: '✕', info: 'ℹ' };
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
        <span class="toast-icon">${icons[type] || '✓'}</span>
        <span>${message}</span>
        <button class="toast-close" onclick="this.parentElement.remove()">×</button>
    `;
    const content = document.querySelector('.content');
    if (content) {
        content.insertBefore(toast, content.firstChild);
        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(-10px)';
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }
}

// ===== CONFIRM DELETE =====
function confirmDelete(message) {
    return confirm(message || "O'chirishni tasdiqlaysizmi?");
}

// ===== FETCH HELPER =====
async function postAction(url, data = {}) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        return await response.json();
    } catch (error) {
        console.error('Xatolik:', error);
        showToast('Server bilan bog\'lanishda xatolik', 'danger');
        return { success: false };
    }
}
