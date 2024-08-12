document.addEventListener('DOMContentLoaded', () => {
    const loadHotelsBtn = document.getElementById('loadHotelsBtn');
    const hotelList = document.getElementById('hotelList');
    const bookingForm = document.getElementById('bookingForm');
    const bookingResult = document.getElementById('bookingResult');
    const searchForm = document.getElementById('searchForm');
    const searchResults = document.getElementById('searchResults');

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

    function getBearerToken() {
        return localStorage.getItem('token'); 
    }

    async function fetchAllHotels() {
        try {
            const token = getBearerToken();
            const response = await fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllHotels', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            return await response.json();
        } catch (error) {
            console.error('Error fetching hotels:', error.message);
            alert('An error occurred while fetching hotels. Please try again later.');
        }
    }

    loadHotelsBtn.addEventListener('click', async () => {
        try {
            hotelList.innerHTML = '<div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div>'; // Show loading spinner
            const allHotels = await fetchAllHotels();
            hotelList.innerHTML = allHotels.map(hotel => `
                <div class="card mb-3">
                    <div id="carousel-${hotel.id}" class="carousel slide">
                        <div class="carousel-inner">
                            ${hotel.hotelImages.map((url, index) => `
                                <div class="carousel-item${index === 0 ? ' active' : ''}">
                                    <img src="${url}" class="d-block w-100" alt="${hotel.name}">
                                </div>
                            `).join('')}
                        </div>
                        <button class="carousel-control-prev" type="button" data-bs-target="#carousel-${hotel.id}" data-bs-slide="prev">
                            <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                            <span class="visually-hidden">Previous</span>
                        </button>
                        <button class="carousel-control-next" type="button" data-bs-target="#carousel-${hotel.id}" data-bs-slide="next">
                            <span class="carousel-control-next-icon" aria-hidden="true"></span>
                            <span class="visually-hidden">Next</span>
                        </button>
                    </div>
                    <div class="card-body">
                        <h5 class="card-title">${hotel.name}</h5>
                        <p class="card-text">${hotel.description}</p>
                        <p class="card-text"><strong>Address:</strong> ${hotel.address}</p>
                    </div>
                </div>
            `).join('');
            document.querySelectorAll('.carousel').forEach(carousel => {
                new bootstrap.Carousel(carousel);
            });
        } catch (error) {
            hotelList.innerHTML = `<div class="alert alert-danger" role="alert">Error: ${error.message}</div>`;
        }
    });

    searchForm.addEventListener('submit', async (event) => {
        event.preventDefault();

        const checkInDate = document.getElementById('searchCheckInDate').value;
        const checkOutDate = document.getElementById('searchCheckOutDate').value;
        const numberOfGuests = parseInt(document.getElementById('numberOfGuests').value, 10);

        if (isNaN(numberOfGuests) || numberOfGuests <= 0) {
            searchResults.innerHTML = `<div class="alert alert-danger" role="alert">Please enter a valid number of guests.</div>`;
            return;
        }

        try {
            searchResults.innerHTML = '<div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div>'; // Show loading spinner
            const token = getBearerToken();
            const response = await fetch(`
                https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAvailableHotelsRooms?checkInDate=${checkInDate}&checkOutDate=${checkOutDate}&numberOfGuests=${numberOfGuests}`, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    
                }
            });
            if (!response.ok) throw new Error('Network response was not ok');
            const availableRooms = await response.json();

            const allHotels = await fetchAllHotels();

            searchResults.innerHTML = availableRooms.length > 0 ? availableRooms.map(room => {
                const hotel = allHotels.find(hotel => hotel.id === room.hotelId);
                return hotel ? `
                    <div class="card mb-3">
                        <div id="carousel-${hotel.id}-room-${room.id}" class="carousel slide">
                            <div class="carousel-inner">
                                ${hotel.hotelImages.map((url, index) => `
                                    <div class="carousel-item${index === 0 ? ' active' : ''}">
                                        <img src="${url}" class="d-block w-100" alt="${hotel.name}">
                                    </div>
                                `).join('')}
                            </div>
                            <button class="carousel-control-prev" type="button" data-bs-target="#carousel-${hotel.id}-room-${room.id}" data-bs-slide="prev">
                                <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                                <span class="visually-hidden">Previous</span>
                            </button>
                            <button class="carousel-control-next" type="button" data-bs-target="#carousel-${hotel.id}-room-${room.id}" data-bs-slide="next">
                                <span class="carousel-control-next-icon" aria-hidden="true"></span>
                                <span class="visually-hidden">Next</span>
                            </button>
                        </div>
                        <div class="card-body">
                            <h5 class="card-title">${hotel.name}</h5>
                            <p class="card-text">${hotel.description}</p>
                            <p class="card-text"><strong>Address:</strong> ${hotel.address}</p>
                            <p class="card-text"><strong>Room Number:</strong> ${room.roomNumber || 'N/A'}</p>
                            <p class="card-text"><strong>Room Type:</strong> ${room.roomType || 'N/A'}</p>
                            <p class="card-text"><strong>Price:</strong> $${room.price}</p>
                        </div>
                    </div>
                ` : '';
            }).join('') : '<div class="alert alert-info" role="alert">No available rooms found for the selected dates and number of guests.</div>';
        } catch (error) {
            searchResults.innerHTML = `<div class="alert alert-danger" role="alert">Error: ${error.message}</div>`;
        }
    });

    bookingForm.addEventListener('submit', async (event) => {
        event.preventDefault();

        const hotelId = parseInt(document.getElementById('hotelId').value, 10);
        const checkInDate = document.getElementById('checkInDate').value;
        const checkOutDate = document.getElementById('checkOutDate').value;
        const numberOfGuests = parseInt(document.getElementById('bookingNumberOfGuests').value, 10);
        const numberOfRooms = parseInt(document.getElementById('numberOfRooms').value, 10);

        if (isNaN(hotelId) || hotelId <= 0 || isNaN(numberOfGuests) || numberOfGuests <= 0 || isNaN(numberOfRooms) || numberOfRooms <= 0) {
            bookingResult.innerHTML = `<div class="alert alert-danger" role="alert">Please enter valid booking details.</div>`;
            return;
        }

        try {
            bookingResult.innerHTML = '<div class="spinner-border" role="status"><span class="visually-hidden">Booking...</span></div>'; // Show loading spinner
            const token = getBearerToken();
            const response = await fetch('https://localhost:7257/api/BookRoom', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    hotelId,
                    checkInDate,
                    checkOutDate,
                    numberOfGuests,
                    numberOfRooms
                })
            });
            if (!response.ok) throw new Error('Network response was not ok');
            const result = await response.json();
            bookingResult.innerHTML = result.success ? `<div class="alert alert-success" role="alert">Booking successful! ${result.message}</div>` : `<div class="alert alert-danger" role="alert">Booking failed: ${result.message}</div>`;
        } catch (error) {
            bookingResult.innerHTML = `<div class="alert alert-danger" role="alert">Error: ${error.message}</div>`;
        }
    });
});
