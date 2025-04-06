// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
async function searchAndCompare() {
    const searchQuery = document.getElementById('searchQuery').value;

    const response = await fetch(`/Car/Search?query=${encodeURIComponent(searchQuery)}`);
    const data = await response.json();

    if (data.success) {
        const cars = data.cars;
        displayCarsForComparison(cars);
    } else {
        alert('Không tìm thấy xe phù hợp.');
    }
}
