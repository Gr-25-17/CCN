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

    setTimeout(() => {
        toastEl.remove();
    }, 5000);
}

document.addEventListener('DOMContentLoaded', function () {
    const offcanvasElement = document.getElementById('loginOffcanvas');
    const titleElement = document.getElementById('loginOffcanvasLabel');
    const bodyElement = document.getElementById('loginOffcanvasBody');

    if (!offcanvasElement || !titleElement || !bodyElement) {
        return;
    }

    const loginTemplate = document.getElementById('loginOffcanvasTemplate');
    const subscribeTemplate = document.getElementById('subscribeOffcanvasTemplate');

    offcanvasElement.addEventListener('show.bs.offcanvas', function (event) {
        const trigger = event.relatedTarget;
        const mode = trigger?.getAttribute('data-auth-mode') ?? 'login';

        switch (mode) {
            case 'register':
                loadLoginTemplate(); 
                titleElement.textContent = 'Registrera';
                clickRegisterLink(); 
                break;

            case 'subscribe':
                if (document.body.classList.contains('authenticated')) {
                    loadSubscribeTemplate(); 
                    titleElement.textContent = '⭐ Prenumerera';
                } else {
                    loadLoginTemplate(); 
                    titleElement.textContent = 'Logga in';
                }
                break;

            default:
                loadLoginTemplate(); 
                titleElement.textContent = 'Logga in';
                break;
        }
    });

    function loadLoginTemplate() {
        if (!loginTemplate) return;
        bodyElement.innerHTML = loginTemplate.innerHTML;
        wireDynamicLinks(); 
    }

    function loadSubscribeTemplate() {
        if (!subscribeTemplate) return;
        bodyElement.innerHTML = subscribeTemplate.innerHTML;

        const returnUrlInput = bodyElement.querySelector('input[name="ReturnUrl"]');
        if (returnUrlInput) {
            returnUrlInput.value = window.location.pathname + window.location.search + window.location.hash;
        }
    }

    function wireDynamicLinks() {
        bodyElement.querySelectorAll('[data-auth-mode]').forEach(element => {
            element.addEventListener('click', function (e) {
                e.preventDefault();
                const nextMode = this.getAttribute('data-auth-mode');

                if (nextMode === 'register') {
                    loadLoginTemplate(); 
                    titleElement.textContent = 'Registrera';
                    clickRegisterLink(); 
                } else if (nextMode === 'login') {
                    loadLoginTemplate(); 
                    titleElement.textContent = 'Logga in';
                } else if (nextMode === 'subscribe') {
                    loadSubscribeTemplate(); 
                    titleElement.textContent = '⭐ Prenumerera';
                }
            });
        });
    }

    function clickRegisterLink() {
        const registerLink = bodyElement.querySelector('#register-link');
        if (registerLink) {
            registerLink.click(); 
        }
    }
});
