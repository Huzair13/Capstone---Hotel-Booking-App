document.addEventListener('DOMContentLoaded', async () => {
    const hotelsPerPage = 6;
    let currentPage = 1;
    let hotels = [];
    let amenitiesMap = {};
    let originalHotels = [];

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

    const token = localStorage.getItem('token');
    if (!token) {
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
        if (!isActive) {
            document.getElementById('deactivatedDiv').style.display = 'block';
            document.getElementById('mainDiv').style.display = 'none';
        }
    } catch (error) {
        console.error('Error fetching IsActive status:', error);
    }

    const cityDropdown = document.getElementById('cityDropdown');
    fetch('https://localhost:7257/api/GetHotelCity', {
        headers: {
            Authorization: `Bearer ${token}`
        }
    })
        .then(response => response.json())
        .then(cities => {
            cityDropdown.innerHTML = '<option value="">All</option>';
            cities.forEach(city => {
                const option = document.createElement('option');
                option.value = city;
                option.textContent = city;
                cityDropdown.appendChild(option);
            });
        })
        .catch(error => {
            console.error('Error fetching cities:', error);
        });

    const hotelListDiv = document.getElementById('hotelList');

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
                    Authorization: `Bearer ${getBearerToken()}`
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            return await response.json();
        } catch (error) {
            console.error('Error fetching amenities:', error.message);
            return [];
        }
    }

    async function fetchAllRooms() {
        try {
            const response = await fetch('https://localhost:7257/api/GetAllRooms', {
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${getBearerToken()}`
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            return await response.json();
        } catch (error) {
            console.error('Error fetching rooms:', error.message);
            return [];
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
                const amenityIcon = getAmenityIcon(amenityName);
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

    document.getElementById('search-button').addEventListener('click', () => {
        if (document.getElementById('cityDropdown').value) {
            const query = document.getElementById('hotel-text-input').value.trim().toLowerCase();

            if (query === '') {
                hotels = hotels.slice(0);
            } else {
                hotels = hotels.filter(hotel => hotel.name.toLowerCase().includes(query));
            }
        }
        else {
            const query = document.getElementById('hotel-text-input').value.trim().toLowerCase();

            if (query === '') {
                hotels = originalHotels.slice(0);
            } else {
                hotels = originalHotels.filter(hotel => hotel.name.toLowerCase().includes(query));
            }
        }
        currentPage = 1;
        displayHotels(hotels.slice(0, hotelsPerPage), amenitiesMap);
        renderPagination(hotels.length, hotelsPerPage, currentPage);

    });

    function renderPagination(totalHotels, hotelsPerPage, currentPage) {
        const paginationContainer = document.querySelector('.pagination');
        paginationContainer.innerHTML = '';
        const pageCount = Math.ceil(totalHotels / hotelsPerPage);

        if (currentPage > 1) {
            const prevItem = document.createElement('li');
            prevItem.className = 'page-item';
            prevItem.innerHTML = `<a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a>`;
            paginationContainer.appendChild(prevItem);
        }

        const startPage = Math.max(1, currentPage - 1);
        const endPage = Math.min(pageCount, currentPage + 1);

        for (let i = startPage; i <= endPage; i++) {
            const pageItem = document.createElement('li');
            pageItem.className = `page-item ${i === currentPage ? 'active' : ''}`;
            pageItem.innerHTML = `<a class="page-link" href="#" data-page="${i}">${i}</a>`;
            paginationContainer.appendChild(pageItem);
        }

        if (currentPage < pageCount) {
            const nextItem = document.createElement('li');
            nextItem.className = 'page-item';
            nextItem.innerHTML = `<a class="page-link" href="#" data-page="${currentPage + 1}">Next</a>`;
            paginationContainer.appendChild(nextItem);
        }

        document.querySelectorAll('.page-link').forEach(link => {
            link.addEventListener('click', function (e) {
                e.preventDefault();
                const page = parseInt(this.getAttribute('data-page'));
                if (page >= 1 && page <= Math.ceil(hotels.length / hotelsPerPage)) {
                    currentPage = page;
                    const start = (page - 1) * hotelsPerPage;
                    const end = start + hotelsPerPage;
                    displayHotels(hotels.slice(start, end), amenitiesMap);
                    renderPagination(hotels.length, hotelsPerPage, currentPage);
                }
            });
        });
    }

    document.getElementById('filter-button').addEventListener('click', async (event) => {
        event.preventDefault();
        await applyFilters();
    });

    async function sortaz() {
        hotels.sort((a, b) => 
            (a.leastRentRoom?.rent || 0) - (b.leastRentRoom?.rent || 0)
        );
        displayHotels(hotels.slice(0, hotelsPerPage), amenitiesMap);
        renderPagination(hotels.length, hotelsPerPage, currentPage);
    }

    async function sortza() {
        hotels.sort((a, b) => 
            (b.leastRentRoom?.rent || 0) - (a.leastRentRoom?.rent || 0)
        );
        displayHotels(hotels.slice(0, hotelsPerPage), amenitiesMap);
        renderPagination(hotels.length, hotelsPerPage, currentPage);
    }
    

    const sortAZBtn = document.getElementById('sort-az');
    const sortZABtn = document.getElementById('sort-za');

    sortAZBtn.addEventListener('click', () => {
        sortaz();
    });

    sortZABtn.addEventListener('click', () => {
        sortza();
    });

    async function applyFilters() {

        const selectedCity = document.getElementById('cityDropdown').value;
        console.log(hotels)

        if (selectedCity) {
            hotels = originalHotels.filter(hotel =>
                hotel.city.toLowerCase() === selectedCity.toLowerCase()
            );
        } else {
            hotels = originalHotels;
        }
        console.log(hotels)
        displayHotels(hotels.slice(0, hotelsPerPage), amenitiesMap);
        renderPagination(hotels.length, hotelsPerPage, currentPage);
    }


    async function init() {
        hotels = await fetchAllHotels();
        originalHotels = hotels;
        const amenities = await fetchAllAmenities();
        const rooms = await fetchAllRooms();

        amenitiesMap = amenities.reduce((map, amenity) => {
            map[amenity.id] = amenity.name;
            return map;
        }, {});

        const hotelRoomsMap = rooms.reduce((map, room) => {
            if (!map[room.hotelId] || room.rent < map[room.hotelId].rent) {
                map[room.hotelId] = room;
            }
            return map;
        }, {});

        hotels.forEach(hotel => {
            hotel.leastRentRoom = hotelRoomsMap[hotel.id];
        });

        console.log(hotels)

        displayHotels(hotels.slice(0, hotelsPerPage), amenitiesMap);
        renderPagination(hotels.length, hotelsPerPage, currentPage);
    }

    init();
});
