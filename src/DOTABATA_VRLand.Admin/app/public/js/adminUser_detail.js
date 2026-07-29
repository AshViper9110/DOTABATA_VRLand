async function loadAdminUser() {
    const params = new URLSearchParams(window.location.search);
    const id = params.get("id");

    if (!id) {
        alert("ユーザーIDが指定されていません");
        return;
    }

    try {
        const meRes = await fetch("/check", { cache: "no-store" });
        const me = await meRes.json();
        const myLevel = me.canManageAdminUsers ?? 0;
        const isSelf = String(me.userId) === String(id);

        const res = await fetch(`/api/admin-user/detail?id=${id}`);

        if (!res.ok) {
            throw new Error("ユーザーの取得に失敗しました");
        }

        const user = await res.json();

        const createdAt = new Date(user.created_at).toLocaleString("ja-JP");
        const updatedAt = new Date(user.updated_at).toLocaleString("ja-JP");

        document.getElementById("id").textContent = user.id;
        document.getElementById("name").textContent = user.name;
        document.getElementById("password").textContent = "********";
        document.getElementById("createdAt").textContent = createdAt;
        document.getElementById("updatedAt").textContent = updatedAt;

        const canEdit = isSelf || myLevel > user.can_manage_admin_users;

        const editNameBtn = document.getElementById("editNameButton");
        const editPasswordBtn = document.getElementById("editPasswordButton");
        const deleteBtn = document.getElementById("deleteButton");
        const editPermissionBtn = document.getElementById("editPermissionButton");

        if (canEdit) {
            editNameBtn.onclick = () => {
                location.href = `/adminUser_nameEdit.html?id=${user.id}`;
            };

            editPasswordBtn.onclick = () => {
                location.href = `/adminUser_passwordEdit.html?id=${user.id}`;
            };
        } else {
            editNameBtn.style.display = "none";
            editPasswordBtn.style.display = "none";
        }

        // 削除ボタンは「自分自身でない」かつ「編集権限がある」場合のみ表示
        if (!isSelf && myLevel > user.can_manage_admin_users) {
            deleteBtn.onclick = async () => {
                if (!confirm("この管理ユーザーを削除しますか？")) {
                    return;
                }

                const response = await fetch(`/api/admin-user/delete/${user.id}`, {
                    method: "DELETE"
                });

                const data = await response.json().catch(() => ({}));

                if (response.ok) {
                    alert(data.message);
                    location.replace("/adminUsers.html");
                } else {
                    alert(data.message || "削除に失敗しました。");
                }
            };
        } else {
            deleteBtn.style.display = "none";
        }

        // 権限変更ボタンは「自分がレベル2」かつ「対象がレベル2でない」場合のみ表示
        if (myLevel === 2 && user.can_manage_admin_users !== 2) {
            editPermissionBtn.onclick = () => {
                location.href = `/adminUser_permissionEdit.html?id=${user.id}`;
            };
        } else {
            editPermissionBtn.style.display = "none";
        }

    } catch (err) {
        console.error(err);
        alert("管理ユーザー情報の取得に失敗しました");
    }
}

window.addEventListener("DOMContentLoaded", loadAdminUser);