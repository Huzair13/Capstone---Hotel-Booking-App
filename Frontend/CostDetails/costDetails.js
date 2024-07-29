const token = localStorage.getItem('token');
const params = new URLSearchParams(window.location.search);
const hotelId = params.get('hotelId');
const checkInDate = params.get('checkInDate');
const checkOutDate = params.get('checkOutDate');
const numOfGuests = params.get('numOfGuests');
const numOfRooms = params.get('numOfRooms');

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
    
});

document.getElementById('bookingDetails').innerHTML = `
            <p>Hotel ID: ${hotelId}</p>
            <p>Check-in Date: ${checkInDate}</p>
            <p>Check-out Date: ${checkOutDate}</p>
            <p>Number of Guests: ${numOfGuests}</p>
            <p>Number of Rooms: ${numOfRooms}</p>
        `;

// Fetch total amount
fetch('https://localhost:7263/api/CalculateTotalAmount', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
        hotelId: parseInt(hotelId),
        checkInDate: checkInDate,
        checkOutDat: checkOutDate,
        numOfGuests: parseInt(numOfGuests),
        numOfRooms: parseInt(numOfRooms)
    })
})
    .then(response => response.json())
    .then(data => {
        console.log(data)
        document.getElementById('totalAmount').innerHTML = `<p>Total Amount: $${data.totalAmount}</p>`;
        document.getElementById('discount').innerHTML = `<p>Discount: $${data.discount}</p>`;
        document.getElementById('finalAmount').innerHTML = `<p>Final Amount: $${data.finalAmount}</p>`;
        document.getElementById('proceedToPay').classList.remove('d-none');
        document.getElementById('proceedToPay').addEventListener('click', () => {
            window.location.href = `/PaymentMode/paymentMode.html?hotelId=${hotelId}&checkInDate=${checkInDate}&checkOutDate=${checkOutDate}&numOfGuests=${numOfGuests}&numOfRooms=${numOfRooms}&totalAmount=${data}`;
        });
    })
    .catch(error => console.error('Error:', error));