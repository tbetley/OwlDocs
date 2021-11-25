let textarea = document.getElementById("editTextarea");
textarea.style.minHeight = "200px";
textarea.style.overflow = "hidden";

// Show/Hide textarea for editing
document.getElementById("toggleEdit").addEventListener("click", function (e) {
    document.getElementById("content").classList.toggle("d-none");
    document.getElementById("edit").classList.toggle("d-none");
    document.getElementById("saveEdit").classList.toggle("d-none");

    document.getElementById("documentName").classList.toggle("d-none");
    document.getElementById("documentNameDiv").classList.toggle("d-none");

    if (this.textContent == "Edit") {
        this.textContent = "Cancel";
        textarea.style.height = (textarea.scrollHeight) + "px";
    }
    else
        this.textContent = "Edit";
})

// Adjust textarea height based on content 
textarea?.addEventListener("input", function (e) {
    console.log("Adjusting textarea size and scrolling");

    // save scrollLeft and scrollTop to prevent screen jump on resize
    let scrollLeft = window.pageXOffset || (document.documentElement || document.parentNode || document.body).scrollLeft;
    let scrollTop = window.pageYOffset || (document.documentElement || document.parentNode || document.body).scrollTop;

    //this.style.height = "";
    this.style.height = (this.scrollHeight) + "px";
    

    console.log(`ScrollLeft ${scrollLeft}; ScrollTop ${scrollTop}`);
    window.scrollTo(scrollLeft, scrollTop);
})

// CTRL+S for saving while in textarea
textarea?.addEventListener('keydown', async function (e) {
    console.log(e);
    if ((e.key == "Tab")) {
        e.preventDefault();

        let val = this.value;
        let start = this.selectionStart;
        let end = this.selectionEnd;

        this.value = val.substring(0, start) + '\t' + val.substring(end);

        this.selectionStart = start + 1;
        this.selectionEnd = start + 1;
    }
    if ((e.key == 83 || e.keyCode == 83) && e.ctrlKey) {
        e.preventDefault();
        await saveDocument();
    }
})

// CTRL+S for saving in rename input box
document.getElementById("documentNameInput").addEventListener("keydown", async function (e) {
    if ((e.key == 83 || e.keyCode == 83) && e.ctrlKey) {
        e.preventDefault();
        await saveDocument();
    }
})

// Save handler
document.getElementById("saveEdit").addEventListener("click", async function (e) {
    await saveDocument();
})



// Send updated document to server and reload the page to get updated html
const saveDocument = async function () {
    document.getElementById("messageDiv").innerHTML = "";

    const editContent = document.getElementById("edit");
    const nameInput = document.getElementById("documentNameInput");

    let data = {};
    data.Id = editContent.getAttribute("data-id");
    data.Markdown = textarea.value;
    data.Name = nameInput.value.trim();
    data.Path = editContent.getAttribute("data-path");
    data.Type = Number.parseInt(editContent.getAttribute("data-type"));

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
        let messageText = await result.text();

        alert("danger", messageText);
    }
}