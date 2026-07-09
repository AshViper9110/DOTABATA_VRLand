const params = new URLSearchParams(location.search);
const id = params.get("id");

async function loadUser() {
    const response = await fetch(`/api/admin-user/detail?id=${id}`);
    const user = await response.json();

    document.getElementById("currentName").textContent = user.name;
}

loadUser();

document.getElementById("saveButton").onclick = async () => {

    const newName = document.getElementById("newName").value;
    const password = document.getElementById("password").value;

    if (!newName || !password) {
        alert("すべて入力してください。");
        return;
    }

    const response = await fetch(`/api/admin-user/name/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            newName,
            password
        })
    });

    const data = await response.json();

    if (response.ok) {
        alert(data.message);
        location.href = `/adminUser_detail.html?id=${id}`;
    } else {
        alert(data.message);
    }
};