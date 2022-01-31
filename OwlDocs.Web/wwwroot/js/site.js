let RENAME_TARGET = null;
const ROOT = 0;
const FILE = 1;
const DIRECTORY = 2;
const IMAGE = 3;

// Event Listenter for opening Folders and Highlighting current selection
let directories = document.getElementsByClassName("directory");
Array.from(directories).forEach(function (element) {
    element.addEventListener("click", function () {
        // clear all selected
        Array.from(directories).forEach(function (dir) {
            dir.classList.remove("selected");
        })

        // select current directory
        element.classList.add("selected");

        // activate child list (show folder items)
        let child = element.nextElementSibling;
        if (child.classList.contains("active"))
            child.classList.remove("active");
        else
            child.classList.add("active");
    })
})


// Event handler for showing images
let showImages = localStorage.getItem("showImages") === "true";
if (showImages) {
    var images = document.getElementsByClassName("image");
    Array.from(images).forEach(function (image) {
        image.classList.remove("image-hidden");
    })
    document.getElementById("showImagesCheckbox").checked = true;
}


// Event handler for checkbox to show images
document.getElementById("showImagesCheckbox").addEventListener("change", function (event) {
    console.log("CHECKED");

    var images = document.getElementsByClassName("image");
    if (this.checked) {
        Array.from(images).forEach(function (image) {
            image.classList.remove("image-hidden");
        })

        localStorage.setItem("showImages", "true");
    }
    else {
        Array.from(images).forEach(function (image) {
            image.classList.add("image-hidden");
        })

        localStorage.setItem("showImages", "false");
    }
})

// Alerting function
function alert(type, message) {
    let messageDiv = document.getElementById("messageDiv");
    messageDiv.innerHTML = "";

    let alertDiv = document.createElement("div");
    alertDiv.classList.add("alert", `alert-${type}`, "alert-dismissible");
    alertDiv.textContent = message;

    let close = document.createElement("button");
    close.type = "button";
    close.classList.add("btn-close");
    close.setAttribute("data-bs-dismiss", "alert");

    alertDiv.appendChild(close);

    messageDiv.appendChild(alertDiv);
}

// drag start functions
function directoryDragStart(event) {

    let path, id, type, name;
    if (event.target.classList.contains("directory-name")) {
        id = event.target.getAttribute("data-id");
        path = event.target.getAttribute("data-path");
        type = event.target.getAttribute("data-type");
        name = event.target.getAttribute("data-name");
    }
    else if (event.target.classList.contains("directory-icon")) {
        id = event.target.nextElementSibling.getAttribute("data-id");
        path = event.target.nextElementSibling.getAttribute("data-path");
        type = event.target.nextElementSibling.getAttribute("data-type");
        name = event.target.nextElementSibling.getAttribute("data-name");
    }
    else if (event.target.classList.contains("directory")) {
        id = event.target.children[1].getAttribute("data-id");
        path = event.target.children[1].getAttribute("data-path");
        type = event.target.children[1].getAttribute("data-type");
        name = event.target.children[1].getAttribute("data-name");
    }

    event.dataTransfer.setData("data/id", id);
    event.dataTransfer.setData("data/path", path);
    event.dataTransfer.setData("data/type", type);
    event.dataTransfer.setData("data/name", name);
}

// Set file data for transfer to folder
function fileDragStart(event) {

    let path, id, type, name;
    if (event.target.classList.contains("file")) {
        path = event.target.firstElementChild.getAttribute("data-path");
        id = event.target.firstElementChild.getAttribute("data-id");
        type = event.target.firstElementChild.getAttribute("data-type");
        name = event.target.firstElementChild.getAttribute("data-name");
    }
    else if (event.target.classList.contains("file-name")) {
        path = event.target.getAttribute("data-path");
        id = event.target.getAttribute("data-id");
        type = event.target.getAttribute("data-type");
        name = event.target.getAttribute("data-name");
    }   

    event.dataTransfer.setData("data/id", id);
    event.dataTransfer.setData("data/path", path);
    event.dataTransfer.setData("data/type", type);
    event.dataTransfer.setData("data/name", name);
}

