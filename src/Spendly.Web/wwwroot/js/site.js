// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {

    // ── Modal Teleport ─────────────────────────────────────────────────────────
    // Bootstrap modals and offcanvas must be direct children of <body> to work
    // correctly when their parent has overflow:auto + position:relative (which
    // creates a stacking context that traps position:fixed backdrops).
    document.querySelectorAll(".modal, .offcanvas").forEach(el => {
        if (el.parentElement !== document.body) {
            document.body.appendChild(el);
        }
    });

    // ── Sidebar toggle ─────────────────────────────────────────────────────────
    const sidebar   = document.getElementById("app-sidebar");
    const toggleBtn = document.getElementById("sidebar-toggle-btn");
    const overlay   = document.getElementById("sidebar-overlay");

    if (sidebar && toggleBtn && overlay) {
        toggleBtn.addEventListener("click", () => {
            const isOpen = sidebar.classList.toggle("show");
            if (isOpen) {
                overlay.classList.remove("d-none");
                overlay.style.pointerEvents = "auto";
            } else {
                overlay.classList.add("d-none");
                overlay.style.pointerEvents = "none";
            }
        });

        overlay.addEventListener("click", () => {
            sidebar.classList.remove("show");
            overlay.classList.add("d-none");
            overlay.style.pointerEvents = "none";
        });
    }

    // ── Auto-scroll sidebar to active link ────────────────────────────────────
    const activeLink = document.querySelector(".sidebar-link.active");
    if (activeLink) {
        activeLink.scrollIntoView({ block: "center", behavior: "instant" });
    }
});
