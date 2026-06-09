// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function toggleMenu() {

    const menu = document.getElementById("menu");
    const button = document.querySelector(".btnPlus");

    if (menu.style.display === "block") {

        menu.style.display = "none";
        button.classList.remove("active");

    } else {

        menu.style.display = "block";
        button.classList.add("active");
    }
}