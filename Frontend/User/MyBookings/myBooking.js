const token = localStorage.getItem('token');
const userId = getUserIdFromToken(token);

const bookingsPerPage = 6;
let currentPage = 1;
let bookings = [];
let originalBookings = [];

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
        // console.log("token not here")
        window.location.href = '/Login/Login.html';
    }
    if (token && isTokenExpired(token)) {
        // console.log("token Expired")
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
                document.getElementById('footer').style.display = 'none';
                // window.location.href = "/Deactivated/deactivated.html"
            }
            console.log(isActive);
        })
        .catch(error => {
            console.error('Error fetching IsActive status:', error);
        });

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

    // Fetch bookings for the user
    fetch(`https://huzairhotelbookingapi.azure-api.net/Booking/api/GetBookingByUser`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            bookings = data;
            originalBookings = data;
            displayBooking(data.slice(0, bookingsPerPage));
            renderPagination(data.length, bookingsPerPage, currentPage)
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
        })
        .catch(error => console.error('Error:', error));



    document.getElementById('request-activation').addEventListener('click', function () {
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

        document.getElementById('request-form').addEventListener('submit', function (event) {
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

})
// Function to decode JWT and extract userId if needed
function getUserIdFromToken(token) {
    if (!token) return null;
    var userId = localStorage.getItem('userID');
    return userId; // Assuming userId is stored in 'name' claim
}
// Function to handle booking cancellation
function handleCancelBooking(bookingId) {
    fetch(`https://huzairhotelbookingapi.azure-api.net/Booking/api/GetUserCancellationCount`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(cancellationCount => {
            if (cancellationCount > 2) {
                if (confirm('If you cancel more than 2 times, your account will be temporarily deactivated. Do you want to proceed?')) {
                    if (confirm('Are you sure you want to cancel this booking?')) {
                        cancelBooking(bookingId);
                        localStorage.removeItem('token');
                        window.reload();
                    }
                }
            } else {
                if (confirm('Are you sure you want to cancel this booking?')) {
                    cancelBooking(bookingId);
                }
            }
        })
        .catch(error => console.error('Error:', error));
}

// Function to cancel booking
function cancelBooking(bookingId) {
    document.getElementById('spinner').style.display = 'block'; 
    document.getElementById('overlay').style.display = 'block';
    fetch(`https://huzairhotelbookingapi.azure-api.net/Cancel/api/CancelBooking/${bookingId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            alert('Booking cancelled successfully.');
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
            window.location.reload();
        })
        .catch(error => {
            console.error('Error:', error)
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
        });
}

function fetchRefundAmount(bookingId) {
    return fetch(`https://huzairhotelbookingapi.azure-api.net/Cancel/api/GetCancelByBookingID/${bookingId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            return data.refundAmount;
        })
        .catch(error => {
            console.error('Error fetching refund amount:', error);
            throw error;  // Re-throw the error for further handling if needed
        });
}



function renderPagination(totalBookings, bookingsPerPage, currentPage) {
    const paginationContainer = document.querySelector('.pagination');
    paginationContainer.innerHTML = '';

    const pageCount = Math.ceil(totalBookings / bookingsPerPage);

    if (pageCount <= 1) return;

    if (currentPage > 1) {
        const prevItem = document.createElement('li');
        prevItem.className = 'page-item';
        prevItem.innerHTML = `<a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a>`;
        paginationContainer.appendChild(prevItem);
    }

    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(pageCount, currentPage + 2);

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

    // Add event listeners for pagination links
    document.querySelectorAll('.page-link').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const page = parseInt(this.getAttribute('data-page'));
            if (page >= 1 && page <= pageCount) {
                currentPage = page;
                const start = (page - 1) * bookingsPerPage;
                const end = start + bookingsPerPage;
                displayBooking(bookings.slice(start, end));
                renderPagination(totalBookings, bookingsPerPage, currentPage);
            }
        });
    });
}

