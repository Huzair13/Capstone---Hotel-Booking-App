//OPEN LOGIN PAGE
var openLoginPage = () => {
    window.location.href = "/Login/Login.html"
}

//OPEN REGISTER PAGE
var openRegisterPage = () => {
    window.location.href = "/Register/RegisterOption.html"
}

//INITIALIZE LANDING CAROUSEL
document.addEventListener("DOMContentLoaded", function () {

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

    document.getElementById('localStay').addEventListener('click', function () {
        window.location.href = "/User/Home/home.html"
    });

    document.getElementById('allHotelFind').addEventListener('click', function () {
        window.location.href = "/User/AllHotels/AllHotels.html"
    });

    document.getElementById('checkByDates').addEventListener('click', function () {
        var myModal = new bootstrap.Modal(document.getElementById('dateModal'), {
            keyboard: false
        });
        myModal.show();
    });

    document.getElementById('dateForm').addEventListener('submit', function (event) {
        event.preventDefault();

        var checkinDate = new Date(document.getElementById('checkinDate').value);
        var checkoutDate = new Date(document.getElementById('checkoutDate').value);
        var today = new Date();

        if (isNaN(checkinDate) || isNaN(checkoutDate)) {
            alert('Please enter valid dates.');
            return;
        }

        if (checkinDate < today || checkoutDate < today) {
            alert('Dates cannot be in the past.');
            return;
        }

        if (checkinDate >= checkoutDate) {
            alert('Check-out date must be after check-in date.');
            return;
        }

        var url = `/User/CheckByDates/checkByDates.html?checkInDate=${encodeURIComponent(document.getElementById('checkinDate').value)}&checkOutDate=${encodeURIComponent(document.getElementById('checkoutDate').value)}`;

        window.location.href = url;
    });



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
                const isRequested = requests.some(request => {
                    return request.userId == userId && request.status === "Requested"; // added return statement
                });
                console.log(isRequested);

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


    var landingCarousel = document.querySelector("#LandingCarousel");
    if (landingCarousel) {
        var landingCarouselInstance = new bootstrap.Carousel(landingCarousel, {
            interval: 3000,
            ride: "carousel"
        });
    }

    //CARD SLIDER INITIALIZE FUNCTION
    function initializeCardSlider() {
        var cardSlider = document.querySelector("#CardSlider");
        if (!cardSlider) return;

        var cardCarouselInner = cardSlider.querySelector(".carousel-inner");
        var cardCarouselItems = cardSlider.querySelectorAll(".carousel-item");
        var cardWidth = cardCarouselItems[0].offsetWidth;
        var cardScrollPosition = 0;

        //SLIDING FUNCTION FO NEXT
        function slideToNext() {
            if (cardScrollPosition < cardCarouselInner.scrollWidth - cardWidth * 4) {
                cardScrollPosition += cardWidth;
            } else {
                cardScrollPosition = 0;
            }
            cardCarouselInner.scrollTo({
                left: cardScrollPosition,
                behavior: "smooth"
            });
        }

        // SLIDING FUNCTION FOR PREV 
        function slideToPrev() {
            if (cardScrollPosition > 0) {
                cardScrollPosition -= cardWidth;
            } else {
                cardScrollPosition = cardCarouselInner.scrollWidth - cardWidth * 4;
            }
            cardCarouselInner.scrollTo({
                left: cardScrollPosition,
                behavior: "smooth"
            });
        }

        //NEXT 
        cardSlider.querySelector(".carousel-control-next").addEventListener("click", function () {
            slideToNext();
        });

        // PREVIOUS
        cardSlider.querySelector(".carousel-control-prev").addEventListener("click", function () {
            slideToPrev();
        });

        var autoSlideInterval = setInterval(slideToNext, 3000);

        //ON HOVER STOP AUTOPLAY
        cardSlider.addEventListener("mouseenter", function () {
            clearInterval(autoSlideInterval);
        });
        cardSlider.addEventListener("mouseleave", function () {
            autoSlideInterval = setInterval(slideToNext, 3000);
        });

        //SMALL SCREEN CARD SLIDER
        window.addEventListener("resize", function () {
            cardWidth = cardCarouselItems[0].offsetWidth;
            cardScrollPosition = 0;
            cardCarouselInner.scrollTo({
                left: cardScrollPosition,
                behavior: "auto"
            });
        });

        //ON LOAD SLIDING INITIALIZE
        cardCarouselInner.scrollTo({
            left: cardScrollPosition,
            behavior: "auto"
        });

        var cardCarousel = new bootstrap.Carousel(cardSlider, {
            interval: 3000,
            ride: "carousel"
        });
    }

    //AUTO PLAY FOR SMALL SCREEN
    var cardSlider = document.querySelector("#CardSlider");
    if (cardSlider) {
        if (window.matchMedia("(min-width: 768px)").matches) {
            initializeCardSlider();
        } else {
            var cardCarousel = new bootstrap.Carousel(cardSlider, {
                interval: 3000,
                ride: "carousel"
            });
        }
    }
});
