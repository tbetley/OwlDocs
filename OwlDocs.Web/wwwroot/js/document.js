let textarea = document.getElementById("editTextarea");

// Show/Hide textarea for editing
document.getElementById("toggleEdit").addEventListener("click", function (e) {
    document.getElementById("content").classList.toggle("d-none");
    document.getElementById("edit").classList.toggle("d-none");
    document.getElementById("saveEdit").classList.toggle("d-none");

    if (this.textContent == "Edit") {
        this.textContent = "Cancel";
        textarea.style.height = "";
        textarea.style.height = (textarea.scrollHeight + 100) + "px";
    }
    else
        this.textContent = "Edit";
})

// Adjust textarea height based on content 
textarea.addEventListener("input", function (e) {
    this.style.height = "";
    this.style.height = (this.scrollHeight + 100) + "px";
})

// CTRL+S for saving while in textarea
textarea.addEventListener('keydown', async function (e) {
    if ((e.key == 83 || e.keyCode == 83) && e.ctrlKey) {
        event.preventDefault();
        await saveDocument();
    }
})

// Save handler
document.getElementById("saveEdit").addEventListener("click", async function (e) {
    await saveDocument();
})

// Send updated document to server and reload the page to get updated html
const saveDocument = async function () {
    const editContent = document.getElementById("edit");

    let data = {};
    data.Id = editContent.getAttribute("data-id");
    data.Markdown = textarea.value;
    data.Name = editContent.getAttribute("data-name");
    data.Path = editContent.getAttribute("data-path");
    console.log(data);

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
        alert(result.statusText);
    }
}