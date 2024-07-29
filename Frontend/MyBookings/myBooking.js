const token = localStorage.getItem('token');
const userId = getUserIdFromToken(token);

document.addEventListener('DOMContentLoaded',()=>{
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
    if(!token){
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
            // window.location.href = "/Deactivated/deactivated.html"
        }
        console.log(isActive);
    })
    .catch(error => {
        console.error('Error fetching IsActive status:', error);
    });
    
})

// Fetch bookings for the user
fetch(`https://localhost:7263/api/GetBookingByUser`, {
    method: 'GET',
    headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    }
})
.then(response => {
    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
})
.then(data => {
    const bookingsContainer = document.getElementById('bookingsContainer');
    if (Array.isArray(data) && data.length > 0) {
        data.forEach(booking => {
            // Create a booking card
            const bookingCard = document.createElement('div');
            bookingCard.className = 'booking-card';
            const statusClass = booking.isCancelled ? 'status-cancelled' : 'status-active';
            const cancelButton = booking.isCancelled ? '' : `<button class="btn btn-danger cancel-btn" data-booking-id="${booking.bookingId}">Cancel Booking</button>`;
            
            bookingCard.innerHTML = `
                <h5>Booking ID: ${booking.bookingId}</h5>
                <p><strong>Hotel ID:</strong> ${booking.hotelId}</p>
                <p><strong>Check-in Date:</strong> ${booking.checkInDate}</p>
                <p><strong>Check-out Date:</strong> ${booking.checkOutDate}</p>
                <p><strong>Number of Guests:</strong> ${booking.numberOfGuests}</p>
                <p><strong>Total Price:</strong> $${booking.totalPrice}</p>
                <p><strong>Discount:</strong> $${booking.discount}</p>
                <p><strong>Final Amount:</strong> $${booking.finalAmount}</p>
                <p><strong>Allocated Rooms:</strong> ${booking.roomNumbers ? booking.roomNumbers.join(', ') : 'Not available'}</p>
                <p><strong>Status:</strong> <span class="${statusClass}">${booking.isCancelled ? 'Cancelled' : 'Active'}</span></p>
                ${cancelButton}
            `;
            bookingsContainer.appendChild(bookingCard);
        });
    } else {
        bookingsContainer.innerHTML = '<p>No bookings found for this user.</p>';
    }

    // Add event listeners for cancel buttons
    document.querySelectorAll('.cancel-btn').forEach(button => {
        button.addEventListener('click', (event) => {
            const bookingId = event.target.getAttribute('data-booking-id');
            handleCancelBooking(bookingId);
        });
    });
})
.catch(error => console.error('Error:', error));

// Function to decode JWT and extract userId if needed
function getUserIdFromToken(token) {
    if (!token) return null;
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload).name; // Assuming userId is stored in 'name' claim
}

// Function to handle booking cancellation
function handleCancelBooking(bookingId) {
    fetch(`https://localhost:7263/api/GetUserCancellationCount`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(cancellationCount => {
        if (cancellationCount > 2) {
            if (confirm('If you cancel more than 2 times, your account will be temporarily deactivated. Do you want to proceed?')) {
                if (confirm('Are you sure you want to cancel this booking?')) {
                    cancelBooking(bookingId);
                    localStorage.removeItem('token');
                    window.reload();
                }
            }
        } else {
            if (confirm('Are you sure you want to cancel this booking?')) {
                cancelBooking(bookingId);
            }
        }
    })
    .catch(error => console.error('Error:', error));
}

// Function to cancel booking
function cancelBooking(bookingId) {
    fetch(`https://localhost:7276/api/CancelBooking/${bookingId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        alert('Booking cancelled successfully.');
        window.location.reload(); 
    })
    .catch(error => console.error('Error:', error));
}