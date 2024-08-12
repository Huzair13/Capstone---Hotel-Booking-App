document.addEventListener('DOMContentLoaded', async () => {
    document.getElementById('spinner').style.display = 'block'; 
    document.getElementById('overlay').style.display = 'block';

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

    function getQueryParams() {
        const params = new URLSearchParams(window.location.search);
        return {
            checkInDate: params.get('checkInDate'),
            checkOutDate: params.get('checkOutDate')
        };
    }

    const queryParams = getQueryParams();


    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/Login/Login.html';
    }
    if (token && isTokenExpired(token)) {
        localStorage.removeItem('token');
        window.location.href = '/Login/Login.html';
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


    function showAlert(message, type) {
        document.getElementById('alertPlaceholder').style.display='block'
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

    try {
        const response = await fetch(`https://huzairhotelbookingapi.azure-api.net/IsActive/${localStorage.getItem('userID')}`, {
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
            document.body.style.background = "#748D92";
            document.getElementById('mainDiv').style.display = 'none';
            document.getElementById('footer').display.style ='none';
        }
    } catch (error) {
        console.error('Error fetching IsActive status:', error);
    }

    const cityDropdown = document.getElementById('cityDropdown');
    fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetHotelCity', {
        headers: {
            Authorization: `Bearer ${token}`,
            
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

    async function fetchAllHotels(queryParams) {
        try {
            const token = getBearerToken();
            const response = await fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllAvailableHotelsByDate?checkInDate=${queryParams.checkInDate}&checkOutDate=${queryParams.checkOutDate}`, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            return await response.json();
        } catch (error) {
            console.error('Error fetching hotels:', error.message);
            showAlert('An error occurred while fetching hotels. Please try again later.','danger');
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
            return [];
        }
    }

    
    document.getElementById('request-activation').addEventListener('click', function() {
        fetch('https://huzairhotelbookingapi.azure-api.net/GetAllRequests', {
            headers: {
                Authorization: `Bearer ${token}`,
                'Content-Type': 'application/json',
                
            }
        })
        .then(response => response.json())
        .then(requests => {
            const userId = localStorage.getItem('userID');
            const isRequested = requests.some(request => 
                request.userId === userId && request.status === 'Requested'
            );
    
            if (isRequested) {
                alert('You have already requested.');
            } else {
                showRequestForm();
            }
        })
        .catch(error => {
            console.error('Error fetching requests:', error);
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
        });
    });


    function showRequestForm() {
        const modalHtml = `
            <div class="modal fade" id="requestModal" tabindex="-1" aria-labelledby="requestModalLabel" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="requestModalLabel">Submit Request</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <form id="request-form">
                                <div class="mb-3">
                                    <label for="reason" class="form-label">Reason:</label>
                                    <input type="text" class="form-control" id="reason" name="reason" required>
                                </div>
                                <button type="submit" class="btn btn-primary">Submit Request</button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        `;
        document.body.insertAdjacentHTML('beforeend', modalHtml);
    
        const requestModal = new bootstrap.Modal(document.getElementById('requestModal'));
        requestModal.show();
    
        document.getElementById('request-form').addEventListener('submit', function(event) {
            document.getElementById('spinner').style.display = 'block'; 
            document.getElementById('overlay').style.display = 'block';
            event.preventDefault();
            const reason = document.getElementById('reason').value;
            
            fetch('https://huzairhotelbookingapi.azure-api.net/RequestForActivation', {
                method: 'POST',
                headers: {
                    Authorization: `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    
                },
                body: JSON.stringify({
                    reason: reason
                })
            })
            .then(response => response.json())
            .then(data => {
                alert('Request submitted successfully!');
                requestModal.hide();
                document.getElementById('requestModal').remove();
                document.getElementById('spinner').style.display = 'none'; 
                document.getElementById('overlay').style.display = 'none';
            })
            .catch(error => {
                console.error('Error submitting request:', error);
                document.getElementById('spinner').style.display = 'none'; 
                document.getElementById('overlay').style.display = 'none';
            });
        });
    }

    async function fetchAllAmenities() {
        try {
            const response = await fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllAmenities', {
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${getBearerToken()}`,
                    
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            return await response.json();
        } catch (error) {
            console.error('Error fetching amenities:', error.message);
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
            shoqAlert("Error Fetching amenities",error)
            return [];
        }
    }

    async function fetchAllRooms() {
        try {
            const response = await fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllRooms', {
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${getBearerToken()}`,
                    
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            return await response.json();
        } catch (error) {
            console.error('Error fetching rooms:', error.message);
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
            showAlert('Error Fetching Rooms','danger')
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
                        <h5 class="card-title hotel-name">${hotel.name}</h5>
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
                                <p class="card-text">${ratingHtml}</p>
                                ${leastRentRoomHtml}
                            <a href="/User/HotelDetails/hotelDetails.html?hotelId=${hotel.id}" class="book-button text-center">Book Now</a>
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
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
    }


    async function init() {
        var queryParams = getQueryParams();
        hotels = await fetchAllHotels(queryParams);
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
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
    }

    await init();
    document.getElementById('spinner').style.display = 'none'; 
    document.getElementById('overlay').style.display = 'none';
});

