const fullNameInput = document.getElementById('fullNameInput');
const ageInput = document.getElementById('ageInput');
const testimonialInput = document.getElementById('testimonialInput');

// Массивы для хранения навыков
let previousSkills = [];
let obtainedSkills = [];
let avatarFile = null;

let mode = 0;
let authorId = null;

let fullNameChanged = false;
let ageChanged = false;
let testimonialChanged = false;
let photoChanged = false;
let previousSkillsChanged = false;
let obtainedSkillsChanged = false;

function setAuthor(author) {
    if (author) {
        mode = 1;
        authorId = author.id;

        fullNameInput.value = author.fullName;
        ageInput.value = author.age;
        testimonialInput.value = author.testimonial;

        author.previousSkills.forEach(s => {
            previousSkills.push(s);
        });

        author.obtainedSkills.forEach(s => {
            obtainedSkills.push(s);
        });

        renderSkills('previousSkillsList', previousSkills, 'previous');
        renderSkills('obtainedSkillsList', obtainedSkills, 'obtained');

        if (author.photoUrl) {
            setPhoto(author.photoUrl);
        }
        else {
            setPhoto(null);
        }
    }
    else {
        mode = 0;
    }
}

// Загрузка аватара
document.getElementById('avatarInput').addEventListener('change', function (e) {
    const file = e.target.files[0];
    if (file) {
        avatarFile = file;
        const reader = new FileReader();
        reader.onload = function (e) {
            setPhoto(e.target.result);
        };
        reader.readAsDataURL(file);
        photoChanged = true;
    }
});

function setPhoto(src) {
    if (src) {
        document.getElementById('avatarImage').src = src;
        document.getElementById('avatarImage').style.display = 'block';
        document.querySelector('.avatar-placeholder').style.display = 'none';
        document.getElementById('removeAvatarBtn').style.display = 'inline-flex';
        document.querySelector('.avatar-preview').style.borderStyle = 'solid';
        document.querySelector('.avatar-preview').style.borderColor = '#6366f1';
    }
    else {
        document.getElementById('avatarInput').value = '';
        document.getElementById('avatarImage').style.display = 'none';
        document.querySelector('.avatar-placeholder').style.display = 'block';
        document.getElementById('removeAvatarBtn').style.display = 'none';
        document.querySelector('.avatar-preview').style.borderStyle = 'dashed';
        document.querySelector('.avatar-preview').style.borderColor = '#d1d5db';
    }
}

// Удаление аватара
document.getElementById('removeAvatarBtn').addEventListener('click', function () {
    avatarFile = null;
    setPhoto(null);
    photoChanged = true;
});

// Добавление предыдущего навыка
function addPreviousSkill() {
    const input = document.getElementById('previousSkillInput');
    const skill = input.value.trim();

    if (skill && !previousSkills.includes(skill)) {
        previousSkills.push(skill);
        renderSkills('previousSkillsList', previousSkills, 'previous');
        input.value = '';
        previousSkillsChanged = true;
    }
}

// Добавление полученного навыка
function addObtainedSkill() {
    const input = document.getElementById('obtainedSkillInput');
    const skill = input.value.trim();

    if (skill && !obtainedSkills.includes(skill)) {
        obtainedSkills.push(skill);
        renderSkills('obtainedSkillsList', obtainedSkills, 'obtained');
        input.value = '';
        obtainedSkillsChanged = true;
    }
}

// Отрисовка навыков
function renderSkills(listId, skills, type) {
    const list = document.getElementById(listId);
    list.innerHTML = skills.map((skill, index) => `
                <div class="skill-tag">
                    ${skill}
                    <button class="skill-remove" onclick="removeSkill('${type}', ${index})">
                        <svg viewBox="0 0 12 12" fill="none" stroke="currentColor">
                            <path stroke-linecap="round" stroke-width="2" d="M3 3L9 9M9 3L3 9"/>
                        </svg>
                    </button>
                </div>
            `).join('');
}

// Удаление навыка
function removeSkill(type, index) {
    if (type === 'previous') {
        previousSkills.splice(index, 1);
        renderSkills('previousSkillsList', previousSkills, 'previous');
        previousSkillsChanged = true;
    } else {
        obtainedSkills.splice(index, 1);
        renderSkills('obtainedSkillsList', obtainedSkills, 'obtained');
        obtainedSkillsChanged = true;
    }
}

// Добавление навыка по Enter
document.getElementById('previousSkillInput').addEventListener('keypress', function (e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        addPreviousSkill();
    }
});

document.getElementById('obtainedSkillInput').addEventListener('keypress', function (e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        addObtainedSkill();
    }
});

document.getElementById('fullNameInput').addEventListener("change", () => {
    fullNameChanged = true;
})

document.getElementById('ageInput').addEventListener("change", () => {
    ageChanged = true;
})

document.getElementById('testimonialInput').addEventListener("change", () => {
    testimonialChanged = true;
})

// Сохранение автора
document.getElementById('saveBtn').addEventListener('click', async function () {
    const fullName = fullNameInput.value.trim();
    const age = ageInput.value;
    const testimonial = testimonialInput.value.trim();

    // Валидация
    if (!fullName) {
        alert('Пожалуйста, введите полное имя');
        return;
    }
    if (!age || age < 1) {
        alert('Пожалуйста, введите корректный возраст');
        return;
    }

    // Создание объекта с данными автора
    const authorData = {
        fullName: fullName,
        age: parseInt(age),
        testimonial: testimonial,
        previousSkills: previousSkills,
        obtainedSkills: obtainedSkills,
        avatarFile: avatarFile
    };

    console.log('Author Data:', authorData);

    // Здесь должна быть ваша логика сохранения
    // Например, отправка FormData на сервер
    const formData = new FormData();
    formData.append('fullName', authorData.fullName);
    formData.append('age', authorData.age);
    formData.append('testimonial', authorData.testimonial);
    authorData.previousSkills.forEach(e => {
        formData.append('previousSkills',e);
    })
    authorData.obtainedSkills.forEach(e => {
        formData.append('obtainedSkills', e);
    })
    if (avatarFile) {
        formData.append('photo', avatarFile);
    }
    let response = null;
    if (mode === 0) {
        response = await fetch("/Authors/Add", {
            method: "POST",
            body: formData
        });
    }
    else {
        formData.append("authorId", authorId);

        formData.append("fullNameChanged", fullNameChanged);
        formData.append("ageChanged", ageChanged);
        formData.append("testimonialChanged", testimonialChanged);
        formData.append("photoChanged", photoChanged);
        formData.append("previousSkillsChanged", previousSkillsChanged);
        formData.append("obtainedSkillsChanged", obtainedSkillsChanged);

        response = await fetch("/Authors/Update", {
            method: "PUT",
            body: formData
        });
    }

    if (response && response.ok) {
        alert('Автор успешно сохранен!');
    }
    else {
        alert('Не удалось сохранить автора!');
    }
    location.href = '/Authors';
});