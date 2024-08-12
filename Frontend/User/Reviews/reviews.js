document.addEventListener('DOMContentLoaded', () => {

    document.getElementById('spinner').style.display = 'block'; 
    document.getElementById('overlay').style.display = 'block';

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

    fetch(`https://huzairhotelbookingapi.azure-api.net/IsActive/${localStorage.getItem('userID')}`, {
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
                document.body.style.background = "#748D92";
                document.getElementById('mainDiv').style.display = 'none';
                // window.location.href = "/Deactivated/deactivated.html"
            }
            console.log(isActive);
        })
        .catch(error => {
            console.error('Error fetching IsActive status:', error);
        });

    
        
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

    const hotelId = getQueryParam('hotelId');
    const filterIcon = document.querySelector('.filter-icon');
    const dropdownMenu = document.querySelector('.dropdown-menu');

    filterIcon.addEventListener('click', () => {
        const dropdown = document.querySelector('.dropdown');
        const bsDropdown = new bootstrap.Dropdown(dropdown);
        bsDropdown.toggle();
    });

    const backToHotel = document.getElementById('backToHotel');
    backToHotel.addEventListener('click', function (event) {
        event.preventDefault(); // Prevent the default action of the link
        const url = `/User/HotelDetails/hotelDetails.html?hotelId=${hotelId}`;
        window.location.href = url;
    });
   
    if (hotelId) {
        fetchReviews(hotelId);
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';

        document.getElementById('latestReview').addEventListener('click', () => fetchReviews(hotelId, 'latest'));
        document.getElementById('oldReview').addEventListener('click', () => fetchReviews(hotelId, 'old'));
        document.getElementById('topRating').addEventListener('click', () => fetchReviews(hotelId, 'topRating'));
        document.getElementById('lowRating').addEventListener('click', () => fetchReviews(hotelId, 'lowRating'));
    } else {
        document.getElementById('reviewsContainer').innerHTML = '<p>No hotel ID provided.</p>';
    }
});

function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

function formatDateTime(dateString) {
    const optionsDate = { year: 'numeric', month: 'long', day: 'numeric' };
    const optionsTime = { hour: '2-digit', minute: '2-digit' };

    const date = new Date(dateString);
    const formattedDate = date.toLocaleDateString(undefined, optionsDate);
    const formattedTime = date.toLocaleTimeString(undefined, optionsTime);

    return {
        date: formattedDate,
        time: formattedTime
    };
}

async function fetchReviews(hotelId, sortOption = 'latest') {
    try {
        const response = await fetch(`https://huzairhotelbookingapi.azure-api.net/Rating/api/GetRatingByHotelID/${hotelId}`,{
            headers : {
                Authorization : `Bearer ${localStorage.getItem('token')}`,
                
            }
        });
        let reviews = await response.json();

        if (sortOption === 'latest') {
            reviews.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
        } else if (sortOption === 'old') {
            reviews.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
        } else if (sortOption === 'topRating') {
            reviews.sort((a, b) => b.ratingValue - a.ratingValue);
        } else if (sortOption === 'lowRating') {
            reviews.sort((a, b) => a.ratingValue - b.ratingValue);
        }

        const reviewsContainer = document.getElementById('reviewsContainer');
        const noReviewsDiv = document.getElementById('noReviews');
        const reviewCount = document.getElementById('reviewCount');
        reviewsContainer.innerHTML = '';

        if (reviews.length > 0) {
            reviewCount.textContent = `Showing ${reviews.length} Reviews`;
            reviews.forEach(review => {
                const { date, time } = formatDateTime(review.createdAt);

                const reviewCard = document.createElement('div');
                reviewCard.classList.add('review-card');
                reviewCard.innerHTML = `
                    <div class="review-feedback">
                        ${review.feedback || 'No feedback provided'}
                    </div>
                    <div class="d-flex flex-column align-items-end">
                        <div class="review-rating">${'⭐'.repeat(review.ratingValue)}</div>
                        <div class="review-date">
                            ${date} <br> ${time}
                        </div>
                    </div>
                `;
                reviewsContainer.appendChild(reviewCard);
            });
            noReviewsDiv.style.display = 'none';
        } else {
            reviewCount.textContent = 'No reviews found';
            noReviewsDiv.style.display = 'block';
        }
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
    } catch (error) {
        console.error('Error fetching reviews:', error);
        document.getElementById('reviewsContainer').innerHTML = '<p>Failed to load reviews.</p>';
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
    }
}