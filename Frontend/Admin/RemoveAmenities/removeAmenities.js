document.addEventListener("DOMContentLoaded", function () {


    const urlParams = new URLSearchParams(window.location.search);
    const hotelId = urlParams.get('hotelId');

    if (!hotelId) {
        showAlert('Hotel ID is missing in the URL.', 'danger');
        return;
    }


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

    const amenitiesIcons = {
        "AC": "fas fa-snowflake",
        "TV": "fas fa-tv",
        "Fan": "fas fa-fan",
        "Geyser": "fas fa-tachometer-alt",
        "Wifi": "fas fa-wifi",
        "Play Station": "fas fa-gamepad",
        'Pool': "fas fa-swimming-pool",
        'Parking': "fas fa-parking",
        'Gym': "fas fa-dumbbell",
        'Restaurant': "fas fa-utensils"
    };

    function fetchHotelAndAmenities() {
        Promise.all([
            fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllAmenities`, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            }).then(response => response.json()),
            fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetHotelByID/${hotelId}`, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            }).then(response => response.json())
        ])
        .then(([allAmenities, hotelDetails]) => {
            const hotelAmenityIds = hotelDetails.amenities;
            const hotelAmenities = allAmenities.filter(amenity => hotelAmenityIds.includes(amenity.id));
            displayAmenities(hotelAmenities);
        })
        .catch(error => {
            showAlert('An error occurred while fetching amenities or hotel details.', 'danger');
        });
    }

    function displayAmenities(amenities) {
        const amenitiesList = document.getElementById('amenitiesList');
        amenitiesList.innerHTML = '';

        amenities.forEach(amenity => {
            const iconClass = amenitiesIcons[amenity.name] || 'fas fa-cogs'; // Default icon if not found
            const div = document.createElement('div');
            div.className = 'form-check';
            div.innerHTML = `
                <input class="form-check-input" type="checkbox" value="${amenity.id}" id="amenity${amenity.id}">
                <label class="form-check-label" for="amenity${amenity.id}">
                    <i class="${iconClass}"></i>
                    ${amenity.name}
                </label>
            `;
            amenitiesList.appendChild(div);
        });
    }

    document.getElementById('amenitiesForm').addEventListener('submit', function (event) {

        event.preventDefault();
        document.getElementById('spinner').style.display = 'block'; 
        document.getElementById('overlay').style.display = 'block';

        const selectedAmenities = [];
        document.querySelectorAll('#amenitiesList input:checked').forEach(checkbox => {
            selectedAmenities.push(checkbox.value);
        });

        if (selectedAmenities.length === 0) {
            showAlert('No amenities were selected.', 'danger');
            return;
        }

        const deleteAmenitiesFromHotelDTO = {
            HotelID: hotelId,
            AmenityIds: selectedAmenities
        };

        fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/DeleteAmenityFromHotel', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(deleteAmenitiesFromHotelDTO)
        })
        .then(response => {
            if (!response.ok) {
                return response.json().then(error => Promise.reject(error));
            }
            return response.json();
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
        })
        .then(response => {
            showAlert('Amenities removed successfully!', 'success');
            window.location.href=`/Admin/ManageOptions/manageOptions.html?hotelId=${hotelId}`
        })
        .catch(error => {
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
            const errorMessage = error && error.Message ? error.Message : 'An error occurred while removing amenities.';
            showAlert(errorMessage, 'danger');
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

    fetchHotelAndAmenities();
});