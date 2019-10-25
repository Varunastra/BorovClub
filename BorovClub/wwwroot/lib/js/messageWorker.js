importScripts("/lib/signalr.js");

onmessage = e => {
    const chatConnection = new signalR.HubConnectionBuilder()
        .withUrl("/messageHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    chatConnection.start().then(function () {
        console.log("connected");
    });

    //chatConnection.on("broadcastMessage", (user, message) => {
    //    const disc = document.getElementById("discussion");
    //    const newNode = document.createElement("li");
    //    newNode.innerHTML = `${user}: ${message}`;
    //    disc.appendChild(newNode);
    //});

    postMessage(chatConnection);
};