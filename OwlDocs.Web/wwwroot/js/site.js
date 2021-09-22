

// Event listener for "off click", removes selected folder highlighting
document.addEventListener("click", function (e) {

    let menu = document.getElementById("menu");
    if (menu.style.display == "block")
        menu.style.display = "none";

    if (!e.target.classList.contains("bi-folder2") &&
        !e.target.classList.contains("directory-name") &&
        !e.target.classList.contains("directory"))
    {
        let directory = document.getElementsByClassName("directory selected")[0];

        if (directory)
            directory.classList.remove("selected");
    }

    if (e.target.id != "newFile" &&
        e.target.id != "newFileInput" &&
        e.target.id != "newFolder" &&
        e.target.id != "newFolderInput") {

        console.log("removing form")
        document.getElementById("newFileForm")?.remove();
        document.getElementById("newFolderForm")?.remove();
    }
    
    
})


// Create new file
document.getElementById("newFile").addEventListener("click", function (e) {
    // get selected folder or root
    let selectedFolder = document.getElementsByClassName("directory selected")[0];
    console.log(selectedFolder);

    let parentId = 1;
    let parentPath = "/";
    if (selectedFolder) {
        parentId = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-id");
        parentPath = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-path");
        selectedFolder = selectedFolder.nextElementSibling;
    }
    else {
        selectedFolder = document.getElementsByClassName("sidebar")[0];
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


// Create new folder
document.getElementById("newFolder").addEventListener("click", function (e) {
    // get selected folder or root
    let selectedFolder = document.getElementsByClassName("directory selected")[0];
    console.log(selectedFolder);

    let parentId = 1;
    let parentPath = "/";
    if (selectedFolder) {
        parentId = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-id");
        parentPath = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-path");
        selectedFolder = selectedFolder.nextElementSibling;
    }
    else {
        selectedFolder = document.getElementsByClassName("sidebar")[0];
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

    // Open menu at click area
    let menu = document.getElementById("menu");
    menu.style.left = `${e.pageX}px`;
    menu.style.top = `${e.pageY}px`;
    menu.style.display = "block";
})