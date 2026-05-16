// ===== DASHBOARD CHART =====
function initDashboardChart(hourlyData) {
    const ctx = document.getElementById('hourlyChart');
    if (!ctx) return;

    const labels = Array.from({ length: 24 }, (_, i) => `${i.toString().padStart(2, '0')}:00`);

    new Chart(ctx.getContext('2d'), {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: "O'tishlar",
                data: hourlyData,
                backgroundColor: function (context) {
                    const chart = context.chart;
                    const { ctx: c, chartArea } = chart;
                    if (!chartArea) return '#3b82f680';
                    const gradient = c.createLinearGradient(0, chartArea.bottom, 0, chartArea.top);
                    gradient.addColorStop(0, '#3b82f620');
                    gradient.addColorStop(1, '#3b82f6a0');
                    return gradient;
                },
                borderColor: '#3b82f6',
                borderWidth: 1,
                borderRadius: 4,
                borderSkipped: false,
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#1e293b',
                    titleColor: '#f1f5f9',
                    bodyColor: '#94a3b8',
                    borderColor: '#334155',
                    borderWidth: 1,
                    cornerRadius: 8,
                    padding: 12,
                    callbacks: {
                        label: function (context) {
                            return `${context.parsed.y} ta o'tish`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        color: '#33415520',
                        drawBorder: false
                    },
                    ticks: {
                        color: '#64748b',
                        font: { size: 10 },
                        maxRotation: 0,
                        callback: function (value, index) {
                            // Faqat har 3-soatda label ko'rsatish
                            return index % 3 === 0 ? this.getLabelForValue(value) : '';
                        }
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: '#33415520',
                        drawBorder: false
                    },
                    ticks: {
                        color: '#64748b',
                        font: { size: 11 }
                    }
                }
            }
        }
    });
}
