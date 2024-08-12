
const user = localStorage.getItem('userID');
const role = localStorage.getItem('role');
const userId = localStorage.getItem('userID');

const nameElementProfile = document.getElementById('userName');
const emailElementProfile = document.getElementById('userEmail');
const mobileElementProfile = document.getElementById('userMobile');
const ageElementProfile = document.getElementById('userAge');
const coinsElementStudent = document.getElementById('userCoins');

const studentElement = document.getElementById('studentData');


document.addEventListener('DOMContentLoaded', async function () {

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
            document.getElementById('footer').display.style = 'none';
            // window.location.href = "/Deactivated/deactivated.html"
        }
        console.log(isActive)

    } catch (error) {
        console.error('Error fetching IsActive status:', error);
    }


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

    fetch(`https://huzairhotelbookingapi.azure-api.net/getuser/${userId}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            
        }
    })
        .then(response => response.json())
        .then(data => {
            // console.log(data);
            studentElement.style.display = "block";
            nameElementProfile.style.fontWeight = "bold";
            nameElementProfile.textContent = data.name;
            ageElementProfile.textContent = data.age;
            emailElementProfile.textContent = data.email;
            mobileElementProfile.textContent = data.mobileNumber;
            coinsElementStudent.textContent = data.coinsEarned ? data.coinsEarned :0;
        })
        .catch(error => console.error('Error fetching student profile data:', error));
});
