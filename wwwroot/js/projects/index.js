//const fullNameInput = document.querySelector("#fullNameInput");
//const ageInput = document.querySelector("#ageInput");
//const testimonialInput = document.querySelector("#testimonialInput");

//const saveBtn = document.querySelector("#saveBtn");

//saveBtn.addEventListener("click", async () => {
//    const formData = new FormData();

//    formData.append("fullName", fullNameInput.value);
//    formData.append("age", ageInput.value);
//    formData.append("testimonial", testimonialInput.value);

//    const response = await fetch("/projects/add", {
//        method: "POST",
//        body: formData
//    });
//});


const searchInput = document.querySelector("#searchInput");
const tableBody = document.querySelector("#tableBody");
const getBtn = document.querySelector("#getBtn");

getBtn.addEventListener("click", () => { updateTable() });

let projects = null;

async function updateTable() {
    const response = await fetch(`/projects/get?search=${searchInput.value}`);

    if (response.ok === false) {
        return;
    }

    const responseBody = await response.json();

    if (!responseBody) {
        return;
    }

    projects = responseBody;
    renderProjects(responseBody);
}

function renderProjects(projects) {
    const tableBody = document.getElementById('tableBody');

    if (projects.length === 0) {
        tableBody.innerHTML = `
                    <tr>
                        <td colspan="7">
                            <div class="empty-state">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                                </svg>
                                <h3>Нет проектов</h3>
                                <p>Добавьте первый проект, чтобы начать работу</p>
                            </div>
                        </td>
                    </tr>
                `;
        return;
    }

    tableBody.innerHTML = projects.map(project => `
                <tr>
                    <td><span class="project-id">#${project.id}</span></td>
                    <td>
                        <span class="project-title">${project.title}</span>
                    </td>
                    <td>
                        <span class="project-author">${project.author.fullName}</span>
                    </td>
                    <td>
                        <span class="project-course">${project.course}</span>
                    </td>
                    <td>
                        <div class="actions-cell">
                            <button class="btn-action btn-edit" onclick="pass('${project.id}')">
                                Перейти
                            </button>
                        </div>
                    </td>
                    <td>
                        <div class="actions-cell">
                            <button class="btn-action btn-edit" onclick="editProject('${project.id}')">
                                Редактировать
                            </button>
                            <button class="btn-action btn-delete" onclick="deleteProject('${project.id}')">
                                Удалить
                            </button>
                        </div>
                    </td>
                </tr>
            `).join('');
}

async function deleteProject(id) {
    if (!confirm("Вы уверены что хотите удалить проект?"))
        return;

    const formData = new FormData();

    formData.append("id", id);

    const response = await fetch("Projects/Delete", {
        method: "Delete",
        body: formData
    });

    updateTable();
}

async function editProject(id) {
    location.href = `Projects/Edit?id=${id}`;
}

const addBtn = document.querySelector("#addBtn");
addBtn.addEventListener("click", () => { window.location.href = "/Projects/Create" })

function pass(id) {
    let project = projects.find(p => p.id == id);
    if (project) {
        window.location.href = project.url;
    }
}

updateTable();