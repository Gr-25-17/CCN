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
        alert("Välj en fil eller klistra in en länk först.");
        return;
    }

    const formData = new FormData();
    if (file) formData.append('file', file);
    if (url) formData.append('externalUrl', url);

    // Visa spinner
    feedback.classList.replace('d-none', 'd-flex');
    statusText.innerText = "Laddar upp...";
    statusText.className = "text-primary fw-bold";

    try {
        const response = await fetch('/Writer/UploadImageAjax', { 
            method: 'POST', 
            body: formData 
        });

        if (!response.ok) throw new Error("Bilden kunde inte valideras eller laddas upp.");

        const data = await response.json();

        // 1. Uppdatera dold input så rätt data sparas i C# Model
        if (hiddenInput) hiddenInput.value = data.fileName;

        // 2. Visa preview av den nya bilden
        previewImg.src = data.temporaryUrl;
        previewContainer.style.display = 'block';

        // 3. TÖM INPUTS - Detta blockerar det "gamla" workflowet från att triggas
        if (fileInput) fileInput.value = '';
        if (urlInput) urlInput.value = '';

        statusText.innerText = data.isProcessing ? "Optimeras till WebP..." : "Klar!";
        statusText.className = "text-success fw-bold";

    } catch (err) {
        statusText.innerText = "Fel: " + err.message;
        statusText.className = "text-danger fw-bold";
    } finally {
        setTimeout(() => feedback.classList.replace('d-flex', 'd-none'), 5000);
    }
}