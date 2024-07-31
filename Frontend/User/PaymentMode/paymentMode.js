const params = new URLSearchParams(window.location.search);
const hotelId = params.get('hotelId');
const checkInDate = params.get('checkInDate');
const checkOutDate = params.get('checkOutDate');
const numOfGuests = params.get('numOfGuests');
const numOfRooms = params.get('numOfRooms');
const totalAmount = params.get('totalAmount');
const userId = localStorage.getItem('userId');

const token = localStorage.getItem('token');

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

window.onload = function() {
    // Replace the current history entry with the home page URL
    history.replaceState(null, null, '/Home/home.html'); // Update '/home' with your actual home page URL
}

document.getElementById('codButton').addEventListener('click', () => {
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
        })
    })
        .then(response => response.json())
        .then(data => {
            alert('Booking successful!');
            window.location.href = `/BookingConfirmation/bookingConfirmation.html?bookingId=${data.bookingId}`;
        })
        .catch(error => console.error('Error:', error));
});

document.getElementById('onlinePaymentButton').addEventListener('click', () => {
    alert('Online Payment not implemented yet. Proceeding to booking...');
    document.getElementById('codButton').click(); 
});