// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const filterForm = document.getElementById("questionFilter");
const searchInput = document.getElementById("questionSearch");
const categoryInput = document.getElementById("categoryId");
const questionRows = document.getElementById("questionRows");
const emptyQuestions = document.getElementById("emptyQuestions");

if (filterForm && searchInput && categoryInput && questionRows && emptyQuestions) {
    let timer;

    const showError = (message) => {
        let error = document.getElementById("questionSearchError");
        if (!error) {
            error = document.createElement("div");
            error.id = "questionSearchError";
            error.className = "alert alert-danger";
            filterForm.insertAdjacentElement("afterend", error);
        }
        error.textContent = message;
    };

    const addCell = (row, text) => {
        const cell = document.createElement("td");
        cell.textContent = text;
        row.appendChild(cell);
    };

    const searchQuestions = async () => {
        const query = new URLSearchParams();
        if (searchInput.value.trim()) query.set("term", searchInput.value.trim());
        if (categoryInput.value) query.set("categoryId", categoryInput.value);

        try {
            const response = await fetch(`/Questions/Search?${query.toString()}`);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            const items = await response.json();
            questionRows.replaceChildren();

            for (const item of items) {
                const row = document.createElement("tr");
                addCell(row, item.title);
                addCell(row, item.categoryName);
                addCell(row, item.answerCount.toString());
                addCell(row, new Date(item.createdAt).toLocaleString("tr-TR"));

                const actions = document.createElement("td");
                actions.className = "text-end text-nowrap";
                const update = document.createElement("a");
                update.className = "btn btn-sm btn-outline-primary me-1";
                update.href = `/Questions/Update/${encodeURIComponent(item.id)}`;
                update.textContent = "Düzenle";
                const remove = document.createElement("a");
                remove.className = "btn btn-sm btn-outline-danger";
                remove.href = `/Questions/Delete/${encodeURIComponent(item.id)}`;
                remove.textContent = "Sil";
                actions.append(update, remove);
                row.appendChild(actions);
                questionRows.appendChild(row);
            }

            emptyQuestions.hidden = items.length !== 0;
            document.getElementById("questionSearchError")?.remove();
        } catch (error) {
            console.error("Soru araması başarısız oldu.", error);
            showError("Arama sırasında bir hata oluştu. Lütfen tekrar deneyin.");
        }
    };

    searchInput.addEventListener("input", () => {
        clearTimeout(timer);
        timer = setTimeout(searchQuestions, 250);
    });
    categoryInput.addEventListener("change", searchQuestions);
    filterForm.addEventListener("submit", (event) => {
        event.preventDefault();
        clearTimeout(timer);
        searchQuestions();
    });
}
