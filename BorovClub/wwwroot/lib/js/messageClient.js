let preventScroll = false;

function playAlert() {
    const sound = new Audio("/notification.mp3");
    sound.play();
}

function chatEvents(dotnetHelper) {
    const chat = document.getElementById("chat");
    const messageInput = document.getElementById("msg-input");
    const messageBtn = document.getElementById("msg-btn");

    function setScrollTop(height) {
        preventScroll = true;
        chat.scrollTop = height;
    }

    let pos = chat.scrollHeight;

    setScrollTop(pos);

    chat.addEventListener("scroll", (e) => {
        if (preventScroll) {
            preventScroll = false;
            return;
        }

        if (chat.scrollTop === 0) {
            dotnetHelper.invokeMethodAsync("OnChatUp").then((isEmpty) => {
                if (!isEmpty) {
                    setScrollTop(pos);
                }
            });
        }
    });

    messageInput.addEventListener("keypress", e => {
        if (e.keyCode == '13') {
            dotnetHelper.invokeMethodAsync("SendMessage", messageInput.value).then(() => {
                setScrollTop(chat.scrollHeight);
            });
        }
    });

    messageBtn.addEventListener("click", e => {
        dotnetHelper.invokeMethodAsync("SendMessage", messageInput.value).then(() => {
            setScrollTop(chat.scrollHeight);
        });
    });

    chat.addEventListener("DOMNodeInserted", e => {
        setScrollTop(chat.scrollHeight);
    });
}