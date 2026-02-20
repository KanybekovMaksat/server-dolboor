function getVideoThumbnail(file, time = 1) {
    return new Promise((resolve, reject) => {
        const video = document.createElement("video");
        video.preload = "metadata";
        video.muted = true;
        video.playsInline = true;
        video.src = URL.createObjectURL(file);

        video.onloadedmetadata = () => {
            video.currentTime = Math.min(time, video.duration);
        };

        video.onseeked = () => {
            const canvas = document.createElement("canvas");
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;

            const ctx = canvas.getContext("2d");
            ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

            const dataUrl = canvas.toDataURL("image/jpeg", 0.8);

            URL.revokeObjectURL(video.src);
            resolve(dataUrl);
        };

        video.onerror = () => {
            URL.revokeObjectURL(video.src);
            reject(new Error('Ошибка загрузки видео'));
        };
    });
}

function getVideoThumbnailFromUrl(url, time = 1) {
    return new Promise((resolve, reject) => {
        const video = document.createElement("video");
        video.preload = "metadata";
        video.muted = true;
        video.playsInline = true;
        video.crossOrigin = "anonymous"; // важно для canvas
        video.src = url;

        video.onloadedmetadata = () => {
            if (video.duration === Infinity || isNaN(video.duration)) {
                reject(new Error("Не удалось получить длительность видео"));
                return;
            }
            video.currentTime = Math.min(time, video.duration);
        };

        video.onseeked = () => {
            try {
                const canvas = document.createElement("canvas");
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;

                const ctx = canvas.getContext("2d");
                ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

                const dataUrl = canvas.toDataURL("image/jpeg", 0.8);
                resolve(dataUrl);
            } catch {
                reject(new Error("Canvas заблокирован CORS политикой"));
            }
        };

        video.onerror = () => reject(new Error("Ошибка загрузки видео"));
    });
}

function createMediaElement(imgUrl, fileId, altText = "") {
    const wrapper = document.createElement("div");
    wrapper.className = "media-element";
    wrapper.dataset.fileId = fileId; // уникальный ID вместо индекса

    const button = document.createElement("button");
    button.className = "media-remove-button";
    button.type = "button";
    
    // обработчик удаления
    button.addEventListener("click", () => {
        removeFile(fileId);
    });

    const svgNS = "http://www.w3.org/2000/svg";
    const svg = document.createElementNS(svgNS, "svg");
    svg.setAttribute("viewBox", "0 0 12 12");
    svg.setAttribute("fill", "none");
    svg.setAttribute("stroke", "currentColor");

    const path = document.createElementNS(svgNS, "path");
    path.setAttribute("stroke-linecap", "round");
    path.setAttribute("stroke-width", "2");
    path.setAttribute("d", "M3 3L9 9M9 3L3 9");

    svg.appendChild(path);
    button.appendChild(svg);

    const img = document.createElement("img");
    img.src = imgUrl;
    img.alt = altText;

    wrapper.appendChild(button);
    wrapper.appendChild(img);

    return wrapper;
}

function objectToFormData(obj, formData = new FormData(), parentKey = '') {
    for (const key in obj) {
        if (Object.hasOwnProperty.call(obj, key)) {
            const value = obj[key];
            // Build the key in bracket notation if it's a nested property
            const formKey = parentKey ? `${parentKey}[${key}]` : key;

            if (typeof value === 'object' && value !== null && !(value instanceof File)) {
                // Recurse for nested objects or arrays
                objectToFormData(value, formData, formKey);
            } else if (value !== null && value !== undefined) {
                // Append primitive values or File objects
                formData.append(formKey, value);
            }
        }
    }
    return formData;
}

const titleInput = document.querySelector("#titleInput");
const courseInput = document.querySelector("#courseInput");
const urlInput = document.querySelector("#urlInput");
const descriptionInput = document.querySelector("#descriptionInput");

const uploadMediaButton = document.querySelector("#uploadMediaButton");
const uploadMediaInput = document.querySelector("#uploadMediaInput");
const mediaList = document.querySelector("#mediaList");

const authorSelectButton = document.querySelector("#authorSelectButton");
const cancelAuthorsBtn = document.querySelector("#cancelAuthorsBtn");
const authorsSearchInput = document.querySelector("#authorsSearchInput");
const getAuthorsBtn = document.querySelector("#getAuthorsBtn");
const authorsTableBody = document.querySelector("#authorsTableBody");
const authorTableBody = document.querySelector("#authorTableBody");

