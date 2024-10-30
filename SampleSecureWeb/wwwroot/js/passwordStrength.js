document.addEventListener('DOMContentLoaded', () => {
    const passwordInput = document.getElementById('passwordInput');
    const passwordStrength = document.getElementById('passwordStrength');

    passwordInput.addEventListener('input', () => {
        const password = passwordInput.value;
        const strength = calculatePasswordStrength(password);
        displayPasswordStrength(strength);
    });

    function calculatePasswordStrength(password) {
        let strength = 0;
        if (password.length >= 12) strength += 1; // Minimal 12 karakter
        if (/[A-Z]/.test(password)) strength += 1; // Huruf besar
        if (/[a-z]/.test(password)) strength += 1; // Huruf kecil
        if (/[0-9]/.test(password)) strength += 1; // Angka
        if (/[^A-Za-z0-9]/.test(password)) strength += 1; // Karakter khusus
        return strength;
    }

    function displayPasswordStrength(strength) {
        passwordStrength.innerHTML = ''; // Kosongkan isi sebelumnya
        const strengthText = document.createElement('span');
        const strengthBar = document.createElement('div');
        strengthBar.style.height = '5px';
        strengthBar.style.width = `${strength * 20}%`;
        strengthBar.style.backgroundColor = getStrengthColor(strength);
        strengthText.textContent = getStrengthText(strength);
        passwordStrength.appendChild(strengthText);
        passwordStrength.appendChild(strengthBar);
    }

    function getStrengthColor(strength) {
        switch (strength) {
            case 0:
            case 1:
                return 'red'; // Merah untuk lemah
            case 2:
            case 3:
                return 'yellow'; // Kuning untuk sedang
            case 4:
            case 5:
                return 'green'; // Hijau untuk kuat
            default:
                return 'gray'; // Abu-abu jika tidak ada
        }
    }

    function getStrengthText(strength) {
        switch (strength) {
            case 0:
            case 1:
                return 'Kata sandi lemah';
            case 2:
            case 3:
                return 'Kata sandi sedang';
            case 4:
            case 5:
                return 'Kata sandi kuat';
            default:
                return '';
        }
    }
});
