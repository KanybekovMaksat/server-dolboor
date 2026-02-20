//const fullNameInput = document.querySelector("#fullNameInput");
//const ageInput = document.querySelector("#ageInput");
//const testimonialInput = document.querySelector("#testimonialInput");

//const saveBtn = document.querySelector("#saveBtn");

//saveBtn.addEventListener("click", async () => {
//    const formData = new FormData();

//    formData.append("fullName", fullNameInput.value);
//    formData.append("age", ageInput.value);
//    formData.append("testimonial", testimonialInput.value);

//    const response = await fetch("/authors/add", {
//        method: "POST",
//        body: formData
//    });
//});


const searchInput = document.querySelector("#searchInput");
const tableBody = document.querySelector("#tableBody");
const getBtn = document.querySelector("#getBtn");

getBtn.addEventListener("click", () => { updateTable() });

async function updateTable() {
    const response = await fetch(`/authors/get?search=${searchInput.value}`);

    if (response.ok === false) {
        return;
    }

    const responseBody = await response.json();

    if (!responseBody) {
        return;
    }

    renderProjects(responseBody);
}

function renderProjects(authors) {
    const tableBody = document.getElementById('tableBody');

    if (authors.length === 0) {
        tableBody.innerHTML = `
                    <tr>
                        <td colspan="7">
                            <div class="empty-state">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                                </svg>
                                <h3>Нет авторов</h3>
                                <p>Добавьте первого автора, чтобы начать работу</p>
                            </div>
                        </td>
                    </tr>
                `;
        return;
    }

    tableBody.innerHTML = authors.map(author => `
                <tr>
                    <td><span class="author-id">#${author.id}</span></td>
                    <td>
                        <div class="author-cell">
                            <img src="${author.photoUrl || '/images/default-avatar.png'}" 
                                 alt="${author.fullName}" 
                                 class="author-avatar">
                            <div class="author-info">
                                <span class="author-name">${author.fullName}</span>
                            </div>
                        </div>
                    </td>
                    <td>
                        <span class="age-badge">${author.age} лет</span>
                    </td>
                    <td>
                        <div class="skills-list">
                            ${author.previousSkills ? author.previousSkills.map(skill =>
                            `<span class="skill-tag">${skill.trim()}</span>`
                        ).join(' ') : '<span style="color: #9ca3af;">—</span>'}
                        </div>
                    </td>
                    <td>
                        <div class="skills-list">
                            ${author.obtainedSkills ? author.obtainedSkills.map(skill =>
                                `<span class="skill-tag obtained">${skill.trim()}</span>`
                            ).join(' ') : '<span style="color: #9ca3af;">—</span>'}
                        </div>
                    </td>
                    <td>
                        <div class="testimonial-cell">
                            <div class="testimonial-preview">
                                ${author.testimonial || '<span style="color: #9ca3af;">Нет отзыва</span>'}
                            </div>
                        </div>
                    </td>
                    <td>
                        <div class="actions-cell">
                            <button class="btn-action btn-edit" onclick="editAuthor('${author.id}')">
                                Редактировать
                            </button>
                            <button class="btn-action btn-delete" onclick="deleteAuthor('${author.id}')">
                                Удалить
                            </button>
                        </div>
                    </td>
                </tr>
            `).join('');
}

async function deleteAuthor(id) {
    if (!confirm("Вы уверены что хотите удалить автора?"))
        return;

    const formData = new FormData();

    formData.append("id", id);

    const response = await fetch("Authors/Delete", {
        method: "Delete",
        body: formData
    });

    updateTable();
}

async function editAuthor(id) {
    location.href = `Authors/Edit?id=${id}`;
}

const addBtn = document.querySelector("#addBtn");
addBtn.addEventListener("click", () => { window.location.href = "/Authors/Create" })

updateTable();