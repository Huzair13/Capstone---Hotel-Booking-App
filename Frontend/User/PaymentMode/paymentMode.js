const params = new URLSearchParams(window.location.search);
const hotelId = params.get('hotelId');
const checkInDate = params.get('checkInDate');
const checkOutDate = params.get('checkOutDate');
const numOfGuests = params.get('numOfGuests');
const numOfRooms = params.get('numOfRooms');
const totalAmount = params.get('totalAmount');
const userId = localStorage.getItem('userId');

const token = localStorage.getItem('token');

if(!hotelId || !checkInDate || !checkOutDate || !numOfGuests || !numOfRooms){
    window.location.href='/User/UserHome/userHome.html';
}

document.addEventListener('DOMContentLoaded', () => {
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

    fetch(`https://localhost:7032/IsActive/${localStorage.getItem('userID')}`, {
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
            document.getElementById('mainDiv').style.display = 'none';
        }
        console.log(isActive);
    })
    .catch(error => {
        console.error('Error fetching IsActive status:', error);
    });

    window.onload = function() {
        history.replaceState(null, null, '/User/Home/Home.html'); 
    }

    document.getElementById('codButton').addEventListener('click', () => {
        showConfirmationModal(() => {
            fetch('https://localhost:7263/api/AddBooking', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    HotelId: parseInt(hotelId),
                    CheckInDate: checkInDate,
                    CheckOutDate: checkOutDate,
                    NumberOfGuests: parseInt(numOfGuests),
                    NumberOfRooms: parseInt(numOfRooms),
                    paymentMode : 0
                })
            })
            .then(response => response.json())
            .then(data => {
                console.log(data)
                alert('Booking successful!');
                window.location.href = `/User/BookingConfirmation/bookingConfirmation.html?bookingId=${data.bookingId}`;
            })
            .catch(error => console.error('Error:', error));
        });
    });

    document.getElementById('onlinePaymentButton').addEventListener('click', () => {
        showConfirmationModal(() => {
            alert('Online Payment not implemented yet. Proceeding to booking...');
            fetch('https://localhost:7263/api/AddBooking', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    HotelId: parseInt(hotelId),
                    CheckInDate: checkInDate,
                    CheckOutDate: checkOutDate,
                    NumberOfGuests: parseInt(numOfGuests),
                    NumberOfRooms: parseInt(numOfRooms),
                    paymentMode : 1
                })
            })
            .then(response => response.json())
            .then(data => {
                console.log(data)
                alert('Booking successful!');
                window.location.href = `/User/BookingConfirmation/bookingConfirmation.html?bookingId=${data.bookingId}`;
            })
            .catch(error => console.error('Error:', error));
        });
    });

    
    document.getElementById('request-activation').addEventListener('click', function() {
        fetch('https://localhost:7032/GetAllRequests', {
            headers: {
                Authorization: `Bearer ${token}`,
                'Content-Type': 'application/json'
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
    
        document.getElementById('request-form').addEventListener('submit', function(event) {
            event.preventDefault();
            const reason = document.getElementById('reason').value;
            
            fetch('https://localhost:7032/RequestForActivation', {
                method: 'POST',
                headers: {
                    Authorization: `Bearer ${token}`,
                    'Content-Type': 'application/json'
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

    function showConfirmationModal(onConfirm) {
        const confirmationModal = new bootstrap.Modal(document.getElementById('confirmationModal'));
        document.getElementById('confirmButton').addEventListener('click', () => {
            confirmationModal.hide();
            onConfirm();
        }, { once: true });
        confirmationModal.show();
    }

    function requestActivation() {
        fetch('/YourEndpoint/RequestActivation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ userId })
        })
        .then(response => response.json())
        .then(data => alert('Reactivation request sent!'))
        .catch(error => console.error('Error requesting reactivation:', error));
    }
});
