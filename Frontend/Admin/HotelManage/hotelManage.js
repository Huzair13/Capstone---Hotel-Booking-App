document.addEventListener('DOMContentLoaded', async () => {
    document.getElementById('spinner').style.display = 'block'; 
    document.getElementById('overlay').style.display = 'block';

    const hotelsPerPage = 6;
    let currentPage = 1;
    let hotels = [];
    let amenitiesMap = {}; 

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

    const logoutButton = document.getElementById('logoutbtn');
    const logoutModal = new bootstrap.Modal(document.getElementById('logoutModal'));
    const confirmLogoutButton = document.getElementById('confirmLogoutButton');
    logoutButton.addEventListener('click', function (event) {
        event.preventDefault();
        logoutModal.show();
    });

    confirmLogoutButton.addEventListener('click', function (event) {
        event.preventDefault();
        localStorage.removeItem('token');
        localStorage.removeItem('userID');
        localStorage.removeItem('role');

        window.location.href = '/Login/Login.html';
    });

    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/Login/Login.html';
    }
    if (token && isTokenExpired(token)) {
        localStorage.removeItem('token');
        window.location.href = '/Login/Login.html';
    }

    if(localStorage.getItem('role')!=="Admin"){
        alert("Unathorized");
        window.location.href="/Login/Login.html"
    }

    const hotelListDiv = document.getElementById('hotelList');

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

    async function fetchAllAmenities() {
        try {
            const response = await fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllAmenities', {
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
            const response = await fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllRooms', {
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
                return `<div>${amenityIcon}</div>`;
            }).join('');

            const leastRentRoomHtml = hotel.leastRentRoom ? `
                <div class="card-text">
                    <strong>Starting From :</strong> <span class="highlight">$${hotel.leastRentRoom.rent}</span>
                </div>
            ` : '';

            function capitalizeFirstLetter(text) {
                return text.charAt(0).toUpperCase() + text.slice(1).toLowerCase();
            }
            
            function formatAddress(address, city, state) {
                return `${capitalizeFirstLetter(address)}, ${capitalizeFirstLetter(city)}, ${capitalizeFirstLetter(state)}`;
            }
            
            const formattedAddress = formatAddress(hotel.address, hotel.city, hotel.state);
            
            const adddressTextHtml = `
                <p class="card-text"><i class="fas fa-map-marker-alt"></i> ${formattedAddress}</p>
            `;

            hotelListDiv.innerHTML += `
                <div class="col-lg-6 col-md-12 mb-4">
                    <div class="card">
                        <div class="carousel slide CardSlider" data-bs-ride="carousel" id="carousel-${hotel.name.replace(/\s+/g, '')}">
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
                            <h5 class="card-title text-center hotel-name">${hotel.name}</h5>
                            <div class="row">
                                <div class="col-md-6 text-center">
                                    ${adddressTextHtml}
                                    <p><a href="tel:${hotel.contactNumber}" class="phone-link"><i class="fas fa-phone"></i> ${hotel.contactNumber}</a></p>
                                    <div class="hotel-amenities mt-3">
                                    <div id="hotelAmenitiesValue"
                                        class="amenities-icons d-flex justify-content-center">
                                        ${amenitiesHtml}
                                    </div>
                                    </div>
                                </div>
                                <div class="col-md-6 text-center">
                                    <p class="card-text">Rating: ${ratingHtml}</p>
                                    ${leastRentRoomHtml}
                                    <a href="/Admin/ManageOptions/manageOptions.html?hotelId=${hotel.id}" class="book-button text-center">Manage Hotel</a>
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
            "AC": '<i class="fas fa-snowflake "></i>',
            'Wifi': '<i class="fas fa-wifi"></i>',
            'Fan': '<i class="fas fa-fan"></i>',
            "TV": '<i class="fas fa-tv"></i>',
            "Play Station": '<i class="fas fa-gamepad"></i>',
            "Geyser": '<i class="fas fa-tachometer-alt"></i>',
            'Pool': '<i class="fas fa-swimming-pool"></i>',
            'Parking': '<i class="fas fa-parking"></i>',
            'Gym': '<i class="fas fa-dumbbell"></i>',
            'Restaurant': '<i class="fas fa-utensils"></i>',
        };
        return icons[amenity] || '<i class="fa fa-wifi"></i>';
    }

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

    async function init() {
        hotels = await fetchAllHotels();
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

        displayHotels(hotels.slice(0, hotelsPerPage), amenitiesMap);
        renderPagination(hotels.length, hotelsPerPage, currentPage);
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
    }

    await init();
    document.getElementById('spinner').style.display = 'none'; 
    document.getElementById('overlay').style.display = 'none';
});