// set drap effect when item is over a folder
function directoryDragOver(event) {
    event.preventDefault();
    event.dataTransfer.dropEffect = "move";
}

// Handle dropping file/dir into folder
async function directoryDrop(event) {
    event.preventDefault();
    console.log("DROPPED");

    console.log(event);

    // get transfer data
    let data = {};
    data.Path = event.dataTransfer.getData("data/path");
    data.Id = event.dataTransfer.getData("data/id");
    data.Type = Number.parseInt(event.dataTransfer.getData("data/type"));
    data.Name = event.dataTransfer.getData("data/name");

    // get dropped directory id
    if (event.target.classList.contains("directory-name")) {
        data.ParentId = event.target.getAttribute("data-id");
        data.ParentPath = event.target.getAttribute("data-path");
    }
    else if (event.target.classList.contains("directory-icon")) {
        data.ParentId = event.target.nextElementSibling.getAttribute("data-id");
        data.ParentPath = event.target.nextElementSibling.getAttribute("data-path");
    }
    else if (event.target.classList.contains("directory")) {
        data.ParentId = event.target.children[1].getAttribute("data-id");
        data.ParentPath = event.target.children[1].getAttribute("data-path");
    }

    // data to return
    console.log(data);

    // check if item already exists in target folder
    if (data.Path.substring(0, data.Path.lastIndexOf("/")) == data.ParentPath ||
        data.Path == data.ParentPath)
        return;

    // submit data
    if (confirm(`Do you want to move:\n ${data.Path} \nTo:\n ${data.ParentPath}`)) {
        console.log("MOVING ITEM");

        const result = await fetch("/Docs", {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        });

        if (result.ok) {
            location.reload();
        }
        else {
            let error = await result.text();
            alert(error);
        }       
    }
}

// add draggable element support to files
window.addEventListener("DOMContentLoaded", function () {
    var dragDirectoryElements = document.getElementsByClassName("directory");
    Array.from(dragDirectoryElements).forEach(function (element) {
        element.addEventListener("dragstart", directoryDragStart);
        element.addEventListener("dragover", directoryDragOver);
        element.addEventListener("drop", directoryDrop);
    })

    var dragFileElements = document.getElementsByClassName("file");
    Array.from(dragFileElements).forEach(function (element) {
        element.addEventListener("dragstart", fileDragStart)
    })
})



// Event listener for "off click", removes selected folder highlighting
document.addEventListener("click", function (e) {

    let menu = document.getElementById("menu");
    if (menu.style.display == "block")
        menu.style.display = "none";

    if (!e.target.classList.contains("bi-folder2") &&
        !e.target.classList.contains("directory-name") &&
        !e.target.classList.contains("directory") &&
        e.target.id != "newFile" &&
        e.target.id != "newImage" && 
        e.target.id != "newFolder") {
        let directory = document.getElementsByClassName("directory selected")[0];

        if (directory) {
            console.log("REMOVING SELECTED FROM DIRECTORY");
            directory.classList.remove("selected");
        }
    }

    if (e.target.id != "newFile" &&
        e.target.id != "newFileInput" &&
        e.target.id != "newFolder" &&
        e.target.id != "newFolderInput" &&
        e.target.id != "newImage" &&
        e.target.id != "newImageInput" &&
        e.target.id != "menuRename") {

        document.getElementById("newFileForm")?.remove();
        document.getElementById("newFolderForm")?.remove();

        if (document.getElementById("renameForm")) {
            document.getElementById("renameForm").previousElementSibling.classList.remove("d-none");
            document.getElementById("renameForm").remove();

        }
    }

    let contexts = document.getElementsByClassName("context-selected");
    Array.from(contexts).forEach(function (e) {
        e.classList.remove("context-selected");
    })

})


