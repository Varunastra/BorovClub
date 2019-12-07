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