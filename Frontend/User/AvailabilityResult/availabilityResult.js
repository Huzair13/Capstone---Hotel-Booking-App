const params = new URLSearchParams(window.location.search);
const hotelId = params.get('hotelId');
const checkInDate = params.get('checkInDate');
const checkOutDate = params.get('checkOutDate');
const numOfGuests = params.get('numOfGuests');
const numOfRooms = params.get('numOfRooms');
const available = params.get('isAvailable');

const currentDate = new Date();
const checkIn = new Date(checkInDate);
const checkOut = new Date(checkOutDate);

document.addEventListener('DOMContentLoaded', async () => {
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

    try {
        const response = await fetch(`https://huzairhotelbookingapi.azure-api.net/IsActive/${localStorage.getItem('userID')}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                
            },
        });

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const isActive = await response.json();
        if (!isActive) {
            document.getElementById('deactivatedDiv').style.display = 'block';
            document.body.style.background = "#748D92";
            document.getElementById('mainDiv').style.display = 'none';
        }
        console.log(isActive)

    } catch (error) {
        console.error('Error fetching IsActive status:', error);
    }


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
                    showAlert("Request Submitted",'success')
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

// Check if the dates are in the past
if (checkIn < currentDate || checkOut < currentDate) {
    document.getElementById('errorMessage').innerHTML = '<p>The check-in or check-out date cannot be in the past. Please select valid dates.</p>';
    document.getElementById('errorMessage').classList.remove('d-none');
    document.getElementById('proceedForBooking').classList.add('d-none');
} else {
    document.getElementById('bookingDetails').innerHTML = `
                <div id="bookingDetails" class="booking-details">
                    <div class="row">
                        <div class="col-12 col-md-6 label-column label-border">
                            <p><strong>Hotel ID:</strong></p>
                        </div>
                        <div class="col-12 col-md-6 data-column text-design" style="font-family: 'Playfair Display', serif;">
                            <p>${hotelId}</p>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-12 col-md-6 label-column label-border">
                            <p><strong>Check-in Date:</strong></p>
                        </div>
                        <div class="col-12 col-md-6 data-column text-design" style="font-family: 'Playfair Display', serif;">
                            <p>${checkInDate}</p>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-12 col-md-6 label-column label-border">
                            <p><strong>Check-out Date:</strong></p>
                        </div>
                        <div class="col-12 col-md-6 data-column text-design" style="font-family: 'Playfair Display', serif;">
                            <p>${checkOutDate}</p>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-12 col-md-6 label-column label-border">
                            <p><strong>Number of Guests:</strong></p>
                        </div>
                        <div class="col-12 col-md-6 data-column text-design" style="font-family: 'Playfair Display', serif;">
                            <p>${numOfGuests}</p>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-12 col-md-6 label-column label-border">
                            <p><strong>Number of Rooms:</strong></p>
                        </div>
                        <div class="col-12 col-md-6 data-column text-design">
                            <p>${numOfRooms}</p>
                        </div>
                    </div>
                </div>

            `;

    const availabilityStatus = document.getElementById('availabilityStatus');
    const lottieContainer = document.createElement('div');
    lottieContainer.classList.add('lottie-container');

    const lottiePlayer = document.createElement('lottie-player');
    lottiePlayer.setAttribute('speed', '1');
    lottiePlayer.setAttribute('loop', 'true');
    lottiePlayer.setAttribute('autoplay', 'true');
    lottiePlayer.setAttribute('direction', '1');
    lottiePlayer.setAttribute('mode', 'normal');
    lottiePlayer.style.maxWidth = '100%';
    lottiePlayer.style.height = 'auto';

    if (available === 'true') {
        availabilityStatus.innerHTML = '<p class="text-success" style="color:white!important;">Available</p>';
        document.getElementById('proceedForBooking').classList.remove('d-none');
        lottiePlayer.setAttribute('src', 'https://lottie.host/335ccac2-5d26-4b9f-973f-b38bcdb7a6b2/L2I6uMG71Y.json');
    } else {
        let countdown = 10;
        availabilityStatus.innerHTML = `
        <p class="text-danger">
            Not Available.<br/>
            You will be redirected in <span id="countdown" class="countdown">${countdown}</span> seconds.
        </p>`;
        lottiePlayer.setAttribute('src', 'https://lottie.host/546edab9-94e0-48e3-b97c-af62e88c15ff/siiCzlcjQM.json');

        // Update the countdown every second
        const countdownInterval = setInterval(() => {
            countdown -= 1;
            document.getElementById('countdown').textContent = countdown;
            if (countdown === 0) {
                clearInterval(countdownInterval);
                window.location.href = `/User/HotelDetails/hotelDetails.html?hotelId=${hotelId}`;
            }
        }, 1000);
    }

    // Append Lottie Player to the lottieContainer
    lottieContainer.appendChild(lottiePlayer);

    // Append lottieContainer to the availabilityStatus container
    availabilityStatus.appendChild(lottieContainer);


    document.getElementById('proceedForBooking').addEventListener('click', () => {
        window.location.href = `/User/CostDetails/costDetails.html?hotelId=${hotelId}&checkInDate=${checkInDate}&checkOutDate=${checkOutDate}&numOfGuests=${numOfGuests}&numOfRooms=${numOfRooms}`;
    });
}