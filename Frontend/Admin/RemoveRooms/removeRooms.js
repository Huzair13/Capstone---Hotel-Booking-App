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
    
    const urlParams = new URLSearchParams(window.location.search);
    const hotelId = urlParams.get('hotelId');
    if (!hotelId) {
        showAlert('Hotel ID is missing in the URL.', 'danger');
        return;
    }

    fetchRooms(hotelId);

    function fetchRooms(hotelId) {
        fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetRoomsByHotelID/${hotelId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        })
            .then(response => {
                console.log('Response Status:', response.status); // Log response status
                return response.json();
            })
            .then(data => {
                console.log('Fetched Rooms Data:', data); // Log the response data
                if (!data || data.length === 0) {
                    showAlert('No rooms data available.', 'warning');
                    return;
                }
                displayRooms(data);
            })
            .catch(error => {
                console.error('Error fetching room details:', error); // Log detailed error
                showAlert(error.message || 'An error occurred while fetching room details.', 'danger');
            });
    }

    function displayRooms(rooms) {
const container = document.getElementById('roomsTableContainer');

if (!Array.isArray(rooms) || rooms.length === 0) {
container.innerHTML = '<p>No rooms found for this hotel.</p>';
return;
}

const tableHtml = `
<table class="table table-striped">
    <thead>
        <tr>
            <th class=text-center>Room Number</th>
            <th class=text-center>Room Floor</th>
            <th class=text-center>Allowed Number of Guests</th>
            <th>Rent</th>
            <th>Action</th>
        </tr>
    </thead>
    <tbody>
        ${rooms.map(room => {
            // Ensure Rent is properly formatted
            const rent = (room.rent !== undefined && !isNaN(room.rent)) ? room.rent.toFixed(2) : 'N/A';
            
            // Display each room's data
            return `
                <tr>
                    <td class=text-center>${room.roomNumber || 'N/A'}</td>
                    <td class=text-center>${room.roomFloor || 'N/A'}</td>
                    <td class=text-center>${room.allowedNumOfGuests || 'N/A'}</td>
                    <td>${rent}</td>
                    <td>
                        <button class="btn btn-danger btn-sm" onclick="removeRoom(${room.roomNumber})">Remove</button>
                    </td>
                </tr>
            `;
        }).join('')}
    </tbody>
</table>
`;
container.innerHTML = tableHtml;
}

    window.removeRoom = function (roomNumber) {
        fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/RemoveRoomFromHotel`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify({
                hotelId : parseInt(hotelId),
                roomNumber : parseInt(roomNumber)
            })
        })
            .then(response => {
                console.log('Remove Room Response Status:', response.status); // Log response status
                return response.json();
            })
            .then(() => {
                showAlert('Room removed successfully!', 'success');
                fetchRooms(hotelId); // Refresh the room list
                // window.location.href=`/Admin/ManageOptions/manageOptions.html?hotelId=${hotelId}`
            })
            .catch(error => {
                console.error('Error removing room:', error); // Log detailed error
                showAlert(error.message || 'An error occurred while removing the room.', 'danger');
            });
    };

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