// Create new file event listener
document.getElementById("newFile").addEventListener("click", function (e) {
    console.log("NEW FILE CLICK");

    // check if new file/folder input already exists
    if (document.getElementById("newFileForm")) {
        return;
    }

    // remove folder input if it exists
    document.getElementById("newFolderForm")?.remove();
    
    // get selected folder or root
    let selectedFolder = document.getElementsByClassName("directory selected")[0];
    console.log(selectedFolder);

    let parentId = 1;
    let parentPath = "/";
    if (selectedFolder) {
        selectedFolder.nextElementSibling.classList.add("active");
        parentId = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-id");
        parentPath = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-path");
        selectedFolder = selectedFolder.nextElementSibling;
    }
    else {
        selectedFolder = document.getElementsByClassName("child-item")[0];
    }

    // create form to submit to create new document
    let form = document.createElement("form");
    form.id = "newFileForm";
    form.method = "post";
    form.action = "/Docs";

    let nameInput = document.createElement("input");
    nameInput.type = "text";
    nameInput.id = "newFileInput";
    nameInput.name = "Name";

    let parentIdInput = document.createElement("input");
    parentIdInput.type = "hidden";
    parentIdInput.name = "ParentId";
    parentIdInput.value = parentId;

    let parentPathInput = document.createElement("input");
    parentPathInput.type = "hidden";
    parentPathInput.name = "ParentPath";
    parentPathInput.value = parentPath;

    let typeInput = document.createElement("input");
    typeInput.type = "hidden";
    typeInput.name = "Type";
    typeInput.value = FILE;

    form.appendChild(nameInput);
    form.appendChild(parentIdInput);
    form.appendChild(typeInput);
    form.appendChild(parentPathInput);

    // 
    selectedFolder.appendChild(form);
    nameInput.focus();
})


// Create new folder event listener
document.getElementById("newFolder").addEventListener("click", function (e) {
    // get selected folder or root
    let selectedFolder = document.getElementsByClassName("directory selected")[0];

    // check if new folder input already exists
    if (document.getElementById("newFolderForm")) {
        return;
    }

    // remove folder input if it exists
    document.getElementById("newFileForm")?.remove();

    let parentId = 1;
    let parentPath = "/";
    if (selectedFolder) {
        selectedFolder.nextElementSibling.classList.add("active");
        parentId = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-id");
        parentPath = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-path");
        selectedFolder = selectedFolder.nextElementSibling;
    }
    else {
        selectedFolder = document.getElementsByClassName("child-item")[0];
    }

    // create form to submit to create new document
    let form = document.createElement("form");
    form.id = "newFolderForm";
    form.method = "post";
    form.action = "/Docs";

    let nameInput = document.createElement("input");
    nameInput.type = "text";
    nameInput.id = "newFolderInput";
    nameInput.name = "Name";

    let parentIdInput = document.createElement("input");
    parentIdInput.type = "hidden";
    parentIdInput.name = "ParentId";
    parentIdInput.value = parentId;

    let parentPathInput = document.createElement("input");
    parentPathInput.type = "hidden";
    parentPathInput.name = "ParentPath";
    parentPathInput.value = parentPath;

    let typeInput = document.createElement("input");
    typeInput.type = "hidden";
    typeInput.name = "Type";
    typeInput.value = DIRECTORY;

    form.appendChild(nameInput);
    form.appendChild(parentIdInput);
    form.appendChild(typeInput);
    form.appendChild(parentPathInput);

    // 
    selectedFolder.appendChild(form);
    nameInput.focus();
})


// Create New Image Listener
document.getElementById("newImage").addEventListener("click", function (e) {
    console.log("NEW IMAGE CLICK");
    // get selected folder or root
    let selectedFolder = document.getElementsByClassName("directory selected")[0];
    console.log(selectedFolder);

    let parentId = 1;
    let parentPath = "/";
    if (selectedFolder) {
        selectedFolder.nextElementSibling.classList.add("active");
        parentId = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-id");
        parentPath = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-path");
        selectedFolder = selectedFolder.nextElementSibling;
    }
    else {
        selectedFolder = document.getElementsByClassName("child-item")[0];
    }

    // open modal
    let modal = bootstrap.Modal.getOrCreateInstance(document.getElementById("imageUploadModal"));
    document.getElementById("fileUploadParentPath").value = parentPath;
    document.getElementById("fileUploadParentId").value = parentId;
    modal.show();    
})


