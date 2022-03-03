document.addEventListener("DOMContentLoaded", function() {
    hljs.highlightAll();
});

function setActive(id) {
    let ele = document.querySelector(`#${id}`);
    if (ele) {
        ele.classList.add("active");
    }
}