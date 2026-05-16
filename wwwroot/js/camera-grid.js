// ===== CAMERA GRID TOGGLE =====
function setGrid(columns) {
    const grid = document.getElementById('cameraGrid');
    if (!grid) return;

    grid.style.gridTemplateColumns = `repeat(${columns}, 1fr)`;

    // Active button toggle
    document.querySelectorAll('.grid-toggle .btn-sm').forEach(btn => {
        btn.classList.remove('active');
    });
    event.target.classList.add('active');

    // LocalStorage saqlash
    try {
        localStorage.setItem('cameraGridColumns', columns);
    } catch (e) { }
}

// Sahifa yuklanganda saqlangan grid ni tiklash
document.addEventListener('DOMContentLoaded', function () {
    try {
        const saved = localStorage.getItem('cameraGridColumns');
        if (saved) {
            const grid = document.getElementById('cameraGrid');
            if (grid) {
                grid.style.gridTemplateColumns = `repeat(${saved}, 1fr)`;
                // Active button
                document.querySelectorAll('.grid-toggle .btn-sm').forEach(btn => {
                    btn.classList.remove('active');
                    if (btn.textContent.trim().startsWith(saved)) {
                        btn.classList.add('active');
                    }
                });
            }
        }
    } catch (e) { }
});
