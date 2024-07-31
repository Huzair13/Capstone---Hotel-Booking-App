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

document.addEventListener('DOMContentLoaded', async()=>{
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

    try {
        const response = await fetch(`https://localhost:7032/IsActive/${localStorage.getItem('userID')}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json', 
            },
        });

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const isActive = await response.json();
        if(!isActive){
            document.getElementById('deactivatedDiv').style.display='block';
            document.getElementById('mainDiv').style.display='none';
            // window.location.href = "/Deactivated/deactivated.html"
        }
        console.log(isActive)
        
    } catch (error) {
        console.error('Error fetching IsActive status:', error);
    }
});

// Check if the dates are in the past
if (checkIn < currentDate || checkOut < currentDate) {
    document.getElementById('errorMessage').innerHTML = '<p>The check-in or check-out date cannot be in the past. Please select valid dates.</p>';
    document.getElementById('errorMessage').classList.remove('d-none');
    document.getElementById('proceedForBooking').classList.add('d-none');
} else {
    document.getElementById('bookingDetails').innerHTML = `
        <p>Hotel ID: ${hotelId}</p>
        <p>Check-in Date: ${checkInDate}</p>
        <p>Check-out Date: ${checkOutDate}</p>
        <p>Number of Guests: ${numOfGuests}</p>
        <p>Number of Rooms: ${numOfRooms}</p>
    `;

    if (available === 'true') {
        document.getElementById('availabilityStatus').innerHTML = '<p class="text-success">Available</p>';
        document.getElementById('proceedForBooking').classList.remove('d-none');
    } else {
        document.getElementById('availabilityStatus').innerHTML = '<p class="text-danger">Not Available</p>';
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 10000);
    }

    document.getElementById('proceedForBooking').addEventListener('click', () => {
        window.location.href = `/CostDetails/costDetails.html?hotelId=${hotelId}&checkInDate=${checkInDate}&checkOutDate=${checkOutDate}&numOfGuests=${numOfGuests}&numOfRooms=${numOfRooms}`;
    });
}