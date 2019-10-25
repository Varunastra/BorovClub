function setCookieAuth(token) {
    document.cookie = "token=" + token + "; Path=/;";
    document.location.href = "/";
}

function cookieLogout() {
    document.cookie = "token=; Path=/;"
    document.location.href = "/";
}