function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

async function fetchHotelDetails(hotelId) {
    try {
        const token = localStorage.getItem('token');
        const fetchOptions = {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            }
        };

        const hotelResponse = await fetch(`https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetHotelByID/${hotelId}`, fetchOptions);
        const hotel = await hotelResponse.json();

        if (hotel) {
            // Populate carousel with hotel images
            const carouselInner = document.querySelector('.carousel-inner');
            carouselInner.innerHTML = '';
            hotel.hotelImages.forEach((imageUrl, index) => {
                const carouselItem = document.createElement('div');
                carouselItem.classList.add('carousel-item');
                if (index === 0) carouselItem.classList.add('active');
                carouselItem.innerHTML = `<img src="${imageUrl}" class="d-block w-100" alt="Hotel Image">`;
                carouselInner.appendChild(carouselItem);
            });

            new bootstrap.Carousel(document.querySelector('#hotelCarousel'), {
                interval: 3000, // Set the interval to 3000 milliseconds (3 seconds)
                ride: 'carousel' // Ensure the carousel starts automatically
            });

            // Populate modal with thumbnails
            const modalMainImage = document.getElementById('modalMainImage');
            const modalThumbnails = document.getElementById('modalThumbnails');
            modalMainImage.src = hotel.hotelImages[0] || '';
            modalThumbnails.innerHTML = '';
            hotel.hotelImages.forEach(imageUrl => {
                const thumbnailImg = document.createElement('img');
                thumbnailImg.src = imageUrl;
                thumbnailImg.alt = 'Hotel Thumbnail';
                thumbnailImg.classList.add('img-fluid');
                thumbnailImg.dataset.large = imageUrl;
                thumbnailImg.addEventListener('click', () => {
                    modalMainImage.src = thumbnailImg.dataset.large;
                });
                modalThumbnails.appendChild(thumbnailImg);
            });



            const [roomsResponse, amenitiesResponse, reviewsResponse] = await Promise.all([
                fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllRooms', fetchOptions),
                fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/GetAllAmenities', fetchOptions),
                fetch(`https://huzairhotelbookingapi.azure-api.net/Rating/api/GetRatingByHotelID/${hotelId}`, fetchOptions)
            ]);

            const rooms = await roomsResponse.json();
            const amenities = await amenitiesResponse.json();
            const reviews = await reviewsResponse.json();
            const hotelRoomIds = hotel.roomIDs;

            const hotelRooms = rooms.filter(room => hotelRoomIds.includes(room.roomNumber));

            const minRentRoom = hotelRooms.reduce((minRoom, currentRoom) => {
                return (minRoom.rent < currentRoom.rent) ? minRoom : currentRoom;
            }, { rent: Infinity });

            const leastRent = minRentRoom ? minRentRoom.rent : 'N/A';

            // Find amenities details
            const hotelAmenities = hotel.amenities.map(id => {
                const amenity = amenities.find(amenity => amenity.id === id);
                return amenity ? amenity.name : 'Unknown';
            }).join(', ');

            // Update hotel details section
            console.log(hotel)
            document.getElementById('hotelName').textContent = hotel.name;
            document.getElementById('hotelAddressValue').textContent = hotel.address;
            // Format and set the least rent value
            const leastRentElement = document.getElementById('hotelLeastRentValue');
            leastRentElement.textContent = `$${leastRent}`;

            document.getElementById('hotelAverageRatingsValue').textContent = `${hotel.averageRatings}/5`;

            const ratingBadge = document.getElementById('hotelAverageRatingsValue');
            let badgeColor = '#ffcc00'; // Default color
            if (hotel.averageRatings >= 4.5) {
                badgeColor = '#28a745'; // Green for high value
            } else if (hotel.averageRatings >= 3) {
                badgeColor = '#ffc107'; // Yellow for medium value
            } else {
                badgeColor = '#dc3545'; // Red for low value
            }
            ratingBadge.style.backgroundColor = badgeColor;

            const starRatings = document.getElementById('hotelStarRatings');
            const fullStars = Math.floor(hotel.averageRatings);
            const halfStar = hotel.averageRatings % 1 !== 0;
            let starsHTML = '';

            for (let i = 0; i < fullStars; i++) {
                starsHTML += '<i class="fas fa-star"></i>';
            }
            if (halfStar) {
                starsHTML += '<i class="fas fa-star-half-alt"></i>';
            }
            for (let i = fullStars + (halfStar ? 1 : 0); i < 5; i++) {
                starsHTML += '<i class="far fa-star"></i>';
            }

            starRatings.innerHTML = starsHTML;
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
            const amenitiesContainer = document.getElementById('hotelAmenitiesValue');
            amenitiesContainer.innerHTML = Object.keys(amenitiesIcons)
                .filter(amenity => hotelAmenities.includes(amenity))
                .map(amenity => `<i class="${amenitiesIcons[amenity]}" title="${amenity}"></i>`)
                .join(' ');

            // Populate reviews section
            const reviewsContainer = document.getElementById('reviewsContainer');
            reviewsContainer.innerHTML = '';
            reviews.slice(0, 3).forEach(review => {
                const date = new Date(review.createdAt).toLocaleDateString();
                const time = new Date(review.createdAt).toLocaleTimeString();
                const reviewElement = document.createElement('div');
                reviewElement.classList.add('review');
                reviewElement.innerHTML = `
                    <div class="review-content">
                        <div class="review-user">User ${review.userId}</div>
                        <div class="review-feedback">
                            <p class="review-feedback" data-full="${review.feedback}">${review.feedback.slice(0, 100)}</p>
                            <span class="expand-btn">Read More</span>
                        </div>
                    </div>
                    <div class="d-flex flex-column align-items-end">
                        <div class="review-rating">${'⭐'.repeat(review.ratingValue)}</div>
                        <div class="review-date">${date} <br> ${time}</div>
                    </div>
                `;
                reviewsContainer.appendChild(reviewElement);
            });

            if (reviews.length > 3) {
                document.getElementById('viewAllReviews').style.display = 'block';
            }
        }
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
    } catch (error) {
        console.error('Error fetching hotel details:', error);
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
    }
}

