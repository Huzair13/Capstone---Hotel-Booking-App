document.addEventListener('DOMContentLoaded', function () {

    //INPUTS
    const userName = document.getElementById('RegisterUserName');
    const userEmail = document.getElementById('RegisterEmail');
    const userMobile = document.getElementById('RegisterMobileNum');
    const userDOB = document.getElementById('RegisterDOB');
    const gender = document.getElementById('gender');
    const userPass1 = document.getElementById('registerPass1');
    const userPass2 = document.getElementById('registerPass2');

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

    //FORM VALIDATION ---- ON SUBMIT
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', function (event) {
            const txtPass = userPass1.value.trim();
            const txtPass2 = userPass2.value.trim();

            // showLoadingModal();
            event.preventDefault();

            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            } else {
                event.preventDefault();

                if (txtPass !== txtPass2) {
                    hideLoadingModal();
                    alert("PassWord Not Matched");
                    return;
                }
                Register();
            }

            form.classList.add('was-validated');
        });
    });

    var Register = () => {
        const txtUserName = userName.value.trim();
        const txtEmail = userEmail.value.trim();
        const txtMobile = userMobile.value.trim();
        const txtDOB = userDOB.value;
        const txtPass = userPass1.value.trim();
        const txtPass2 = userPass2.value.trim();
        const txtGender = gender.value;

        const jsonInput = {
            name: txtUserName,
            email: txtEmail,
            mobileNumber: txtMobile,
            dateOfBirth: txtDOB,
            password: txtPass,
            gender : parseInt(txtGender),
            userType: "Guest"
        };

        fetch('https://huzairhotelbookingapi.azure-api.net/Register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(jsonInput)
        })
            .then(res => {
                if (!res.ok) {
                    throw new Error('Network response was not ok');
                }
                return res.json();
            })
            .then(data => {
                hideLoadingModal();
                setTimeout(() => {
                    const userID = data.id;
                    document.getElementById('userID').textContent = userID;
                    $('#userIDModal').modal('show');
                }, 1500);

            })
            .catch(error => {
                hideLoadingModal();
                console.error('Error:', error);
            });
    };


    //REDIRECT TO LOGIN AFTER REGISTER
    $('#userIDModal').on('hidden.bs.modal', function () {
        window.location.href = '/Login/Login.html';
    });

    //VALIDATION ON ENTERING THE INPUT
    const inputs = document.querySelectorAll('input, select');
    inputs.forEach(input => {
        input.addEventListener('input', function () {
            validateInput(input);
        });
    });

    //VALIDATE THE INPUT
    function validateInput(input) {
        if (input.checkValidity()) {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        } else {
            input.classList.remove('is-valid');
            input.classList.add('is-invalid');
        }
    }
});
