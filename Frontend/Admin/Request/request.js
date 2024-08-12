const apiUrl = 'https://huzairhotelbookingapi.azure-api.net';
let currentAction = '';
let requestIdToProcess = null;

document.addEventListener('DOMContentLoaded', () => {
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

    const tokenTest = localStorage.getItem('token');
    if (!tokenTest) {
        window.location.href = '/Login/Login.html';
    }
    if (tokenTest && isTokenExpired(tokenTest)) {
        localStorage.removeItem('token');
        window.location.href = '/Login/Login.html';
    }

    if (localStorage.getItem('role') !== "Admin") {
        alert("Unauthorized");
        window.location.href = "/Login/Login.html";
        return;
    }

    fetchRequests('All');

    document.querySelectorAll('.filter-btn').forEach(button => {
        button.addEventListener('click', (e) => {
            document.querySelectorAll('.filter-btn').forEach(btn => btn.classList.remove('active-tab'));
            e.target.classList.add('active-tab');
            fetchRequests(e.target.getAttribute('data-status'));
        });
    });

    document.getElementById('confirmAction').addEventListener('click', () => {
        if (currentAction === 'Accept') {
            handleRequest('AcceptRequest');
        } else if (currentAction === 'Reject') {
            handleRequest('RejectRequest');
        }
    });
});

function fetchRequests(status) {
    document.getElementById('spinner').style.display = 'block'; 
    document.getElementById('overlay').style.display = 'block';

    fetch(`https://huzairhotelbookingapi.azure-api.net/GetAllRequests`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`,
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
            return response.json();
        })
        .then(data => {
            console.log(data);
            const filteredData = status === 'All' ? data.filter(req => req.status === "Requested") : data.filter(req => req.status === status);
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
            displayRequests(filteredData);
        })
        .catch(error => {
            console.error('Error fetching requests:', error)
            document.getElementById('spinner').style.display = 'none'; 
            document.getElementById('overlay').style.display = 'none';
        });
}

function displayRequests(requests) {
    const tableBody = document.getElementById('requestsTableBody');
    tableBody.innerHTML = '';
    requests.forEach(request => {
        let actionButtons = '';
        if (request.status === 'Requested') {
            actionButtons = `
                <button class="btn btn-success btn-sm" onclick="showConfirmation(${request.id}, 'Accept')">Accept</button>
                <button class="btn btn-danger btn-sm" onclick="showConfirmation(${request.id}, 'Reject')">Reject</button>
            `;
        }

        if (request.status === 'Accepted' || request.status === 'Rejected') {
            document.getElementById('actionsTable').style.display = 'none';
            tableBody.innerHTML += `
            <tr>
                <td>${request.id}</td>
                <td>${request.userId}</td>
                <td>${request.reason}</td>
                <td>${request.status}</td>
            </tr>
        `;
        }
        else {
            tableBody.innerHTML += `
            <tr>
                <td>${request.id}</td>
                <td>${request.userId}</td>
                <td>${request.reason}</td>
                <td>${request.status}</td>
                <td>${actionButtons}</td>
            </tr>
        `;
        }
    });
}

function showConfirmation(requestId, action) {
    requestIdToProcess = requestId;
    currentAction = action;
    document.getElementById('actionType').textContent = action === 'Accept' ? 'accept' : 'reject';
    const confirmationModal = new bootstrap.Modal(document.getElementById('confirmationModal'));
    confirmationModal.show();
}

function handleRequest(endpoint) {
    fetch(`${apiUrl}/${endpoint}?requestId=${requestIdToProcess}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
    })
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.json();
        })
        .then(() => {
            fetchRequests(document.querySelector('.filter-btn.active-tab')?.getAttribute('data-status') || 'All');
            const confirmationModal = bootstrap.Modal.getInstance(document.getElementById('confirmationModal'));
            if (confirmationModal) {
                confirmationModal.hide();
            }
        })
        .catch(error => console.error('Error handling request:', error));
}
