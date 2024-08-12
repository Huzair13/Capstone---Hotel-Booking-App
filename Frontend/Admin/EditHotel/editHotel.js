document.addEventListener('DOMContentLoaded', function () {
    const token = localStorage.getItem('token');
    const urlParams = new URLSearchParams(window.location.search);
    const hotelId = urlParams.get('hotelId');


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

    if (hotelId) {
        // Fetch hotel details
        fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetHotelByID/${hotelId}`, {
            headers: {
                'Content-Type': 'application/json',
                Authorization: `Bearer ${token}`
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data) {
                    document.getElementById('hotelId').value = data.id;
                    document.getElementById('hotelName').value = data.name;
                    document.getElementById('hotelDescription').value = data.description;
                    document.getElementById('hotelAddress').value = data.address;
                    document.getElementById('hotelCity').value = data.city;
                    document.getElementById('hotelState').value = data.state;
                    document.getElementById('hotelType').value = data.type;
                }
            })
            .catch(error => console.error('Error fetching hotel details:', error));
    }

    document.getElementById('editHotelForm').addEventListener('submit', function (e) {
        e.preventDefault();
        document.getElementById('spinner').style.display = 'block'; 
        document.getElementById('overlay').style.display = 'block';
        const formData = new FormData(e.target);
        var sendData = {
            HotelId: document.getElementById('hotelId').value,
            Name: document.getElementById('hotelName').value,
            Address: document.getElementById('hotelAddress').value,
            City: document.getElementById('hotelCity').value,
            State: document.getElementById('hotelState').value,
            Type: document.getElementById('hotelType').value,
            Description: document.getElementById('hotelDescription').value
        }
        const data = Object.fromEntries(formData.entries());
        fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/UpdateHotel', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(sendData)
        })
            .then(response => {
                if (response.ok) {
                    alert('Hotel updated successfully!');
                    window.location.href = '/Admin/HotelManage/hotelManage.html';
                } else {
                    alert('Failed to update hotel.');
                }
                document.getElementById('spinner').style.display = 'none'; 
                document.getElementById('overlay').style.display = 'none';
            })
            .catch(error => {
                console.error('Error updating hotel:', error);
                alert('An error occurred while updating the hotel.');
                document.getElementById('spinner').style.display = 'none'; 
                document.getElementById('overlay').style.display = 'none';
            });
    });
});