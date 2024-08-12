document.addEventListener('DOMContentLoaded', () => {
    const hotelListDiv = document.getElementById('hotelList');
    const currentLocationDiv = document.getElementById('currentLocationDiv');

    function getBearerToken() {
        return localStorage.getItem('token');
    }

    async function fetchAllHotels() {
        try {
            const token = getBearerToken();
            const response = await fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllHotels', {
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

    async function getCoordinates(address,state, city) {
        const url = `https://nominatim.openstreetmap.org/search?address=${encodeURIComponent(address)}&city=${encodeURIComponent(city)}&state=${encodeURIComponent(state)}&format=json&limit=1`;
        // const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(address + ', ' + city + ', ' + state)}&format=json&limit=1`;

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

    function displayHotels(hotels) {
        hotelListDiv.innerHTML = '';

        hotels.forEach(hotel => {
            const imagesHtml = hotel.hotelImages.map(imageUrl => `
                <div class="carousel-item ${hotel.hotelImages.indexOf(imageUrl) === 0 ? 'active' : ''}">
                    <img src="${imageUrl}" class="d-block w-100" alt="Image of ${hotel.name}">
                </div>
            `).join('');

            hotelListDiv.innerHTML += `
                <div class="col-md-4 mb-4">
                    <div class="card">
                        <div id="carousel-${hotel.name.replace(/\s+/g, '')}" class="carousel slide">
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
                            <h5 class="card-title">${hotel.name}</h5>
                            <p class="card-text">${hotel.description}</p>
                            <p class="card-text"><strong>Address:</strong> ${hotel.address}</p>
                            <p class="card-text"><strong>State:</strong> ${hotel.state}</p>
                            <p class="card-text"><strong>City:</strong> ${hotel.city}</p>
                            <p class="card-text"><strong>Distance:</strong> ${hotel.distance} km</p>
                        </div>
                    </div>
                </div>
            `;
        });

        // Initialize carousels
        document.querySelectorAll('.carousel').forEach(carousel => {
            new bootstrap.Carousel(carousel);
        });
    }

    async function displayNearbyHotels() {
        getCurrentLocation(async (lat, lon) => {
            const allHotels = await fetchAllHotels();

            const hotelsWithDistance = await Promise.all(
                allHotels.map(async hotel => {
                    const hotelCoordinates = await getCoordinates(hotel.address,hotel.state, hotel.city);
                    if (hotelCoordinates) {
                        const distance = calculateDistance(lat, lon, hotelCoordinates.lat, hotelCoordinates.lon);
                        return { ...hotel, distance };
                    } else {
                        return null;
                    }
                })
            );

            const filteredHotels = hotelsWithDistance.filter(h => h !== null);
            const sortedHotels = filteredHotels.sort((a, b) => a.distance - b.distance);

            console.log('Sorted Hotels:', sortedHotels); // Debugging line
            displayHotels(sortedHotels);
        });
    }

    displayNearbyHotels();
});
