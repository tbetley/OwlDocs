

// Event listener for "off click", removes selected folder highlighting
document.addEventListener("click", function (e) {
    console.log(e.target);
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
    if (selectedFolder) {
        parentId = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-id");        
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

    let typeInput = document.createElement("input");
    typeInput.type = "hidden";
    typeInput.name = "Type";
    typeInput.value = "File";

    form.appendChild(nameInput);
    form.appendChild(parentIdInput);
    form.appendChild(typeInput);

    // 
    selectedFolder.appendChild(form);
    nameInput.focus();
})


// Create new folder
document.getElementById("newFolder").addEventListener("click", function (e) {
    // get selected folder or root
    let selectedFolder = document.getElementsByClassName("selected")[0];
    console.log(selectedFolder);

    let parentId = 1;
    if (selectedFolder) {
        parentId = selectedFolder.getElementsByClassName("directory-name")[0].getAttribute("data-id");
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

    let typeInput = document.createElement("input");
    typeInput.type = "hidden";
    typeInput.name = "Type";
    typeInput.value = "Directory";

    form.appendChild(nameInput);
    form.appendChild(parentIdInput);
    form.appendChild(typeInput);

    // 
    selectedFolder.appendChild(form);
    nameInput.focus();
})