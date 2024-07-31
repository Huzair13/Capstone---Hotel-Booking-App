function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

let userRatingsAll =[]

async function fetchUserRating(hotelId, userId) {
    try {
        const response = await fetch(`https://localhost:7226/api/GetAllRating`, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (response.ok) {
            const ratings = await response.json();
            const parsedHotelId = parseInt(hotelId, 10);
            const parsedUserId = parseInt(userId, 10);

            const userRating = ratings.find(rating =>
                rating.hotelId === parsedHotelId && rating.userId === parsedUserId
            );

            if (userRating) {
                userRatingsAll =userRating;
                displayUserRating(userRating);
            } else {
                document.getElementById('updateRatingButton').style.display = 'none'
                document.getElementById('rateButton').style.display = 'block';
            }
        } else {
            throw new Error('Failed to fetch user rating');
        }
    } catch (error) {
        console.error('Error fetching user rating:', error);
    }
}

function displayUserRating(userRating) {
    console.log(userRating);

    const starContainer = document.getElementById('userStarRating');
    const feedbackContainer = document.getElementById('userFeedback');
    const rateButton = document.getElementById('rateButton');

    // Hide the rate button
    rateButton.style.display = 'none';

    // Clear previous stars
    starContainer.innerHTML = '';

    // Parse the rating value as a float
    const ratingValue = parseFloat(userRating.ratingValue);

    // Validate rating value
    if (isNaN(ratingValue) || ratingValue < 0 || ratingValue > 5) {
        console.error('Invalid rating value:', ratingValue);
        return;
    }

    // Display full stars
    const fullStars = Math.floor(ratingValue);
    const hasHalfStar = (ratingValue % 1) >= 0.5;

    for (let i = 0; i < fullStars; i++) {
        const star = document.createElement('span');
        star.classList.add('fa', 'fa-star', 'checked');
        starContainer.appendChild(star);
    }

    // Display half star if applicable
    if (hasHalfStar) {
        const halfStar = document.createElement('span');
        halfStar.classList.add('fa', 'fa-star-half-alt', 'checked');
        starContainer.appendChild(halfStar);
    }

    // Display empty stars
    const totalStars = 5;
    const remainingStars = totalStars - fullStars - (hasHalfStar ? 1 : 0);
    for (let i = 0; i < remainingStars; i++) {
        const star = document.createElement('span');
        star.classList.add('fa', 'fa-star');
        starContainer.appendChild(star);
    }

    // Display feedback
    feedbackContainer.textContent = userRating.feedback || '';
}


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

        const hotelResponse = await fetch(`https://localhost:7257/api/GetHotelByID/${hotelId}`, fetchOptions);
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
                fetch('https://localhost:7257/api/GetAllRooms', fetchOptions),
                fetch('https://localhost:7257/api/GetAllAmenities', fetchOptions),
                fetch(`https://localhost:7226/api/GetRatingByHotelID/${hotelId}`, fetchOptions)
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
            leastRentElement.textContent = `₹${leastRent}`;

            document.getElementById('hotelAverageRatingsValue').textContent = `${hotel.averageRatings}/5`;

            const ratingBadge = document.getElementById('hotelAverageRatingsValue');
            let badgeColor = '#ffcc00'; 
            if (hotel.averageRatings >= 4.5) {
                badgeColor = '#28a745'; 
            } else if (hotel.averageRatings >= 3) {
                badgeColor = '#ffc107'; 
            } else {
                badgeColor = '#dc3545'; 
            }
            ratingBadge.style.backgroundColor = badgeColor;

            const starRatings = document.getElementById('hotelStarRatings');
            const fullStars = Math.floor(hotel.averageRatings);
            const halfStar = hotel.averageRatings % 1 !== 0;
            let starsHTML = '';

            for (let i = 0; i < fullStars; i++) {
                starsHTML += '<i class="fas fa-star checked"></i>';
            }
            if (halfStar) {
                starsHTML += '<i class="fas fa-star-half-alt checked"></i>';
            }
            for (let i = fullStars + (halfStar ? 1 : 0); i < 5; i++) {
                starsHTML += '<i class="far fa-star text-black"></i>';
            }

            starRatings.innerHTML = starsHTML;

            const amenitiesIcons = {
                "AC": "fas fa-snowflake",
                "TV": "fas fa-tv",
                "Fan": "fas fa-fan",
                "Geyser": "fas fa-tachometer-alt",
                "Wifi": "fas fa-wifi",
                "Play Station": "fas fa-gamepad"
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
                        <div class="review-date text-light">${date} <br> ${time}</div>
                    </div>
                `;
                reviewsContainer.appendChild(reviewElement);
            });

            if (reviews.length > 3) {
                document.getElementById('viewAllReviews').style.display = 'block';
            }
        }
    } catch (error) {
        console.error('Error fetching hotel details:', error);
    }
}

document.addEventListener('DOMContentLoaded', () => {

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

    fetch(`https://localhost:7032/IsActive/${localStorage.getItem('userID')}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(isActive => {
            if (!isActive) {
                document.getElementById('deactivatedDiv').style.display = 'block';
                document.getElementById('mainDiv').style.display = 'none';
                // window.location.href = "/Deactivated/deactivated.html"
            }
            console.log(isActive);
        })
        .catch(error => {
            console.error('Error fetching IsActive status:', error);
        });

    // const token = localStorage.getItem('token');
    const hotelId = getQueryParam('hotelId');
    const viewAllReview = document.getElementById('viewAllReviews');
    viewAllReview.addEventListener('click', (event) => {
        event.preventDefault();
        window.location.href = `reviews.html?hotelId=${hotelId}`;
    });

    const userId = localStorage.getItem('userID');
    fetchUserRating(hotelId, userId);

    // Event listener for the "Rate" button
    document.getElementById('rateButton').addEventListener('click', () => {
        const ratingModal = new bootstrap.Modal(document.getElementById('ratingModal'));
        ratingModal.show();
    });

    // Event listener for the "Rate" button
    document.getElementById('updateRatingButton').addEventListener('click', () => {
        const ratingModal = new bootstrap.Modal(document.getElementById('ratingModalUpdate'));
        document.getElementById('ratingUpdateValue').value = parseInt(userRatingsAll.ratingValue,10);
        document.getElementById('ratingUpdateFeedback').textContent = userRatingsAll.feedback;
        ratingModal.show();
    });

    // Event listener for the form submission
    document.getElementById('submitRating').addEventListener('click', async () => {
        const ratingValue = parseInt(document.getElementById('ratingValue').value);
        const feedback = document.getElementById('ratingFeedback').value;
        const token = localStorage.getItem('token');

        try {
            const response = await fetch('https://localhost:7226/api/AddRating', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    hotelId: parseInt(hotelId),
                    ratingValue: parseFloat(ratingValue),
                    feedback: feedback
                })
            });

            if (response.ok) {
                alert('Rating submitted successfully!');
                fetchUserRating(hotelId, userId);
                const ratingModal = bootstrap.Modal.getInstance(document.getElementById('ratingModal'));
                ratingModal.hide();
            } else if (response.status === 400) {
                document.getElementById('ratingError').style.display = 'block';
            } else {
                throw new Error('Failed to submit rating');
            }
        } catch (error) {
            console.error('Error submitting rating:', error);
        }
    });

    // Event listener for the form submission
    document.getElementById('submitUpdateRating').addEventListener('click', async () => {
        const ratingValue = parseInt(document.getElementById('ratingUpdateValue').value);
        const feedback = document.getElementById('ratingUpdateFeedback').value;
        const token = localStorage.getItem('token');

        try {
            const response = await fetch(`https://localhost:7226/api/updateRating/${userRatingsAll.id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    hotelId: parseInt(hotelId),
                    feedback: feedback,
                    ratingValue: parseFloat(ratingValue)
                })
            });

            if (response.ok) {
                alert('Rating Updated successfully!');
                fetchUserRating(hotelId, userId);
                const ratingModal = bootstrap.Modal.getInstance(document.getElementById('ratingModalUpdate'));
                ratingModal.hide();
                window.location.reload();
            } else if (response.status === 400) {
                document.getElementById('ratingError').style.display = 'block';
            } else {
                throw new Error('Failed to submit rating');
            }
        } catch (error) {
            console.error('Error submitting rating:', error);
        }
    });

    

    const decreaseGuestsBtn = document.getElementById('decreaseGuests');
    const increaseGuestsBtn = document.getElementById('increaseGuests');
    const numGuestsInput = document.getElementById('numGuests');

    const decreaseRoomsBtn = document.getElementById('decreaseRooms');
    const increaseRoomsBtn = document.getElementById('increaseRooms');
    const numRoomsInput = document.getElementById('numRooms');

    decreaseGuestsBtn.addEventListener('click', () => {
        numGuestsInput.value = Math.max(1, parseInt(numGuestsInput.value) - 1);
    });

    increaseGuestsBtn.addEventListener('click', () => {
        numGuestsInput.value = parseInt(numGuestsInput.value) + 1;
    });

    decreaseRoomsBtn.addEventListener('click', () => {
        numRoomsInput.value = Math.max(1, parseInt(numRoomsInput.value) - 1);
    });

    increaseRoomsBtn.addEventListener('click', () => {
        numRoomsInput.value = parseInt(numRoomsInput.value) + 1;
    });

    document.getElementById('checkAvailability').addEventListener('click', async () => {
        const checkinDate = document.getElementById('checkinDate').value;
        const checkoutDate = document.getElementById('checkoutDate').value;
        const numGuests = document.getElementById('numGuests').value;
        const numRooms = document.getElementById('numRooms').value;

        const requestBody = {
            hotelId: hotelId,
            checkInDate: checkinDate,
            checkOutDate: checkoutDate,
            numOfGuests: numGuests,
            numOfRooms: numRooms
        };

        console.log(requestBody)
        try {
            const response = await fetch('https://localhost:7257/api/CheckHotelAvailability', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(requestBody)
            });

            console.log(response)

            const result = await response.json();
            const isAvailable = response.status === 200 ? result : false;
            console.log(isAvailable)

            const queryString = new URLSearchParams({
                hotelId: hotelId,
                checkInDate: checkinDate,
                checkOutDate: checkoutDate,
                numOfGuests: numGuests,
                numOfRooms: numRooms,
                isAvailable: isAvailable
            }).toString();

            window.location.href = `/AvailabilityResult/availabilityResult.html?${queryString}`;
        } catch (error) {
            console.error('Error:', error);
        }
    });

    if (hotelId) {
        fetchHotelDetails(hotelId);
    }

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
});