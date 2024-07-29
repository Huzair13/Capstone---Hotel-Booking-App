document.addEventListener('DOMContentLoaded', async () => {
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

    const hotelListDiv = document.getElementById('hotelList');
    const currentLocationDiv = document.getElementById('currentLocationDiv');

    function getBearerToken() {
        return localStorage.getItem('token');
    }

    async function fetchAllHotels() {
        try {
            const token = getBearerToken();
            const response = await fetch('https://localhost:7257/api/GetAllHotels', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            return await response.json();
        } catch (error) {
            console.error('Error fetching hotels:', error.message);
            alert('An error occurred while fetching hotels. Please try again later.');
            return [];
        }
    }

    async function fetchAllAmenities() {
        try {
            const response = await fetch('https://localhost:7257/api/GetAllAmenities', {
                headers: {
                    'Content-Type': 'application/json',
                    Authorization : `Bearer ${getBearerToken()}`
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            return await response.json();
        } catch (error) {
            console.error('Error fetching amenities:', error.message);
            return [];
        }
    }

    async function getCoordinates(address, state, city) {
        const url = `https://nominatim.openstreetmap.org/search?address=${encodeURIComponent(address)}&city=${encodeURIComponent(city)}&state=${encodeURIComponent(state)}&format=json&limit=1`;

        try {
            const response = await fetch(url);
            if (!response.ok) throw new Error('Network response was not ok');
            const data = await response.json();

            if (data.length > 0) {
                const { lat, lon } = data[0];
                return { lat: parseFloat(lat), lon: parseFloat(lon) };
            } else {
                throw new Error('Coordinates not found for the given state and city');
            }
        } catch (error) {
            console.error('Error getting coordinates:', error.message);
            return null;
        }
    }

    function calculateDistance(lat1, lon1, lat2, lon2) {
        const R = 6371; // Radius of the Earth in km
        const dLat = (lat2 - lat1) * (Math.PI / 180);
        const dLon = (lon2 - lon1) * (Math.PI / 180);
        const a =
            Math.sin(dLat / 2) * Math.sin(dLat / 2) +
            Math.cos(lat1 * (Math.PI / 180)) * Math.cos(lat2 * (Math.PI / 180)) *
            Math.sin(dLon / 2) * Math.sin(dLon / 2);
        const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        const distance = R * c;
        return distance.toFixed(2); // Distance in km
    }

    function getCurrentLocation(callback) {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(position => {
                callback(position.coords.latitude, position.coords.longitude);
            }, error => {
                console.error('Error getting current location:', error.message);
                currentLocationDiv.innerHTML = `<div class="alert alert-danger" role="alert">Error getting current location.</div>`;
            });
        } else {
            currentLocationDiv.innerHTML = `<div class="alert alert-danger" role="alert">Geolocation is not supported by this browser.</div>`;
        }
    }

    function displayHotels(hotels, amenitiesMap) {
        hotelListDiv.innerHTML = '';

        hotels.forEach(hotel => {
            const imagesHtml = hotel.hotelImages.map((imageUrl, index) => `
                <div class="carousel-item ${index === 0 ? 'active' : ''}">
                    <img src="${imageUrl}" class="d-block w-100" alt="Image of ${hotel.name}">
                </div>
            `).join('');

            const ratingHtml = Array.from({ length: 5 }, (_, i) => i < hotel.averageRatings ? '<span class="star">★</span>' : '<span class="star">☆</span>').join('');

            const amenitiesHtml = hotel.amenities.map(amenityId => {
                const amenityName = amenitiesMap[amenityId];
                const amenityIcon = getAmenityIcon(amenityName); // Function to get amenity icons
                return `<li>${amenityIcon} ${amenityName}</li>`;
            }).join('');

            const leastRentRoomHtml = hotel.leastRentRoom ? `
    <div class="card-text">
        <strong>Starting From :</strong> <span class="highlight">$${hotel.leastRentRoom.rent}</span>
    </div>
` : '';

            hotelListDiv.innerHTML += `
    <div class="col-lg-6 col-md-12 mb-4">
        <div class="card">
            <div class="carousel slide" data-bs-ride="carousel" id="carousel-${hotel.name.replace(/\s+/g, '')}">
                <div class="carousel-inner">
                    ${imagesHtml}
                </div>
                <button class="carousel-control-prev" type="button" data-bs-target="#carousel-${hotel.name.replace(/\s+/g, '')}" data-bs-slide="prev">
                    <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                    <span class="visually-hidden">Previous</span>
                </button>
                <button class="carousel-control-next" type="button" data-bs-target="#carousel-${hotel.name.replace(/\s+/g, '')}" data-bs-slide="next">
                    <span class="carousel-control-next-icon" aria-hidden="true"></span>
                    <span class="visually-hidden">Next</span>
                </button>
            </div>
            <div class="card-body">
                <h5 class="card-title text-center">${hotel.name}</h5>
                <div class="row">
                    <div class="col-md-6 text-center">
                        <p class="card-text"><i class="fas fa-map-marker-alt"></i> ${hotel.address}, ${hotel.city}, ${hotel.state}</p>
                        <p class="card-text"><i class="fas fa-ruler-combined"></i> Distance: ${hotel.distance} km</p>
                        <ul class="list-unstyled">
                            ${amenitiesHtml}
                        </ul>
                    </div>
                    <div class="col-md-6 text-center">
                        <p class="card-text">Rating: ${ratingHtml}</p>
                        ${leastRentRoomHtml}
                        <a href="/HotelDetails/hotelDetails.html?hotelId=${hotel.id}" class="book-button text-center">Book Now</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
`;
        });
    }

    function getAmenityIcon(amenity) {
        const icons = {
            'Free Wi-Fi': '<i class="fa fa-wifi"></i>',
            'Pool': '<i class="fa fa-swimming-pool"></i>',
            'Parking': '<i class="fa fa-parking"></i>',
            'Gym': '<i class="fa fa-dumbbell"></i>',
            'Restaurant': '<i class="fa fa-utensils"></i>',
        };
        return icons[amenity] || '<i class="fa fa-wifi"></i>';
    }

    async function fetchAllRooms() {
        try {
            const response = await fetch('https://localhost:7257/api/GetAllRooms', {
                headers: {
                    'Content-Type': 'application/json',
                    Authorization : `Bearer ${getBearerToken()}`
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            return await response.json();
        } catch (error) {
            console.error('Error fetching rooms:', error.message);
            return [];
        }
    }

    async function init() {
        const [hotels, amenities, rooms] = await Promise.all([
            fetchAllHotels(),
            fetchAllAmenities(),
            fetchAllRooms()
        ]);

        const amenitiesMap = amenities.reduce((map, amenity) => {
            map[amenity.id] = amenity.name;
            return map;
        }, {});

        // Find least rent room for each hotel
        const hotelRoomsMap = rooms.reduce((map, room) => {
            if (!map[room.hotelId] || room.rent < map[room.hotelId].rent) {
                map[room.hotelId] = room;
            }
            return map;
        }, {});

        getCurrentLocation(async (lat, lon) => {
            for (const hotel of hotels) {
                const coordinates = await getCoordinates(hotel.address, hotel.state, hotel.city);
                if (coordinates) {
                    const distance = calculateDistance(lat, lon, coordinates.lat, coordinates.lon);
                    hotel.distance = distance;
                }

                // Attach least rent room to the hotel
                hotel.leastRentRoom = hotelRoomsMap[hotel.id];
            }

            hotels.sort((a, b) => a.distance - b.distance);

            displayHotels(hotels, amenitiesMap);
        });
    }

    init();
});
