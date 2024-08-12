const params = new URLSearchParams(window.location.search);
const hotelId = params.get('hotelId');
const checkInDate = params.get('checkInDate');
const checkOutDate = params.get('checkOutDate');
const numOfGuests = params.get('numOfGuests');
const numOfRooms = params.get('numOfRooms');
const totalAmount = params.get('totalAmount');
const userId = localStorage.getItem('userId');

const token = localStorage.getItem('token');

if (!hotelId || !checkInDate || !checkOutDate || !numOfGuests || !numOfRooms) {
    window.location.href = '/User/UserHome/userHome.html';
}

document.addEventListener('DOMContentLoaded', () => {

    window.onload = function () {
        history.replaceState(null, null, '/User/UserHome/userHome.html');
    }


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

    if (!token) {
        window.location.href = '/Login/Login.html';
    }

    if (token && isTokenExpired(token)) {
        localStorage.removeItem('token');
        window.location.href = '/Login/Login.html';
    }

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


    function showAlert(message, type) {
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

    async function sendEmail(data) {
        const qrCode = new QRCodeStyling({
            width: 300,
            height: 300,
            data: JSON.stringify(data),
            dotsOptions: {
                color: "#4267b2",
                type: "rounded"
            },
            backgroundOptions: {
                color: "#e9ebee",
            },
            imageOptions: {
                crossOrigin: "anonymous",
                margin: 20,
                imageSize: 0.4,
                image: "https://rghuzteststorage.blob.core.windows.net/huztestcontainer/1-copy.jpg" // URL to your logo image
            }
        });

        qrCode.append(document.getElementById("qr-code"));

        try {
            const blob = await qrCode.getRawData("image/png");
            const reader = new FileReader();

            return new Promise((resolve, reject) => {
                reader.onloadend = () => {
                    const qrCodeDataUrl = reader.result;
                    const emailData = {
                        toEmail: "ahamedhuzair13@gmail.com",
                        subject: "Booking Confirmation",
                        body: "Dear Guest,\n Thank you for booking with Book My Stay! 🎉\n Find you booking Detail QR Below",
                        imageUrl: qrCodeDataUrl.split(',')[1]
                    };

                    fetch('https://huzairhotelbookingapi.azure-api.net/Booking/api/sendEmail', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            
                        },
                        body: JSON.stringify(emailData)
                    })
                        .then(response => {
                            if (!response.ok) {
                                return response.text().then(text => {
                                    throw new Error(text);
                                });
                            }
                            return response.text();
                        })
                        .then(data => {
                            console.log('Email Sent:', data);
                            resolve(data);
                        })
                        .catch(error => {
                            console.error('Error sending email:', error);
                            reject(error);
                        });
                };

                reader.readAsDataURL(blob);
            });
        } catch (error) {
            console.error('Failed to generate QR code:', error);
        }
    }

    async function handleBookingResponse(response) {
        try {
            const data = await response.json();
            console.log(data);

            await sendEmail(data);

            document.getElementById('spinner').style.display = 'none';
            document.getElementById('overlay').style.display = 'none';
            alert('Booking successful!');
            window.location.href = `/User/BookingConfirmation/bookingConfirmation.html?bookingId=${data.bookingId}`;
        } catch (error) {
            console.error('Error:', error);
            showAlert("An Error Occurred At Booking", 'danger');
            showAlert(error, 'danger');
            document.getElementById('spinner').style.display = 'none';
            document.getElementById('overlay').style.display = 'none';
        }
    }

    document.getElementById('codButton').addEventListener('click', () => {
        document.getElementById('spinner').style.display = 'block';
        document.getElementById('overlay').style.display = 'block';
        showConfirmationModal(() => {
            fetch('https://huzairhotelbookingapi.azure-api.net/Booking/api/AddBooking', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`,
                    
                },
                body: JSON.stringify({
                    HotelId: parseInt(hotelId),
                    CheckInDate: checkInDate,
                    CheckOutDate: checkOutDate,
                    NumberOfGuests: parseInt(numOfGuests),
                    NumberOfRooms: parseInt(numOfRooms),
                    paymentMode: 0
                })
            })
                .then(response => {
                    if (!response.ok) {
                        return response.text().then(text => { throw new Error(text); });
                    }
                    return handleBookingResponse(response);
                })
                .catch(error => {
                    showAlert("An Error Occured At Booking"), 'danger';
                    showAlert(error, 'danger');
                    document.getElementById('spinner').style.display = 'none';
                    document.getElementById('overlay').style.display = 'none';
                    console.error('Error:', error)
                });
        });
    });

    document.getElementById('onlinePaymentButton').addEventListener('click', () => {


        document.getElementById('spinner').style.display = 'block';
        document.getElementById('overlay').style.display = 'block';
        showConfirmationModal(() => {
            alert('Online Payment not implemented yet. Proceeding to booking...');
            fetch('https://huzairhotelbookingapi.azure-api.net/Booking/api/AddBooking', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`,
                    
                },
                body: JSON.stringify({
                    HotelId: parseInt(hotelId),
                    CheckInDate: checkInDate,
                    CheckOutDate: checkOutDate,
                    NumberOfGuests: parseInt(numOfGuests),
                    NumberOfRooms: parseInt(numOfRooms),
                    paymentMode: 1
                })
            })
                .then(response => {
                    if (!response.ok) {
                        return response.text().then(text => { throw new Error(text); });
                    }
                    return handleBookingResponse(response);
                })
                .catch(error => {
                    console.error('Error:', error)
                    showAlert("An Error Occured At Booking", 'danger');
                    showAlert(error, 'danger');
                    document.getElementById('spinner').style.display = 'none';
                    document.getElementById('overlay').style.display = 'none';
                });
        });
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
            document.getElementById('spinner').style.display = 'block';
            document.getElementById('overlay').style.display = 'block';
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
                    document.getElementById('spinner').style.display = 'none';
                    document.getElementById('overlay').style.display = 'none';
                })
                .catch(error => {
                    console.error('Error submitting request:', error);
                    document.getElementById('spinner').style.display = 'none';
                    document.getElementById('overlay').style.display = 'none';
                });
        });
    }

    function showConfirmationModal(onConfirm) {
        const confirmationModal = new bootstrap.Modal(document.getElementById('confirmationModal'));
        document.getElementById('confirmButton').addEventListener('click', () => {
            confirmationModal.hide();
            onConfirm();
        }, { once: true });
        confirmationModal.show();
    }
});
