$(document).ready(function () {

    
    function isTokenExpired(token) {
        try {
            const decoded = jwt_decode(token);
            const currentTime = Date.now() / 1000;
            return decoded.exp < currentTime;
        } catch (error) {
            console.error("Error decoding token:", error);
            return true;
        }
    }

    const tokenTest = localStorage.getItem('token');
    if (!tokenTest) {
        window.location.href = '/Login/Login.html';
    }
    if (tokenTest && isTokenExpired(tokenTest)) {
        localStorage.removeItem('token');
        window.location.href = '/Login/Login.html';
    }

    if (localStorage.getItem('role') !== "Admin") {
        alert("Unauthorized");
        window.location.href = "/Login/Login.html";
        return;
    }

    const loadingSpinner = document.getElementById('loadingSpinner');
    const alertPlaceholder = document.getElementById('alertPlaceholder');

    const urlParams = new URLSearchParams(window.location.search);
    const hotelId = urlParams.get('hotelId');

    $('#imageUploadForm').on('submit', function (event) {
        loadingSpinner.classList.remove('d-none');
        event.preventDefault();
        const files = $('#images')[0].files;

        if (!files.length) {
            showAlert('No files were uploaded.', 'danger');
            return;
        }

        const formData = new FormData();
        formData.append('hotelId', hotelId);
        for (let i = 0; i < files.length; i++) {
            formData.append('files', files[i]);
        }

        $.ajax({
            url: `https://huzairhotelbookingapi.azure-api.net/Hotel/api/AddImagesToHotel/${hotelId}`,
            headers :{
                Authorization : `Bearer ${localStorage.getItem('token')}`
            },
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                showAlert('Images uploaded successfully!', 'success');
                loadingSpinner.classList.add('d-none');
                window.location.href = `/Admin/ManageOptions/manageOptions.html?hotelId=${hotelId}`
            },
            error: function (xhr) {
                const errorMessage = xhr.responseJSON && xhr.responseJSON.Message ? xhr.responseJSON.Message : 'An error occurred while uploading images.';
                showAlert(errorMessage, 'danger');
            }
        });
    });

    function showAlert(message, type) {
        const alertPlaceholder = $('#alertPlaceholder');
        const alert = `<div class="alert alert-${type} alert-dismissible fade show" role="alert">
                            ${message}
                            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                       </div>`;
        alertPlaceholder.html(alert);
    }
});
