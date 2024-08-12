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

    function fetchHotelAndAmenities() {
        Promise.all([
            fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllAmenities`, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`                }
            }).then(response => response.json()),
            fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetHotelByID/${hotelId}`, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            }).then(response => response.json())
        ])
            .then(([allAmenities, hotelDetails]) => {
                const hotelAmenityIds = hotelDetails.amenities;
                const availableAmenities = allAmenities.filter(amenity => !hotelAmenityIds.includes(amenity.id));
                displayAmenities(availableAmenities);
            })
            .catch(error => {
                showAlert('An error occurred while fetching amenities or hotel details.', 'danger');
            });
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

    function displayAmenities(amenities) {
        const amenitiesList = document.getElementById('amenitiesList');
        amenitiesList.innerHTML = '';

        amenities.forEach(amenity => {
            const iconClass = amenitiesIcons[amenity.name] || 'fas fa-cogs'; // Default icon if not found
            const div = document.createElement('div');
            div.className = 'form-check mb-3 d-flex align-items-center';
            div.innerHTML = `
                <input class="form-check-input" type="checkbox" value="${amenity.id}" id="amenity${amenity.id}">
                <label class="form-check-label ms-3 d-flex align-items-center" for="amenity${amenity.id}">
                    <i class="${iconClass} me-2"></i>
                    ${amenity.name}
                </label>
            `;
            amenitiesList.appendChild(div);
        });
    }


    document.getElementById('amenitiesForm').addEventListener('submit', function (event) {
        document.getElementById('spinner').style.display = 'block'; 
        document.getElementById('overlay').style.display = 'block';
        event.preventDefault();

        const selectedAmenities = [];
        document.querySelectorAll('#amenitiesList input:checked').forEach(checkbox => {
            selectedAmenities.push(checkbox.value);
        });

        if (selectedAmenities.length === 0) {
            showAlert('No amenities were selected.', 'danger');
            return;
        }

        const addAmenitiesToHotelDTO = {
            hotelId: hotelId,
            amenityIds: selectedAmenities
        };

        fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/AddAmenitiesToHotel', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(addAmenitiesToHotelDTO)
        })
            .then(response => {
                if (!response.ok) {
                    return response.json().then(error => Promise.reject(error));
                }
                return response.json();
            })
            .then(response => {
                showAlert('Amenities added successfully!', 'success');
                document.getElementById('spinner').style.display = 'none'; 
                document.getElementById('overlay').style.display = 'none';
                window.location.href=`/Admin/ManageOptions/manageOptions.html?hotelId=${hotelId}`;
            })
            .catch(error => {
                document.getElementById('spinner').style.display = 'none'; 
                document.getElementById('overlay').style.display = 'none';
                const errorMessage = error && error.Message ? error.Message : 'An error occurred while adding amenities.';
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