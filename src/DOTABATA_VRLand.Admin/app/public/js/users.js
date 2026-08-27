let allUsers = []; // 全件を保持

// ユーザー一覧を取得して表示
async function loadUsers() {
    try {
        const res = await fetch("/api/users/get");
        allUsers = await res.json();
        renderUsers(allUsers);
    } catch (err) {
        console.error(err);
        alert("ユーザー一覧の取得に失敗しました");
    }
}

function renderUsers(users) {
    const list = document.getElementById("list");
    list.innerHTML = "";

    if (users == null || users.length === 0) {
        list.innerHTML = `<tr><td colspan="3">データなし</td></tr>`;
        return;
    }

    users.forEach((user) => {
        const tr = document.createElement("tr");

        tr.innerHTML = `
      <td>${user.id}</td>
      <td>${user.name}</td>
     
    `;

        list.appendChild(tr);
    });
}

// --- ユーザー編集処理 ---
function detailUser(id) {
    console.log(id);
}

// --- ユーザー削除処理 ---
function deleteUser(id) {
    console.log(id);
}

// --- 名前検索(クライアント側フィルタ) ---
function searchUsersByName() {
    const keyword = document.getElementById("nameSearchInput").value.trim().toLowerCase();
    if (!keyword) {
        renderUsers(allUsers);
        return;
    }
    const filtered = allUsers.filter((user) =>
        user.name.toLowerCase().includes(keyword)
    );
    renderUsers(filtered);
}

window.addEventListener("DOMContentLoaded", async () => {
    await loadUsers();

    document.getElementById("nameSearchBtn").addEventListener("click", searchUsersByName);
    document.getElementById("nameSearchInput").addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            searchUsersByName();
        }
    });
});