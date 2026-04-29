// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    const sidebar = document.getElementById("app-sidebar");
    const toggleBtn = document.getElementById("sidebar-toggle-btn");
    const overlay = document.getElementById("sidebar-overlay");

    if (sidebar && toggleBtn && overlay) {
        toggleBtn.addEventListener("click", () => {
            sidebar.classList.toggle("show");
            overlay.classList.toggle("d-none");
        });

        overlay.addEventListener("click", () => {
            sidebar.classList.remove("show");
            overlay.classList.add("d-none");
        });
    }

    // Auto-scroll sidebar to show the active section
    const activeLink = document.querySelector(".sidebar-link.active");
    if (activeLink) {
        activeLink.scrollIntoView({ block: "center", behavior: "instant" });
    }
});
