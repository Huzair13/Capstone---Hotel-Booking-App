document.getElementById('hotelForm').addEventListener('submit', async function (event) {
    event.preventDefault();

    document.getElementById('spinner').style.display = 'block'; 
    document.getElementById('overlay').style.display = 'block';
    
    const formData = new FormData(this);
    const files = document.getElementById('images').files;

    for (let i = 0; i < files.length; i++) {
        formData.append('files', files[i]);
    }
    try {
        const response = await fetch('https://huzairhotelbookingapi.azure-api.net/Hotel/api/AddHotel', {
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