const uploadProjectInput = document.querySelector("#uploadProjectInput");
const uploadProjectButton = document.querySelector("#uploadProjectButton");
const projectFilesCountIndicator = document.querySelector("#projectFilesCountIndicator");

const overlay = document.querySelector("#overlay");

const projectFiles = [];

let selectedAuthor = null;

let mode = 0;
let projectId;

let isUrlChanged = false;
let isAuthorChanged = false;
let isMediaChanged = false;
let isProjectChanged = false;

let isProjectLoaded = false;

function setProject(project) {
    if (project) {
        mode = 1;
        projectId = project.id;
        titleInput.value = project.title;
        descriptionInput.textContent = project.description;
        urlInput.value = project.url;
        courseInput.value = project.course;

        author = {
            id: project.authorId,
            fullName: project.authorFullName,
            age: project.authorAge,
            photoUrl: project.authorPhotoUrl,
            previousSkills: project.authorPreviousSkills,
            obtainedSkills: project.authorObtainedSkills
        }

        project.medias.forEach(m => {
            addFileUrl(m.url, m.id, m.type == 0 ? 'image' : 'video');
        });

        selectAuthor(author);

        if (project.loadedProjectFilesCount > 0) {
            isProjectLoaded = true;
            uploadProjectButton.textContent = "Отменить";
            projectFilesCountIndicator.textContent = `Загружено: ${project.loadedProjectFilesCount} файлов`;
        }

        isUrlChanged = false;
        isAuthorChanged = false;
        isMediaChanged = false;
        isProjectChanged = false;

        if (project.codeStructure) {
            codeStructure = project.codeStructure;
            renderTree();
        }
    }
    else {
        mode = 0
    }
}

urlInput.addEventListener("change", () => {
    isUrlChanged = true;
});

// Author

function toggleOverlay() {
    if (overlay.style.display != "flex") {
        overlay.style.display = "flex";
    }
    else {
        overlay.style.display = "none";
    }
}

authorSelectButton.addEventListener("click", () => {
    toggleOverlay();
    getAuthors();
});

cancelAuthorsBtn.addEventListener("click", () => {
    toggleOverlay();
});

getAuthorsBtn.addEventListener("click", () => {
    getAuthors();
});

async function getAuthors() {
    const response = await fetch(`/authors/get?search=${authorsSearchInput.value}`);

    if (response.ok === false) {
        return;
    }

    const responseBody = await response.json();

    if (!responseBody) {
        return;
    }

    renderAuthors(responseBody, authorsTableBody);
}

