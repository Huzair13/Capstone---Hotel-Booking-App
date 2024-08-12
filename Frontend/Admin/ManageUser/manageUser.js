document.getElementById('updateRoleForm').addEventListener('submit', async function (event) {
    event.preventDefault();
    document.getElementById('spinner').style.display = 'block'; 
    document.getElementById('overlay').style.display = 'block';
    const userId = document.getElementById('userId').value;
    const userRole = document.getElementById('userRole').value;

    try {
        const response = await fetch(`https://huzairhotelbookingapi.azure-api.net/UpdateUserRole?userId=${userId}&userRole=${userRole}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                Authorization: `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (response.ok) {
            alert('User role updated successfully');
        } else {
            const error = await response.json();
            alert(`Error: ${error.message}`);
        }
    } catch (error) {
        alert('An unexpected error occurred');
    }finally{
        document.getElementById('spinner').style.display = 'none'; 
        document.getElementById('overlay').style.display = 'none';
    }
});

document.addEventListener('DOMContentLoaded',function(){
    
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
});