document.getElementById('search-button').addEventListener('click', () => {

    const query = document.getElementById('hotel-text-input').value.trim().toLowerCase();

    if (query === '') {
        bookings = originalBookings.slice(0);
    } else {
        bookings = originalBookings.filter(booking => booking.bookingId === parseInt(query));
    }

    currentPage = 1;
    displayBooking(bookings.slice(0, bookingsPerPage));
    renderPagination(bookings.length, bookingsPerPage, currentPage);

});

document.getElementById('filter-button').addEventListener('click', () => {
    const bookingStatus = document.getElementById('status').value;
    console.log(bookingStatus);

    if (bookingStatus === 'All') {
        bookings = originalBookings.slice();
    } else if (bookingStatus === "Active") {
        bookings = originalBookings.filter(booking => !booking.isCancelled);
    } else if (bookingStatus === "Cancelled") {
        bookings = originalBookings.filter(booking => booking.isCancelled);
    }

    console.log(bookings);

    currentPage = 1;
    displayBooking(bookings.slice(0, bookingsPerPage));
    renderPagination(bookings.length, bookingsPerPage, currentPage);
});



function displayBooking(data) {
    const bookingsContainer = document.getElementById('bookingsContainer');
    bookingsContainer.innerHTML = '';
    if (Array.isArray(data) && data.length > 0) {
        data.forEach(booking => {
            // Create a booking card
            const bookingCard = document.createElement('div');
            bookingCard.className = 'booking-card';
            const statusClass = booking.isCancelled ? 'status-cancelled' : 'status-active';
            const cancelButton = booking.isCancelled ? '' : `<button class="btn btn-danger cancel-btn" data-booking-id="${booking.bookingId}">Cancel Booking</button>`;

            bookingCard.innerHTML = `
                <h5>Booking ID: ${booking.bookingId}</h5>
                <p><strong>Hotel ID:</strong> ${booking.hotelId}</p>
                <p><strong>Check-in Date:</strong> ${booking.checkInDate}</p>
                <p><strong>Check-out Date:</strong> ${booking.checkOutDate}</p>
                <p><strong>Number of Guests:</strong> ${booking.numberOfGuests}</p>
                <p><strong>Total Price:</strong> $${booking.totalPrice}</p>
                <p><strong>Discount:</strong> $${booking.discount}</p>
                <p><strong>Final Amount:</strong> $${booking.finalAmount}</p>
                <p><strong>Allocated Rooms:</strong> ${booking.roomNumbers ? booking.roomNumbers.join(', ') : 'Not available'}</p>
                <p><strong>Payment Mode:</strong> 
                    ${booking.paymentMode === 0 ? 'offline' : 'online'}
                </p>
                <p><strong>Payment Status:</strong> ${booking.isPaid ? "Completed" : "Not Completed"}</p>
                <div class="d-flex justify-content-between flex-wrap flex-column flex-md-row"><strong>Status:</strong> <div class="${statusClass}"><span>${booking.isCancelled ? 'Cancelled' : 'Active'}</span></div></div>
                ${cancelButton}
            `;

            if (booking.isCancelled) {
                fetchRefundAmount(booking.bookingId)
                    .then(refundAmount => {
                        const bookingDetails = document.createElement('div');
                        bookingDetails.className = 'card-footer';
                        bookingDetails.innerHTML = `<p class="card-text"><strong>Refund Amount: </strong>₹ ${refundAmount}</p>`;
                        bookingCard.appendChild(bookingDetails);
                    })
                    .catch(error => {
                        console.error('Error fetching refund amount:', error);
                    });
            }

            bookingsContainer.appendChild(bookingCard);
        });
    } else {
        bookingsContainer.innerHTML = '<p>No bookings found for this user.</p>';
    }

    // Add event listeners for cancel buttons
    document.querySelectorAll('.cancel-btn').forEach(button => {
        button.addEventListener('click', (event) => {
            const bookingId = event.target.getAttribute('data-booking-id');
            handleCancelBooking(bookingId);
        });
    });
}


