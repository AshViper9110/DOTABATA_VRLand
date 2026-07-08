async function loadAdminUser() {
    // URLからIDを取得
    const params = new URLSearchParams(window.location.search);
    const id = params.get("id");

    if (!id) {
        alert("ユーザーIDが指定されていません");
        return;
    }

    try {
        const res = await fetch(`/api/admin-user/detail?id=${id}`);

        if (!res.ok) {
            throw new Error("ユーザーの取得に失敗しました");
        }

        const user = await res.json();

        // 日付を整形
        const createdAt = new Date(user.created_at).toLocaleString("ja-JP");
        const updatedAt = new Date(user.updated_at).toLocaleString("ja-JP");

        // 画面へ表示
        document.getElementById("id").textContent = user.id;
        document.getElementById("name").textContent = user.name;
        document.getElementById("password").textContent = "********";
        document.getElementById("createdAt").textContent = createdAt;
        document.getElementById("updatedAt").textContent = updatedAt;

        // 編集ボタン
        document.getElementById("editNameButton").onclick = () => {
            location.href = `/adminUser_nameEdit.html?id=${user.id}`;
        };

        document.getElementById("editPasswordButton").onclick = () => {
            location.href = `/adminUser_passwordEdit.html?id=${user.id}`;
        };

    } catch (err) {
        console.error(err);
        alert("管理ユーザー情報の取得に失敗しました");
    }
}

// ページ読み込み時
window.addEventListener("DOMContentLoaded", loadAdminUser);