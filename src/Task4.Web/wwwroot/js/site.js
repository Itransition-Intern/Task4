// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function getUniqIdValue(element) {
    if (!element) {
        return null;
    }

    return element.dataset.id
        ?? element.value
        ?? element.id
        ?? null;
}