function renderAuthors(authors, tableBody) {
    tableBody.innerHTML = "";

    // Пустое состояние
    if (authors.length === 0) {
        const tr = document.createElement("tr");
        const td = document.createElement("td");
        td.colSpan = 5;

        const emptyDiv = document.createElement("div");
        emptyDiv.className = "empty-state";

        const h3 = document.createElement("h3");
        h3.textContent = "Нет авторов";

        const p = document.createElement("p");
        p.textContent = "Добавьте первого автора, чтобы начать работу";

        emptyDiv.appendChild(h3);
        emptyDiv.appendChild(p);
        td.appendChild(emptyDiv);
        tr.appendChild(td);
        tableBody.appendChild(tr);
        return;
    }

    authors.forEach(author => {
        const tr = document.createElement("tr");

        // --- ID ---
        const tdId = document.createElement("td");
        const idSpan = document.createElement("span");
        idSpan.className = "author-id";
        idSpan.textContent = `#${author.id}`;
        tdId.appendChild(idSpan);

        // --- Автор ---
        const tdAuthor = document.createElement("td");
        const authorCell = document.createElement("div");
        authorCell.className = "author-cell";

        const img = document.createElement("img");
        img.className = "author-avatar";
        img.src = author.photoUrl || "/images/default-avatar.png";
        img.alt = author.fullName;

        const infoDiv = document.createElement("div");
        infoDiv.className = "author-info";

        const nameSpan = document.createElement("span");
        nameSpan.className = "author-name";
        nameSpan.textContent = author.fullName;

        infoDiv.appendChild(nameSpan);
        authorCell.appendChild(img);
        authorCell.appendChild(infoDiv);
        tdAuthor.appendChild(authorCell);

        // --- Возраст ---
        const tdAge = document.createElement("td");
        const ageSpan = document.createElement("span");
        ageSpan.className = "age-badge";
        ageSpan.textContent = `${author.age} лет`;
        tdAge.appendChild(ageSpan);

        // --- Предыдущие навыки ---
        const tdPrevSkills = document.createElement("td");
        const prevSkillsDiv = document.createElement("div");
        prevSkillsDiv.className = "skills-list";

        if (author.previousSkills?.length) {
            author.previousSkills.forEach(skill => {
                const tag = document.createElement("span");
                tag.className = "skill-tag";
                tag.textContent = skill.trim();
                prevSkillsDiv.appendChild(tag);
            });
        } else {
            const empty = document.createElement("span");
            empty.style.color = "#9ca3af";
            empty.textContent = "—";
            prevSkillsDiv.appendChild(empty);
        }
        tdPrevSkills.appendChild(prevSkillsDiv);

        // --- Полученные навыки ---
        const tdObtSkills = document.createElement("td");
        const obtSkillsDiv = document.createElement("div");
        obtSkillsDiv.className = "skills-list";

        if (author.obtainedSkills?.length) {
            author.obtainedSkills.forEach(skill => {
                const tag = document.createElement("span");
                tag.className = "skill-tag obtained";
                tag.textContent = skill.trim();
                obtSkillsDiv.appendChild(tag);
            });
        } else {
            const empty = document.createElement("span");
            empty.style.color = "#9ca3af";
            empty.textContent = "—";
            obtSkillsDiv.appendChild(empty);
        }
        tdObtSkills.appendChild(obtSkillsDiv);

        // Сборка строки
        tr.appendChild(tdId);
        tr.appendChild(tdAuthor);
        tr.appendChild(tdAge);
        tr.appendChild(tdPrevSkills);
        tr.appendChild(tdObtSkills);

        tableBody.appendChild(tr);

        tr.addEventListener("click", () => { selectAuthor(author); toggleOverlay(); });
    });
}

function selectAuthor(author) {
    selectedAuthor = author;
    renderAuthors([author], authorTableBody);
    isAuthorChanged = true;
}
let filesMap = new Map(); // используем Map вместо массива
let fileIdCounter = 0; // счётчик для уникальных ID
const ALLOWED_TYPES = ['image/png', 'image/jpeg', 'image/jpg', 'video/mp4'];

uploadMediaButton.addEventListener("click", () => {
    uploadMediaInput.click();
});

uploadMediaInput.addEventListener("change", async () => {
    if (uploadMediaInput.files && uploadMediaInput.files.length > 0) {
        const newFiles = Array.from(uploadMediaInput.files).filter(f => 
            ALLOWED_TYPES.includes(f.type)
        );
        
        // Добавляем только новые файлы
        for (const file of newFiles) {
            await addFile(file);
        }
        
        uploadMediaInput.value = '';
    }
});

async function addFile(file, id = null) {
    const fileId = fileIdCounter++;
    filesMap.set(fileId, {
        id: id,
        url: null,
        file: file
    });
    
    let thumbnailUrl;
    
    if (file.type.startsWith('image/')) {
        thumbnailUrl = URL.createObjectURL(file);
    } else if (file.type === 'video/mp4') {
        thumbnailUrl = await getVideoThumbnail(file);
    }
    
    const element = createMediaElement(thumbnailUrl, fileId, file.name);
    mediaList.appendChild(element);
    isMediaChanged = true;
}

async function addFileUrl(url, id = null, type = 'image') {
    const fileId = fileIdCounter++;
    filesMap.set(fileId, {
        id: id,
        url: url,
        file: null
    });
    
    let thumbnailUrl;

    if (type == 'image') {
        thumbnailUrl = url;
    } else if (type == 'video') {
        thumbnailUrl = await getVideoThumbnailFromUrl(url);
    }

    const element = createMediaElement(thumbnailUrl, fileId);
    mediaList.appendChild(element);
    isMediaChanged = true;
}

