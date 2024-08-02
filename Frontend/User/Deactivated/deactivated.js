// script.js
async function requestActivation() {
    try {
        const response = await fetch('https://localhost:7032/RequestReactivation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}` // Include token if required for authentication
            },
            body: JSON.stringify({ userID: localStorage.getItem('userID') })
        });

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const result = await response.json();
        alert('Your reactivation request has been sent successfully.');
        // Optionally, redirect to another page or handle the response as needed
    } catch (error) {
        console.error('Error requesting reactivation:', error);
        alert('There was an error processing your request. Please try again later.');
    }
}
