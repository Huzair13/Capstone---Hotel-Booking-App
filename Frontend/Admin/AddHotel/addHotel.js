document.getElementById('hotelForm').addEventListener('submit', async function (event) {
    event.preventDefault();
    spinner.style.display = 'block'; 


    const formData = new FormData(this);
    const files = document.getElementById('images').files;

    for (let i = 0; i < files.length; i++) {
        formData.append('files', files[i]);
    }
    try {
        const response = await fetch('https://localhost:7257/api/AddHotel', {
            method: 'POST',
            headers: {
                Authorization: `Bearer ${localStorage.getItem('token')}`
            },
            body: formData
        });

        if (response.ok) {
            alert('Hotel added successfully');
            this.reset();
            window.location.href = "/Admin/AdminHome/adminHome.html"
        } else {
            alert('Failed to add hotel');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred');
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