function removeFile(fileId) {
    // Удаляем из Map
    filesMap.delete(fileId);
    
    // Удаляем элемент из DOM
    const element = mediaList.querySelector(`[data-file-id="${fileId}"]`);
    if (element) {
        element.remove();
    }
    isMediaChanged = true;
}

// Если нужно получить список файлов (например, для отправки на сервер)
function getFilesArray() {
    return Array.from(filesMap.values());
} 

uploadProjectButton.addEventListener("click", () => {
    if (projectFiles.length > 0 || isProjectLoaded) {
        projectFiles.length = 0;
        isProjectLoaded = false;
        uploadProjectButton.textContent = "Выбрать папку проекта";
        projectFilesCountIndicator.textContent = ``;
    }
    else {
        uploadProjectInput.click();
    }
    isProjectChanged = true;
});

uploadProjectInput.addEventListener("change", (e) => {
    if (uploadProjectInput.files && uploadProjectInput.files.length > 0) {
        Array.from(uploadProjectInput.files).forEach(f => {
            projectFiles.push(f);
        });
    }
    if (projectFiles.length > 0) {
        uploadProjectButton.textContent = "Отменить";
        projectFilesCountIndicator.textContent = `Загружено: ${projectFiles.length} файлов`;
    }
    e.target.value = "";
});

const saveBtn = document.querySelector("#saveBtn");

saveBtn.addEventListener("click", () => {
    post();
});

async function post() {
    if (!titleInput.value) {
        alert("Введите заголовок проекта!");
        return;
    }
    if (!descriptionInput.value) {
        alert("Введите описание!");
        return;
    }
    if (!urlInput.value) {
        alert("Введите ссылку на проект!");
        return;
    }
    if (!courseInput.value) {
        alert("Введите курс!");
        return;
    }

    if (!selectedAuthor) {
        alert("Выберите автора!");
        return;
    }

    const formData = new FormData()

    formData.append("title", titleInput.value);
    formData.append("description", descriptionInput.value);
    formData.append("url", urlInput.value);
    formData.append("course", courseInput.value);
    formData.append("authorId", selectedAuthor.id);
    formData.append("codeStructure", JSON.stringify(codeStructure));

    let medias = getFilesArray();
    let i = -1
    medias.forEach((media, index) => {
        if (media.file) {
            formData.append("mediaFiles", media.file);
            i++;
        }
        formData.append("mediaFileIndexes", media.file ? i : -1)
        formData.append("mediaUrls", media.url);
        formData.append("mediaIds", media.id);
    });

    projectFiles.forEach(f => {
        formData.append("projectFiles", f);
    });
    let response;
    if (mode == 0) {
        response = await fetch("/projects/add", { method: "POST", body: formData });
    }
    else {
        formData.append("id", projectId);
        formData.append("isUrlChanged", isUrlChanged);
        formData.append("isAuthorChanged", isAuthorChanged);
        formData.append("isMediaChanged", isMediaChanged);
        formData.append("isProjectChanged", isProjectChanged);

        response = await fetch("/projects/update", { method: "PUT", body: formData });
    }

    if (response && response.ok) {
        alert('Проект успешно сохранен!');
    }
    else {
        alert('Не удалось сохранить проект!');
    }
    location.href = '/Projects';
}

// Code Structure Editor
let codeStructure = { folders: [], files: [] };
let selectedFile = null;
let itemIdCounter = 0;

const treeContent = document.querySelector("#treeContent");
const detailsEmpty = document.querySelector("#detailsEmpty");
const detailsForm = document.querySelector("#detailsForm");
const fileNameInput = document.querySelector("#fileNameInput");
const fileLanguageInput = document.querySelector("#fileLanguageInput");
const fileExplanationInput = document.querySelector("#fileExplanationInput");
const fileContentInput = document.querySelector("#fileContentInput");
const saveFileBtn = document.querySelector("#saveFileBtn");
const deleteFileBtn = document.querySelector("#deleteFileBtn");
const addRootFolderBtn = document.querySelector("#addRootFolderBtn");

function generateItemId() {
    return `temp_${itemIdCounter++}`;
}

function createFolder(name, parentId = null) {
    const folder = {
        id: generateItemId(),
        name: name,
        parentId: parentId,
        folders: [],
        files: []
    };

    if (parentId) {
        const parent = findFolder(codeStructure, parentId);
        if (parent) {
            parent.folders.push(folder);
        }
    } else {
        codeStructure.folders.push(folder);
    }

    renderTree();
    return folder;
}

