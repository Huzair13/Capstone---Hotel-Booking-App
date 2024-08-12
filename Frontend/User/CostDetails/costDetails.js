const token = localStorage.getItem('token');
const params = new URLSearchParams(window.location.search);
const hotelId = params.get('hotelId');
const checkInDate = params.get('checkInDate');
const checkOutDate = params.get('checkOutDate');
const numOfGuests = params.get('numOfGuests');
const numOfRooms = params.get('numOfRooms');

if(!hotelId || !checkInDate || !checkOutDate || !numOfGuests || !numOfRooms){
    window.location.href='/User/UserHome/userHome.html';
}

document.addEventListener('DOMContentLoaded', () => {
    spinner.style.display = 'block'; 
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
                document.getElementById('mainDiv').style.display = 'none';
                document.body.style.background = "#748D92";
                // window.location.href = "/Deactivated/deactivated.html"
            }
            console.log(isActive);
        })
        .catch(error => {
            console.error('Error fetching IsActive status:', error);
        });

    
        
    document.getElementById('request-activation').addEventListener('click', function() {
        fetch('https://huzairhotelbookingapi.azure-api.net/RequestForActivation', {
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
            
            fetch('https://localhost:7032/RequestForActivation', {
                method: 'POST',
                headers: {
                    Authorization: `Bearer ${token}`,
                    'Content-Type': 'application/json'
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


});

document.getElementById('bookingDetails').innerHTML = `
    <div class="detail-item"><span>Hotel ID:</span> <span>${hotelId}</span></div>
    <div class="detail-item"><span>Check-in Date:</span> <span>${checkInDate}</span></div>
    <div class="detail-item"><span>Check-out Date:</span> <span>${checkOutDate}</span></div>
    <div class="detail-item"><span>Number of Guests:</span> <span>${numOfGuests}</span></div>
    <div class="detail-item"><span>Number of Rooms:</span> <span>${numOfRooms}</span></div>
`;


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

// Fetch total amount
fetch('https://huzairhotelbookingapi.azure-api.net/Booking/api/CalculateTotalAmount', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
        
    },
    body: JSON.stringify({
        hotelId: parseInt(hotelId),
        checkInDate: checkInDate,
        checkOutDat: checkOutDate,
        numOfGuests: parseInt(numOfGuests),
        numOfRooms: parseInt(numOfRooms)
    })
})
    .then(response => response.json())
    .then(data => {
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
        console.log(data)
        document.getElementById('totalAmount').innerHTML = `<div class="detail-item"><span>💸 Total Amount :</span> <span>₹${data.totalAmount}</span></div>`;
        document.getElementById('discount').innerHTML = `<div class="detail-item"><span>💸 Discount :</span> <span>₹${data.discount}</span></div>`;
        document.getElementById('finalAmount').innerHTML = `<div class="detail-item"><span>💸 Final Amount:</span> <span>₹${data.finalAmount}</span></div>`;
        document.getElementById('proceedToPay').classList.remove('d-none');
        document.getElementById('proceedToPay').addEventListener('click', () => {

            const button = document.getElementById('proceedToPay');
            const buttonText = document.getElementById('buttonText');
            const spinner = document.getElementById('spinner2');

            button.disabled = true;
            buttonText.style.display = 'none';
            spinner.style.display = 'inline-block';

            window.location.href = `/User/PaymentMode/paymentMode.html?hotelId=${hotelId}&checkInDate=${checkInDate}&checkOutDate=${checkOutDate}&numOfGuests=${numOfGuests}&numOfRooms=${numOfRooms}&totalAmount=${data}`;

            button.disabled = false;
            buttonText.style.display = 'inline';
            spinner.style.display = 'none';
        });
    })
    .catch(error =>{ 
        console.error('Error:', error)
        showAlert(error,'danger');
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
    }
);