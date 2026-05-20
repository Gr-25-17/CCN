async function startMigration() {
    const btn = document.getElementById('btnMigrate');
    const feedback = document.getElementById('migrationFeedback');
    const statusText = document.getElementById('migrationStatus');
    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    btn.disabled = true;
    feedback.classList.replace('d-none', 'd-flex');
    statusText.innerText = 'Migrering pågår... vänligen vänta.';
    statusText.className = 'fw-bold text-primary';

    try {
        const response = await fetch('/Admin/RunImageMigration', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...(antiForgeryToken ? { 'RequestVerificationToken': antiForgeryToken } : {})
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
