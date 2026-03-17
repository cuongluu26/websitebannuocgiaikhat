document.addEventListener("DOMContentLoaded", function () {

    var password = document.getElementById("password");

    var length = document.getElementById("length");
    var number = document.getElementById("number");
    var special = document.getElementById("special");

    var strength = document.getElementById("strength");

    password.addEventListener("keyup", function () {

        var value = password.value;
        var score = 0;

        // ===== ĐIỀU KIỆN ĐỘ DÀI =====
        if (value.length >= 6 && value.length <= 20) {

            length.classList.remove("invalid");
            length.classList.add("valid");
            length.innerHTML = "✔ 6 - 20 ký tự";

            score++;

        } else {

            length.classList.remove("valid");
            length.classList.add("invalid");
            length.innerHTML = "❌ 6 - 20 ký tự";

        }

        // ===== KIỂM TRA SỐ =====
        if (/\d/.test(value)) {

            number.classList.remove("invalid");
            number.classList.add("valid");
            number.innerHTML = "✔ Có ít nhất 1 số";

            score++;

        } else {

            number.classList.remove("valid");
            number.classList.add("invalid");
            number.innerHTML = "❌ Có ít nhất 1 số";

        }

        // ===== KÝ TỰ ĐẶC BIỆT =====
        if (/[!@#$%^&*]/.test(value)) {

            special.classList.remove("invalid");
            special.classList.add("valid");
            special.innerHTML = "✔ Có ký tự đặc biệt";

            score++;

        } else {

            special.classList.remove("valid");
            special.classList.add("invalid");
            special.innerHTML = "❌ Có ký tự đặc biệt";

        }

        // ===== THANH ĐỘ MẠNH =====

        if (score == 1) {

            strength.style.width = "33%";
            strength.style.background = "red";

        }
        else if (score == 2) {

            strength.style.width = "66%";
            strength.style.background = "orange";

        }
        else if (score == 3) {

            strength.style.width = "100%";
            strength.style.background = "green";

        }
        else {

            strength.style.width = "0%";

        }

    });

});


// ===== HIỆN / ẨN MẬT KHẨU =====

function togglePassword() {

    var pass = document.getElementById("password");

    if (pass.type === "password") {

        pass.type = "text";

    } else {

        pass.type = "password";

    }

}