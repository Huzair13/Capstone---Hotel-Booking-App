$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const hotelId = urlParams.get('hotelId');

    $('#imageUploadForm').on('submit', function (event) {
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
            url: `https://localhost:7257/api/AddImagesToHotel/${hotelId}`,
            headers :{
                Authorization : `Bearer ${localStorage.getItem('token')}`
            },
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                showAlert('Images uploaded successfully!', 'success');
                window.location.href = `/ManageOptions/manageOptions.html?hotelId=${hotelId}`
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
