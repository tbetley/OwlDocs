
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
    console.log("OFF CLICK");
    console.log(e.target);
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
        e.target.id != "newImageInput") {

        console.log("REMOVING FORM")
        document.getElementById("newFileForm")?.remove();
        document.getElementById("newFolderForm")?.remove();
    }


})


// Create new file event listener
document.getElementById("newFile").addEventListener("click", function (e) {
    console.log("NEW FILE CLICK");
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
    typeInput.value = "File";

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
    typeInput.value = "Directory";

    form.appendChild(nameInput);
    form.appendChild(parentIdInput);
    form.appendChild(typeInput);
    form.appendChild(parentPathInput);

    // 
    selectedFolder.appendChild(form);
    nameInput.focus();
})


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

    console.log(e.target);

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

    // set menu delete button values
    document.getElementById("menuPathInput").value = targetElement.getAttribute("data-path");
    document.getElementById("menuTypeInput").value = targetElement.getAttribute("data-type");
    document.getElementById("menuIdInput").value = targetElement.getAttribute("data-id");
    document.getElementById("menuForm").addEventListener("submit", function (e) {
        if (!confirm("Are you sure you want to delete this and all child items?"))
            e.preventDefault();
    })

    // Open menu at click area
    let menu = document.getElementById("menu");
    menu.style.left = `${e.pageX}px`;
    menu.style.top = `${e.pageY}px`;
    menu.style.display = "block";


})