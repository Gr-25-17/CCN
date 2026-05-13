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
    const toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        const container = document.createElement('div');
        container.id = 'toastContainer';
        container.style.position = 'fixed';
        container.style.top = '20px';
        container.style.right = '20px';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }

    const toastEl = document.createElement('div');
    toastEl.className = `alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show`;
    toastEl.textContent = message;
    toastEl.innerHTML += '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>';

    document.getElementById('toastContainer').appendChild(toastEl);

    setTimeout(() => toastEl.remove(), 5000);
}

document.addEventListener('DOMContentLoaded', () => {
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
        const isAuthenticated = document.body.classList.contains('authenticated');

        if (!loginView || !registerView || !subscribeView) {
            return;
        }

        loginView.classList.add('d-none');
        registerView.classList.add('d-none');
        subscribeView.classList.add('d-none');

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

        if (!loginView || !registerView || !subscribeView) {
            return;
        }

        title.textContent = 'Logga in';
        loginView.classList.remove('d-none');
        registerView.classList.add('d-none');
        subscribeView.classList.add('d-none');
    });
});
