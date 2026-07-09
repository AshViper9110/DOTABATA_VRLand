const params = new URLSearchParams(location.search);
const id = params.get("id");

document.getElementById("saveButton").onclick = async () => {

    const currentPassword = document.getElementById("currentPassword").value;
    const newPassword = document.getElementById("newPassword").value;
    const confirmPassword = document.getElementById("confirmPassword").value;

    if (!currentPassword || !newPassword || !confirmPassword) {
        alert("すべて入力してください。");
        return;
    }

    if (newPassword !== confirmPassword) {
        alert("新しいパスワードが一致しません。");
        return;
    }

    const response = await fetch(`/api/admin-user/password/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            currentPassword,
            newPassword
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