function createFile(name, parentId) {
    const file = {
        id: generateItemId(),
        name: name,
        language: '',
        explanation: '',
        content: '',
        parentId: parentId
    };

    const parent = findFolder(codeStructure, parentId);
    if (parent) {
        parent.files.push(file);
    }

    renderTree();
    selectFile(file.id);
    return file;
}

function findFolder(structure, folderId) {
    for (const folder of structure.folders) {
        if (folder.id === folderId) return folder;
        const found = findFolder(folder, folderId);
        if (found) return found;
    }
    return null;
}

function findFile(structure, fileId) {
    for (const folder of structure.folders) {
        const file = folder.files.find(f => f.id === fileId);
        if (file) return file;
        const found = findFile(folder, fileId);
        if (found) return found;
    }
    return null;
}

function deleteItem(itemId, isFolder) {
    function removeFromParent(structure, id, isFolder) {
        if (isFolder) {
            const index = structure.folders.findIndex(f => f.id === id);
            if (index !== -1) {
                structure.folders.splice(index, 1);
                return true;
            }
        } else {
            const index = structure.files.findIndex(f => f.id === id);
            if (index !== -1) {
                structure.files.splice(index, 1);
                return true;
            }
        }

        for (const folder of structure.folders) {
            if (removeFromParent(folder, id, isFolder)) return true;
        }
        return false;
    }

    removeFromParent(codeStructure, itemId, isFolder);
    renderTree();
    hideDetails();
}