// Context menu event listener
document.getElementById("sidebar-items").addEventListener("contextmenu", function (e) {
    e.preventDefault();

    // remove existing state
    let contexts = document.getElementsByClassName("context-selected");
    Array.from(contexts).forEach(function (e) {
        e.classList.remove("context-selected");
    })

    document.getElementById("menuRename").classList.add("d-none");
    RENAME_TARGET = null;

    // get path, type for selected item
    // get data for file
    let targetElement;
    if (e.target.classList.contains("file-name")) {
        targetElement = e.target;
    }
    else if (e.target.getElementsByClassName("file-name")[0]) {
        targetElement = e.target.getElementsByClassName("file-name")[0];
    }
    // get data for directory
    else if (e.target.classList.contains("directory-name")) {
        targetElement = e.target;
    }
    else if (e.target.getElementsByClassName("directory-name")[0]) {
        targetElement = e.target.getElementsByClassName("directory-name")[0];
    }
    else if (e.target.classList.contains("directory-icon")) {
        targetElement = e.target.nextElementSibling;
    }

    console.log("TARGET:");
    console.log(targetElement);
    targetElement.classList.add("context-selected");

    // set menu delete button values
    document.getElementById("menuPathInput").value = targetElement.getAttribute("data-path");
    document.getElementById("menuTypeInput").value = targetElement.getAttribute("data-type");
    document.getElementById("menuIdInput").value = targetElement.getAttribute("data-id");

    // turn on rename if targer is folder
    if (targetElement.getAttribute("data-type") == DIRECTORY) {

        document.getElementById("menuRename").classList.remove("d-none");

        RENAME_TARGET = targetElement;
    }


    // Open menu at click area
    let menu = document.getElementById("menu");
    menu.style.left = `${e.pageX}px`;
    menu.style.top = `${e.pageY}px`;
    menu.style.display = "block";
})


document.getElementById("menuRename").addEventListener("click", async function (e) {
    console.log("rename event");
    console.log(RENAME_TARGET);

    // create form + add event listener on rename target
    // create form to submit to create new document
    let form = document.createElement("form");
    form.id = "renameForm";
    form.method = "post";
    form.action = "/Docs"; // TODO - get from asp tags
    form.style.display = "inline";

    let nameInput = document.createElement("input");
    nameInput.type = "text";
    nameInput.id = "renameNameInput";
    nameInput.name = "Name";
    nameInput.value = RENAME_TARGET.getAttribute("data-name");

    let typeInput = document.createElement("input");
    typeInput.type = "hidden";
    typeInput.name = "Type";
    typeInput.value = DIRECTORY;

    let pathInput = document.createElement("input");
    pathInput.type = "hidden";
    pathInput.name = "Path";
    pathInput.value = RENAME_TARGET.getAttribute("data-path");

    let idInput = document.createElement("input");
    idInput.type = "hidden";
    idInput.name = "Id";
    idInput.value = parseInt(RENAME_TARGET.getAttribute("data-id"));

    form.appendChild(nameInput);
    form.appendChild(typeInput);
    form.appendChild(pathInput);
    form.appendChild(idInput);

    form.addEventListener("submit", async function (e) {
        e.preventDefault();

        const formData = new FormData(this);
        const entries = formData.entries();
        const data = Object.fromEntries(entries);

        data.Id = parseInt(data.Id);
        data.Type = parseInt(data.Type);

        const result = await fetch("/Docs", {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        });

        console.log(result);
        if (result.ok) {
            location.reload();
        }
        else {
            let error = await result.text();
            alert("danger", error);
        }
    })
    
    RENAME_TARGET.parentElement.appendChild(form);


    nameInput.focus();

    RENAME_TARGET.classList.add("d-none");
})

// Validate submission of context menu delete action
document.getElementById("menuForm").addEventListener("submit", function (e) {
    if (!confirm("Are you sure you want to delete this and all child items?")) {
        console.log("CANCELING DELETE");
        e.preventDefault();
        return;
    }
})

