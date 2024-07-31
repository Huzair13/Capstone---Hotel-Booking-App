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
    

    const params = new URLSearchParams(window.location.search);
    const bookingId = params.get('bookingId');
    // const token = localStorage.getItem('token');

    // Fetch booking details using the bookingId
    fetch(`https://localhost:7263/api/GetBookingById/${bookingId}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    })
        .then(response => response.json())
        .then(data => {
            console.log(data)
            document.getElementById('bookingConfirmation').innerHTML = `
                <p>Booking ID: ${data.id}</p>
                <p>Hotel ID: ${data.hotelId}</p>
                <p>User ID: ${data.userId}</p>
                <p>Check-in Date: ${data.checkInDate}</p>
                <p>Check-out Date: ${data.checkOutDate}</p>
                <p>Number of Guests: ${data.numberOfGuests}</p>
                <p>Total Price: $${data.totalPrice}</p>
                <p>Allocated Rooms: ${data.roomNumbers.join(', ')}</p>
            `;
        })
        .catch(error => console.error('Error:', error));

});
