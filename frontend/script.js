const API_BASE =
    "http://localhost:5262";

const fileList =
    document.getElementById("fileList");

const fileTitle =
    document.getElementById("fileTitle");

const fileContent =
    document.getElementById("fileContent");

async function loadFiles() {

    const response =
        await fetch(`${API_BASE}/files`);

    const files =
        await response.json();

    fileList.innerHTML = "";

    files.forEach(file => {

        const li =
            document.createElement("li");

        li.textContent = file;

        li.onclick = () =>
            loadFileContent(file);

        fileList.appendChild(li);
    });
}

async function loadFileContent(path) {

    const response =
        await fetch(
            `${API_BASE}/files/content?path=${encodeURIComponent(path)}`
        );

    const data =
        await response.json();

    fileTitle.textContent =
        data.fileName;

    if (data.type === "text") {

        fileContent.textContent =
            data.content;
    }
    else {

        fileContent.textContent =
            data.message;
    }
}

loadFiles();