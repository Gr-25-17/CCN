async function startMigration() {
    const btn = document.getElementById('btnMigrate');
    const feedback = document.getElementById('migrationFeedback');
    const statusText = document.getElementById('migrationStatus');
    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    if (!btn || !feedback || !statusText) {
        return;
    }

    btn.disabled = true;
    feedback.classList.replace('d-none', 'd-flex');
    statusText.innerText = 'Migrering pågår... vänligen vänta.';
    statusText.className = 'fw-bold text-primary';

    try {
        const response = await fetch('/Admin/RunImageMigration', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...(antiForgeryToken ? { RequestVerificationToken: antiForgeryToken } : {})
            }
        });

        if (!response.ok) {
            throw new Error('Ett fel uppstod på servern.');
        }

        statusText.innerText = await response.text();
        statusText.className = 'fw-bold text-success';
    } catch (error) {
        statusText.innerText = error?.message ?? 'Ett oväntat fel uppstod.';
        statusText.className = 'fw-bold text-danger';
    } finally {
        btn.disabled = false;
    }
}

async function loadAdminAnalytics() {
    const container = document.getElementById('statsContainer');
    const errorBox = document.getElementById('errorBox');
    if (!container || !errorBox) {
        return;
    }

    try {
        const res = await fetch('/api/admin/subscription-stats', { headers: { Accept: 'application/json' } });
        if (!res.ok) {
            throw new Error(`HTTP ${res.status}`);
        }

        const data = await res.json();
        const cards = [
            { label: 'Totala användare', value: data.totalRegisteredUsers ?? 0, className: 'text-dark' },
            { label: 'Aktiva prenumeranter', value: data.activeSubscribers ?? 0, className: 'text-success' },
            { label: 'Inaktiva prenumeranter', value: data.inactiveSubscribers ?? 0, className: 'text-danger' },
            { label: 'Nya denna månad', value: data.newSubscribersThisMonth ?? 0, className: 'text-primary' },
            { label: 'Nya förra månaden', value: data.newSubscribersLastMonth ?? 0, className: 'text-secondary' },
            { label: 'Återkommande', value: data.returningSubscribers ?? 0, className: 'text-info' }
        ];

        container.innerHTML = cards.map(card => `
            <div class="col-md-4">
                <div class="card shadow-sm border-0 h-100">
                    <div class="card-body">
                        <h6 class="text-muted">${card.label}</h6>
                        <h2 class="${card.className}">${card.value}</h2>
                    </div>
                </div>
            </div>`).join('');
    } catch (err) {
        errorBox.classList.remove('d-none');
        errorBox.textContent = `Kunde inte ladda analytics: ${err.message}`;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const analyticsTab = document.getElementById('analytics-tab');
    if (!analyticsTab) {
        return;
    }

    analyticsTab.addEventListener('shown.bs.tab', loadAdminAnalytics, { once: true });
});
