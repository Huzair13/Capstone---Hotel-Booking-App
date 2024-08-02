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
    const role = localStorage.getItem('role');
    if(role !== "Admin"){
        window.location.href ="/login/Login.html";
    }
    if (!token) {
        window.location.href = '/Login/Login.html';
    }
    if (token && isTokenExpired(token)) {
        localStorage.removeItem('token');
        window.location.href = '/Login/Login.html';
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

    document.getElementById('manageHotel').addEventListener('click', function () {
        window.location.href = "/Admin/HotelManage/hotelManage.html"
    });

    document.getElementById('manageRequests').addEventListener('click', function () {
        window.location.href = "/Admin/Request/request.html"
    });

    document.getElementById('addHotel').addEventListener('click', function () {
        window.location.href = "/Admin/AddHotel/addHotel.html"
    });

    document.getElementById('manageUser').addEventListener('click', function () {
        window.location.href = "/Admin/ManageUser/manageUser.html"
    });

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
