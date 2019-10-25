const modal = document.querySelector(".loading-container");
function openLoadingModal() {
    modal.style.display = "block";
}
function closeLoadingModal() {
    modal.style.display = "none";
}

//function getUploadData() {
//    const input = document.getElementById("avatar-input");
//    const file = input.files[0];
//    const filename = file.name;
//    DotNet.invokeMethodAsync("BorovClub", "uploadData", file, filename);
//}

function avatarOnChange() {
    const input = document.getElementById("avatar-input");
    input.addEventListener("change", (e) => {
        const formData = new FormData();
        formData.append("file", e.target.files[0])
        fetch("/api/fileupload/uploadAvatar", {
            method: "POST",
            headers: {
                "Accept": "*/*"
            },
            body: formData
        }
        ).then(response => response.text()).then(data => {
            const image = document.getElementById("avatar-image");
            image.src = data;
        });
    });
}