function renderTree() {
    treeContent.innerHTML = '';

    function renderItem(item, isFolder, level = 0) {
        const div = document.createElement('div');

        const itemDiv = document.createElement('div');
        itemDiv.className = 'tree-item';
        itemDiv.style.paddingLeft = `${level * 16 + 8}px`;

        const contentDiv = document.createElement('div');
        contentDiv.className = 'tree-item-content';

        if (isFolder) {
            const chevron = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
            chevron.classList.add('tree-item-chevron');
            chevron.setAttribute('viewBox', '0 0 16 16');
            chevron.setAttribute('fill', 'none');
            const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
            path.setAttribute('d', 'M6 12L10 8L6 4');
            path.setAttribute('stroke', 'currentColor');
            path.setAttribute('stroke-width', '2');
            path.setAttribute('stroke-linecap', 'round');
            chevron.appendChild(path);
            contentDiv.appendChild(chevron);

            const expanded = item.expanded !== false;
            if (expanded) chevron.classList.add('expanded');

            chevron.addEventListener('click', (e) => {
                e.stopPropagation();
                item.expanded = !item.expanded;
                renderTree();
            });
        }

        const icon = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        icon.classList.add('tree-item-icon');
        icon.setAttribute('viewBox', '0 0 16 16');
        icon.setAttribute('fill', 'none');

        if (isFolder) {
            icon.innerHTML = '<path d="M2 4.5C2 3.67 2.67 3 3.5 3H6l1 2h5.5c.83 0 1.5.67 1.5 1.5v6c0 .83-.67 1.5-1.5 1.5h-9C2.67 14 2 13.33 2 12.5v-8z" fill="#6366f1"/>';
        } else {
            icon.innerHTML = '<path d="M4 2h8l2 2v9a1 1 0 01-1 1H3a1 1 0 01-1-1V3a1 1 0 011-1z" fill="#94a3b8" stroke="#64748b" stroke-width="0.5"/>';
        }

        contentDiv.appendChild(icon);

        const name = document.createElement('span');
        name.className = 'tree-item-name';
        name.textContent = item.name;
        contentDiv.appendChild(name);

        itemDiv.appendChild(contentDiv);

        if (isFolder) {
            const actions = document.createElement('div');
            actions.className = 'tree-item-actions';

            const addFolderBtn = document.createElement('button');
            addFolderBtn.type = 'button';
            addFolderBtn.className = 'btn-item-action';
            addFolderBtn.title = 'Создать папку';
            addFolderBtn.innerHTML = '<svg width="12" height="12" viewBox="0 0 16 16" fill="none"><path d="M2 4.5C2 3.67 2.67 3 3.5 3H6l1 2h5.5c.83 0 1.5.67 1.5 1.5v6c0 .83-.67 1.5-1.5 1.5h-9C2.67 14 2 13.33 2 12.5v-8z" fill="currentColor"/></svg>';
            addFolderBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                const folderName = prompt('Название папки:');
                if (folderName) {
                    createFolder(folderName, item.id);
                    item.expanded = true;
                    renderTree();
                }
            });

            const addFileBtn = document.createElement('button');
            addFileBtn.type = 'button';
            addFileBtn.className = 'btn-item-action';
            addFileBtn.title = 'Создать файл';
            addFileBtn.innerHTML = '<svg width="12" height="12" viewBox="0 0 16 16" fill="none"><path d="M4 2h8l2 2v9a1 1 0 01-1 1H3a1 1 0 01-1-1V3a1 1 0 011-1z" fill="currentColor"/></svg>';
            addFileBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                const fileName = prompt('Название файла:');
                if (fileName) {
                    createFile(fileName, item.id);
                    item.expanded = true;
                    renderTree();
                }
            });

            const deleteBtn = document.createElement('button');
            deleteBtn.type = 'button';
            deleteBtn.className = 'btn-item-action';
            deleteBtn.title = 'Удалить';
            deleteBtn.innerHTML = '<svg width="12" height="12" viewBox="0 0 16 16" fill="none"><path d="M3 3L9 9M9 3L3 9" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>';
            deleteBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                if (confirm('Удалить папку и все её содержимое?')) {
                    deleteItem(item.id, true);
                }
            });

            actions.appendChild(addFolderBtn);
            actions.appendChild(addFileBtn);
            actions.appendChild(deleteBtn);
            itemDiv.appendChild(actions);
        } else {
            itemDiv.addEventListener('click', () => selectFile(item.id));
            if (selectedFile && selectedFile.id === item.id) {
                itemDiv.classList.add('selected');
            }
        }

        div.appendChild(itemDiv);

        if (isFolder && item.expanded !== false) {
            const childrenDiv = document.createElement('div');
            childrenDiv.className = 'tree-children';

            item.folders.forEach(folder => {
                childrenDiv.appendChild(renderItem(folder, true, level + 1));
            });

            item.files.forEach(file => {
                childrenDiv.appendChild(renderItem(file, false, level + 1));
            });

            div.appendChild(childrenDiv);
        }

        return div;
    }

    if (codeStructure.folders.length === 0 && codeStructure.files.length === 0) {
        const empty = document.createElement('div');
        empty.style.padding = '20px';
        empty.style.textAlign = 'center';
        empty.style.color = '#9ca3af';
        empty.style.fontSize = '13px';
        empty.textContent = 'Нет файлов. Создайте корневую папку.';
        treeContent.appendChild(empty);
        return;
    }

    codeStructure.folders.forEach(folder => {
        treeContent.appendChild(renderItem(folder, true));
    });
}

function selectFile(fileId) {
    if (selectedFile) {
        saveFile();
    }
    selectedFile = findFile(codeStructure, fileId);
    if (selectedFile) {
        fileNameInput.value = selectedFile.name;
        fileLanguageInput.value = selectedFile.language || '';
        fileExplanationInput.value = selectedFile.explanation || '';
        fileContentInput.value = selectedFile.content || '';

        detailsEmpty.style.display = 'none';
        detailsForm.style.display = 'block';
    }
    renderTree();
}

function hideDetails() {
    selectedFile = null;
    detailsEmpty.style.display = 'flex';
    detailsForm.style.display = 'none';
    renderTree();
}

addRootFolderBtn.addEventListener('click', () => {
    const folderName = prompt('Название корневой папки:');
    if (folderName) {
        createFolder(folderName);
    }
});

saveFileBtn.addEventListener('click', () => {
    saveFile();
});

function saveFile() {
    if (selectedFile) {
        selectedFile.name = fileNameInput.value;
        selectedFile.language = fileLanguageInput.value;
        selectedFile.explanation = fileExplanationInput.value;
        selectedFile.content = fileContentInput.value;
        renderTree();
    }
}

deleteFileBtn.addEventListener('click', () => {
    if (selectedFile && confirm('Удалить файл?')) {
        deleteItem(selectedFile.id, false);
    }
});

// Инициализация
renderTree();