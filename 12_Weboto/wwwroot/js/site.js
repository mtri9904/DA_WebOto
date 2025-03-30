document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchInput");
    const searchDropdown = document.getElementById("searchDropdown");

    // Hiển thị lịch sử tìm kiếm khi click vào ô tìm kiếm
    searchInput.addEventListener("focus", showSearchHistory);

    // Ẩn dropdown khi click ra ngoài
    document.addEventListener("click", function (event) {
        if (!searchDropdown.contains(event.target) && event.target !== searchInput) {
            searchDropdown.style.display = "none";
        }
    });

    // Lưu từ khóa vào lịch sử khi nhấn Enter
    searchInput.addEventListener("keypress", function (e) {
        if (e.key === "Enter") {
            const searchTerm = searchInput.value.trim();
            if (searchTerm) {
                saveSearchHistory(searchTerm);
            }
        }
    });

    function showSearchHistory() {
        const historyList = document.getElementById("searchHistory");
        const topSearchesContainer = document.getElementById("topSearches");

        // Hiển thị lịch sử tìm kiếm
        const history = JSON.parse(sessionStorage.getItem("searchHistory")) || [];
        historyList.innerHTML = "";
        if (history.length > 0) {
            history.forEach(search => {
                const li = document.createElement("li");
                li.className = "search-history-item";
                li.textContent = search;
                li.onclick = () => {
                    searchInput.value = search;
                    filterCars();
                    searchDropdown.style.display = "none";
                };
                historyList.appendChild(li);
            });
        } else {
            historyList.innerHTML = "<li class='text-muted'>Không có lịch sử tìm kiếm</li>";
        }

        // Hiển thị tìm kiếm hàng đầu
        topSearchesContainer.innerHTML = "";
        topSearches.forEach(search => {
            const span = document.createElement("span");
            span.className = "top-search-item";
            span.textContent = search;
            span.onclick = () => {
                searchInput.value = search;
                filterCars();
                saveSearchHistory(search);
                searchDropdown.style.display = "none";
            };
            topSearchesContainer.appendChild(span);
        });

        searchDropdown.style.display = "block";
    }

    function saveSearchHistory(searchTerm) {
        if (!searchTerm.trim()) return;
        let history = JSON.parse(sessionStorage.getItem("searchHistory")) || [];
        if (!history.includes(searchTerm)) {
            history.push(searchTerm);
            sessionStorage.setItem("searchHistory", JSON.stringify(history));
        }
    }

    function filterCars() {
        let input = searchInput.value.toLowerCase();
        let cars = document.querySelectorAll(".car-item");

        cars.forEach(car => {
            let carName = car.getAttribute("data-name");
            let brandName = car.getAttribute("data-brand");

            if (carName.includes(input) || (brandName && brandName.includes(input))) {
                car.style.display = "block";
            } else {
                car.style.display = "none";
            }
        });
    }
});
