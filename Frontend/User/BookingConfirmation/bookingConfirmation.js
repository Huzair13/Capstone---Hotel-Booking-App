document.addEventListener('DOMContentLoaded', () => {

    document.getElementById('spinner').style.display = 'block'; 
    document.getElementById('overlay').style.display = 'block';

    function isTokenExpired(token) {
        try {
            const decoded = jwt_decode(token);
            console.log(decoded)
            const currentTime = Date.now() / 1000;
            return decoded.exp < currentTime;
        } catch (error) {
            console.error("Error decoding token:", error);
            return true;
        }
    }
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/Login/Login.html';
    }
    if (token && isTokenExpired(token)) {
        localStorage.removeItem('token');
        window.location.href = '/Login/Login.html';
    }


    document.getElementById('backToHome').addEventListener('click', function () {
        window.location.href = "/User/UserHome/userHome.html";
    });

    fetch(`https://huzairhotelbookingapi.azure-api.net/IsActive/${localStorage.getItem('userID')}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            
        },
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(isActive => {
            if (!isActive) {
                document.getElementById('deactivatedDiv').style.display = 'block';
                document.body.style.background = "#748D92";
                document.getElementById('mainDiv').style.display = 'none';
            }
            console.log(isActive);
        })
        .catch(error => {
            console.error('Error fetching IsActive status:', error);
        });


    document.getElementById('request-activation').addEventListener('click', function () {
        fetch('https://huzairhotelbookingapi.azure-api.net/GetAllRequests', {
            headers: {
                Authorization: `Bearer ${token}`,
                'Content-Type': 'application/json',
                
            }
        })
            .then(response => response.json())
            .then(requests => {
                const userId = localStorage.getItem('userID');
                const isRequested = requests.some(request =>
                    request.userId === userId && request.status === 'Requested'
                );

                if (isRequested) {
                    alert('You have already requested.');
                } else {
                    showRequestForm();
                }
            })
            .catch(error => {
                console.error('Error fetching requests:', error);
            });
    });


    function showRequestForm() {
        const modalHtml = `
            <div class="modal fade" id="requestModal" tabindex="-1" aria-labelledby="requestModalLabel" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="requestModalLabel">Submit Request</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <form id="request-form">
                                <div class="mb-3">
                                    <label for="reason" class="form-label">Reason:</label>
                                    <input type="text" class="form-control" id="reason" name="reason" required>
                                </div>
                                <button type="submit" class="btn btn-primary">Submit Request</button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        `;
        document.body.insertAdjacentHTML('beforeend', modalHtml);

        const requestModal = new bootstrap.Modal(document.getElementById('requestModal'));
        requestModal.show();

        document.getElementById('request-form').addEventListener('submit', function (event) {
            event.preventDefault();
            const reason = document.getElementById('reason').value;

            fetch('https://huzairhotelbookingapi.azure-api.net/RequestForActivation', {
                method: 'POST',
                headers: {
                    Authorization: `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    
                },
                body: JSON.stringify({
                    reason: reason
                })
            })
                .then(response => response.json())
                .then(data => {
                    alert('Request submitted successfully!');
                    requestModal.hide();
                    document.getElementById('requestModal').remove();
                })
                .catch(error => {
                    console.error('Error submitting request:', error);
                });
        });
    }

    const params = new URLSearchParams(window.location.search);
    const bookingId = params.get('bookingId');
    if(!bookingId){
        alert("Booking ID is Missing");
        window.location.href='/User/UserHome/userHome.html'
    }
    // const token = localStorage.getItem('token');

    // Fetch booking details using the bookingId
    fetch(`https://huzairhotelbookingapi.azure-api.net/Booking/api/GetBookingByID/${bookingId}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            
        }
    })
        .then(response => response.json())
        .then(data => {
            console.log(data)
            document.getElementById('bookingConfirmation').innerHTML = `
                <div class="row mb-3">
                    <div class="col-4 large-text">Booking ID:</div>
                    <div class="col-8 large-text">${data.id}</div>
                </div>
                <div class="row mb-3">
                    <div class="col-4 large-text">Hotel ID:</div>
                    <div class="col-8 large-text">${data.hotelId}</div>
                </div>
                <div class="row mb-3">
                    <div class="col-4 large-text">User ID:</div>
                    <div class="col-8 large-text">${data.userId}</div>
                </div>
                <div class="row mb-3">
                    <div class="col-4 large-text">Check-in Date:</div>
                    <div class="col-8 large-text">${data.checkInDate}</div>
                </div>
                <div class="row mb-3">
                    <div class="col-4 large-text">Check-out Date:</div>
                    <div class="col-8 large-text">${data.checkOutDate}</div>
                </div>
                <div class="row mb-3">
                    <div class="col-4 large-text">Number of Guests:</div>
                    <div class="col-8 large-text">${data.numberOfGuests}</div>
                </div>
                <div class="row mb-3 row-highlight">
                    <div class="col-4 large-text">Final Amount :</div>
                    <div class="col-8 large-text text-dark" style="font-weight:900">$${data.finalAmount}</div>
                </div>
                <div class="row row-highlight">
                    <div class="col-4 large-text">Allocated Rooms:</div>
                    <div class="col-8 large-text text-dark" style="font-weight:900">${data.roomNumbers.join(', ')}</div>
                </div>
            `;

            // Generate QR code
            const qrCodeData = JSON.stringify(data);
            const qrCodeContainer = document.getElementById('qrCode');
            qrCodeContainer.innerHTML = '';
            $(qrCodeContainer).qrcode({
                text: qrCodeData,
                width: 128,
                height: 128
            });

            // Style the save button
            const saveButton = document.getElementById('saveQrCodeBtn');

            // Save QR code
            saveButton.addEventListener('click', () => {
                const canvas = qrCodeContainer.querySelector('canvas');
                if (canvas) {
                    const link = document.createElement('a');
                    link.href = canvas.toDataURL('image/png');
                    link.download = 'booking-qr-code.png';
                    link.click();
                }
            });

            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
        })
        .catch(error => {
            console.error('Error:', error)
            showAlert('Error','danger')
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
        });


});

function showAlert(message, type) {
    document.getElementById('alertPlaceholder').style.display='block'
    const alertPlaceholder = document.getElementById('alertPlaceholder');
    const alert = document.createElement('div');
    alert.className = `alert alert-${type} alert-dismissible fade show`;
    alert.role = 'alert';
    alert.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    alertPlaceholder.innerHTML = '';
    alertPlaceholder.appendChild(alert);
}

document.getElementById('backToHome').addEventListener('click', function () {
    window.location.href = "/User/UserHome/userHome.html"
});