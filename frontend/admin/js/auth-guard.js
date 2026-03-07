document.addEventListener("DOMContentLoaded", () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '../index/login.html';
    } else {
        document.documentElement.style.display = 'block';
        document.body.style.display = 'block';
    }
});