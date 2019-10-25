let chatConnection;

function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2) return parts.pop().split(";").shift();
}

window.onload = () => {
    if (getCookie("token")) {
        chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/messageHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        chatConnection.start().then(function () {
            console.log("connected");
        });

        chatConnection.on("broadcastMessage", (user, message) => {
            try {
                const disc = document.getElementById("discussion");
                const newNode = document.createElement("li");
                newNode.innerHTML = `${user}: ${message}`;
                disc.appendChild(newNode);
            }
            catch (error) {
                if (error.name === "TypeError") {
                    const notification = new Audio("/notification.mp3");
                    notification.play();
                    const msgContainer = document.createElement("div");
                    msgContainer.classList.add("modal-message");
                    const msgSender = document.createElement("div");
                    msgSender.innerHTML = user;
                    const msgText = document.createElement("div");
                    msgText.innerHTML = message;
                    msgContainer.appendChild(msgSender);
                    msgContainer.appendChild(msgText);
                    document.body.appendChild(msgContainer);
                }
            }

        });
    }
}

function sendMessageToUser(reciever, message) {
  chatConnection
    .invoke("SendMessageToUser", reciever, message)
    .catch(err => console.error(err.toString()));
}
