(() => {
    'use strict';
    function parseJwt(token) {
        try {
            return JSON.parse(atob(token.split('.')[1]));
        } catch (e) {
            return {};
        }
    }

    const form = document.querySelector('.needs-validation');
    const userIdInput = document.getElementById('loginUserID');
    const userPassInput = document.getElementById('loginUserPass');

    const loadingModal = new bootstrap.Modal(document.getElementById('loadingModal'));

    // Show loading modal
    function showLoadingModal() {
        const loadingAnimationContainer = document.getElementById('loadingAnimation');
        loadingAnimationContainer.innerHTML = '';


        const animation = bodymovin.loadAnimation({
            container: loadingAnimationContainer,
            renderer: 'svg',
            loop: true,
            autoplay: true,
            path: 'https://lottie.host/c8bd9837-fcdf-4106-8906-b454da03b8b7/9qRpxRP31N.json'
        });

        loadingModal.show();
    }

    // Hide loading modal
    function hideLoadingModal() {
        loadingModal.hide();
    }

    userIdInput.addEventListener('input', function () {
        validateInput(userIdInput);
    });

    userPassInput.addEventListener('input', function () {
        validateInput(userPassInput);
    });

    form.addEventListener('submit', event => {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        } else {
            showLoadingModal();
            event.preventDefault();
            const txtUid = userIdInput.value * 1;
            const txtPass = userPassInput.value;

            fetch('https://huzairhotelbookingapi.azure-api.net/Login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    userId: txtUid,
                    password: txtPass
                })
            })
                .then(res => res.json())
                .then(data => {
                    setTimeout(() => {
                        login(data)
                        hideLoadingModal();
                        // console.log(data);
                        if(data.role === "Admin"){
                            window.location.href ='/Admin/AdminHome/adminHome.html'
                        }
                        else{
                            window.location.href = '/User/UserHome/userHome.html'
                        }
                    }, 1000);
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        }

        form.classList.add('was-validated');
    });

    function login(data){
        localStorage.setItem('token', data.token);
        localStorage.setItem('userID', data.userID);
        localStorage.setItem('role', data.role);
    }

    function validateInput(input) {
        if (!input.checkValidity()) {
            input.classList.add('is-invalid');
            input.classList.remove('is-valid');
        } else {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        }
    }
})();
