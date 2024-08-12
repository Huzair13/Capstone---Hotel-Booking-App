document.addEventListener("DOMContentLoaded", function () {
    function isTokenExpired(token) {
        try {
            const decoded = jwt_decode(token);
            const currentTime = Date.now() / 1000;
            return decoded.exp < currentTime;
        } catch (error) {
            console.error("Error decoding token:", error);
            return true;
        }
    }

    const tokenTest = localStorage.getItem('token');
    if (!tokenTest) {
        window.location.href = '/Login/Login.html';
    }
    if (tokenTest && isTokenExpired(tokenTest)) {
        localStorage.removeItem('token');
        window.location.href = '/Login/Login.html';
    }

    if (localStorage.getItem('role') !== "Admin") {
        alert("Unauthorized");
        window.location.href = "/Login/Login.html";
        return;
    }

    // Retrieve hotel ID from URL
    const urlParams = new URLSearchParams(window.location.search);
    const hotelId = urlParams.get('hotelId');
    if (!hotelId) {
        showAlert('Hotel ID is missing in the URL.', 'danger');
        return;
    }
    document.getElementById('hotelId').value = hotelId;

    document.getElementById('addRoomForm').addEventListener('submit', function (event) {
        document.getElementById('spinner').style.display = 'block'; 
        document.getElementById('overlay').style.display = 'block';
        event.preventDefault();

        const roomDTO = {
            HotelId: parseInt(document.getElementById('hotelId').value),
            RoomNumber: parseInt(document.getElementById('roomNumber').value),
            RoomType: document.getElementById('roomType').value,
            RoomFloor: parseInt(document.getElementById('roomFloor').value),
            AllowedNumOfGuests: parseInt(document.getElementById('allowedNumOfGuests').value),
            Rent: parseFloat(document.getElementById('rent').value)
        };

        fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/AddRoomToHotel/${roomDTO.HotelId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(roomDTO)
        })
            .then(response => {
                if (!response.ok) {
                    return response.json().then(error => Promise.reject(error));
                }
                return response.json();
            })
            .then(response => {
                showAlert('Room added successfully!', 'success');
                window.location.href=`/Admin/ManageOptions/manageOptions.html?hotelId=${hotelId}`
                document.getElementById('spinner').style.display = 'none'; 
                document.getElementById('overlay').style.display = 'none';
            })
            .catch(error => {
                const errorMessage = error && error.Message ? error.Message : 'An error occurred while adding the room.';
                showAlert(errorMessage, 'danger');
                document.getElementById('spinner').style.display = 'none'; 
                document.getElementById('overlay').style.display = 'none';
            });
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
});