async function deleteHotel(hotelId) {
    if (hotelId) {
        fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/DeleteHotel', {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify(hotelId) 
        })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Network response was not ok.');
            }
        })
        .then(data => {
            console.log('Response Data:', data); 
            alert('Hotel successfully deleted.');
            window.location.href="/HotelManage/hotelManage.html";
        })
        .catch(error => {
            console.error('Fetch Error:', error); // Log fetch errors
            alert('An error occurred while deleting the hotel.');
        });
    }
    
}

document.addEventListener('DOMContentLoaded', () => {

    document.getElementById('spinner').style.display = 'block'; 
    document.getElementById('overlay').style.display = 'block';

    if(localStorage.getItem('role')!=="Admin"){
        window.location.href="/Login/Login.html"
    }

    const deleteButton = document.getElementById('confirmDeleteButton');
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
    if (!token) {
        window.location.href = '/Login/Login.html';
    }
    if (token && isTokenExpired(token)) {
        localStorage.removeItem('token');
        window.location.href = '/Login/Login.html';
    }

    // const token = localStorage.getItem('token');
    const hotelId = getQueryParam('hotelId');
    const viewAllReview = document.getElementById('viewAllReviews');
    viewAllReview.addEventListener('click', (event) => {
        event.preventDefault();
        window.location.href = `reviews.html?hotelId=${hotelId}`;
    });

    if (hotelId) {
        fetchHotelDetails(hotelId);
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
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
    

    document.getElementById('editHotel').addEventListener('click', function (){
        window.location.href=`/Admin/EditHotel/editHotel.html?hotelId=${hotelId}`;
    });

    document.getElementById('addImagesToHotel').addEventListener('click',function (){
        window.location.href=`/Admin/AddImages/addImages.html?hotelId=${hotelId}`;
    })

    document.getElementById('addAmenitiesAdmin').addEventListener('click',function (){
        window.location.href=`/Admin/AddAmenities/addAmenity.html?hotelId=${hotelId}`;
    })

    document.getElementById('removeAmenitiesAdmin').addEventListener('click',function (){
        window.location.href=`/Admin/RemoveAmenities/removeAmenities.html?hotelId=${hotelId}`;
    })

    document.getElementById('addRoomsAdmin').addEventListener('click',function (){
        window.location.href=`/Admin/AddRooms/addRooms.html?hotelId=${hotelId}`;
    })

    document.getElementById('removeRoomsAdmin').addEventListener('click',function (){
        window.location.href=`/Admin/RemoveRooms/removeRooms.html?hotelId=${hotelId}`;
    })


    document.getElementById('reviewsContainer').addEventListener('click', event => {
        if (event.target.classList.contains('expand-btn')) {
            const feedback = event.target.previousElementSibling;
            feedback.classList.toggle('expanded');
            event.target.textContent = feedback.classList.contains('expanded') ? 'Read Less' : 'Read More';
        }
    });

    document.getElementById('viewAllReviews').addEventListener('click', () => {
        // Handle the action for viewing all reviews
    });

    deleteButton.addEventListener('click', function () {
        deleteHotel(hotelId);
    });

});