function setToken(token) {
    localStorage.setItem("token", token);
    document.location.href = "/";
}

function removeToken() {
    if (localStorage.getItem("token")) {
        localStorage.removeItem("token");
    }
    document.location.href = "/";
}