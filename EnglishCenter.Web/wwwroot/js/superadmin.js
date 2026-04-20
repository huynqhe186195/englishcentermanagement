document.addEventListener("DOMContentLoaded", function () {
    const sidebarToggle = document.getElementById("sidebarToggle");
    const sidebar = document.getElementById("saSidebar");

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener("click", function () {
            sidebar.classList.toggle("show");
        });
    }

    const deleteButtons = document.querySelectorAll(".btn-delete");
    deleteButtons.forEach(function (btn) {
        btn.addEventListener("click", function () {
            const itemName = btn.getAttribute("data-name") || "this item";
            const confirmed = confirm(`Are you sure you want to delete ${itemName}?`);
            if (confirmed) {
                alert(`Delete action confirmed for ${itemName}. Connect this to your backend handler.`);
            }
        });
    });

    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll(".sidebar-nav .nav-link").forEach(function (link) {
        const href = (link.getAttribute("href") || "").toLowerCase();
        if (href && currentPath.includes(href)) {
            link.classList.add("active");
        }
    });
});