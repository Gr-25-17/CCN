async function uploadImageAjax() {
    const fileInput = document.getElementById('imageFileInput');
    const urlInput = document.getElementById('imageUrlInput');
    const hiddenInput = document.getElementById('hiddenImageUrl');

    const feedback = document.getElementById('uploadFeedback');
    const statusText = document.getElementById('uploadStatusText');
    const previewContainer = document.getElementById('previewContainer');
    const previewImg = document.getElementById('imagePreview');

    const file = fileInput?.files[0];
    const url = urlInput?.value.trim();

    if (!file && !url) {
        alert('Välj en fil eller klistra in en länk först.');
        return;
    }

    const formData = new FormData();
    if (file) formData.append('file', file);
    if (url) formData.append('externalUrl', url);

    feedback.classList.replace('d-none', 'd-flex');
    statusText.innerText = 'Laddar upp...';
    statusText.className = 'text-primary fw-bold';

    try {
        const response = await fetch('/Writer/UploadImageAjax', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) throw new Error('Bilden kunde inte valideras eller laddas upp.');

        const data = await response.json();

        if (hiddenInput) hiddenInput.value = data.fileName;
        previewImg.src = data.temporaryUrl;
        previewContainer.style.display = 'block';

        if (fileInput) fileInput.value = '';
        if (urlInput) urlInput.value = '';

        statusText.innerText = data.isProcessing ? 'Optimeras till WebP...' : 'Klar!';
        statusText.className = 'text-success fw-bold';
    } catch (err) {
        statusText.innerText = 'Fel: ' + err.message;
        statusText.className = 'text-danger fw-bold';
    } finally {
        setTimeout(() => feedback.classList.replace('d-flex', 'd-none'), 5000);
    }
}

function showToast(message, type = 'info') {
    let toastContainer = document.getElementById('toastContainer');

    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        toastContainer.style.position = 'fixed';
        toastContainer.style.top = '20px';
        toastContainer.style.right = '20px';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    const toastEl = document.createElement('div');
    toastEl.className = `alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show`;
    toastEl.textContent = message;
    toastEl.innerHTML += '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>';

    toastContainer.appendChild(toastEl);
    setTimeout(() => toastEl.remove(), 5000);
}

document.addEventListener('DOMContentLoaded', () => {
    const logoutForm = document.querySelector('form[data-ajax-logout="true"]');
    const logoutButton = document.querySelector('[data-logout-button="true"]');

    if (logoutForm && logoutButton) {
        logoutButton.addEventListener('click', async () => {
            try {
                const response = await fetch(logoutForm.action, {
                    method: 'POST',
                    body: new FormData(logoutForm),
                    credentials: 'same-origin'
                });

                if (!response.ok) {
                    throw new Error('Logout misslyckades.');
                }

                const path = window.location.pathname;

                const isProtectedPage =
                    path.startsWith('/Admin') ||
                    path.startsWith('/Writer') ||
                    path.startsWith('/Identity/Account/Manage');

                if (isProtectedPage) {
                    window.location.href = '/';
                    return;
                }

                window.location.reload();
            } catch {
                window.location.href = '/';
            }
        });
    }

    const offcanvas = document.getElementById('loginOffcanvas');
    const title = document.getElementById('loginOffcanvasLabel');

    if (!offcanvas || !title) {
        return;
    }

    offcanvas.addEventListener('show.bs.offcanvas', event => {
        const trigger = event.relatedTarget;
        const mode = trigger?.getAttribute('data-auth-mode') ?? 'login';

        const loginView = document.getElementById('loginView');
        const registerView = document.getElementById('registerView');
        const subscribeView = document.getElementById('subscribeView');
        const forgotPasswordView = document.getElementById('forgotPasswordView');
        const resendConfirmationView = document.getElementById('resendConfirmationView');
        const isAuthenticated = document.body.classList.contains('authenticated');

        if (!loginView || !registerView || !subscribeView || !forgotPasswordView || !resendConfirmationView) {
            return;
        }

        loginView.classList.add('d-none');
        registerView.classList.add('d-none');
        subscribeView.classList.add('d-none');
        forgotPasswordView.classList.add('d-none');
        resendConfirmationView.classList.add('d-none');

        switch (mode) {
            case 'register':
                title.textContent = 'Registrera dig';
                registerView.classList.remove('d-none');
                break;

            case 'subscribe':
                if (isAuthenticated) {
                    title.textContent = '⭐ Prenumerera';
                    subscribeView.classList.remove('d-none');
                } else {
                    title.textContent = 'Logga in';
                    loginView.classList.remove('d-none');
                }
                break;

            default:
                title.textContent = 'Logga in';
                loginView.classList.remove('d-none');
                break;
        }
    });

    offcanvas.addEventListener('hidden.bs.offcanvas', () => {
        const loginView = document.getElementById('loginView');
        const registerView = document.getElementById('registerView');
        const subscribeView = document.getElementById('subscribeView');
        const forgotPasswordView = document.getElementById('forgotPasswordView');
        const resendConfirmationView = document.getElementById('resendConfirmationView');

        if (!loginView || !registerView || !subscribeView || !forgotPasswordView || !resendConfirmationView) {
            return;
        }

        title.textContent = 'Logga in';
        loginView.classList.remove('d-none');
        registerView.classList.add('d-none');
        subscribeView.classList.add('d-none');
        forgotPasswordView.classList.add('d-none');
        resendConfirmationView.classList.add('d-none');
    });
});
