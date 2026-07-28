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

        // 削除ボタンの表示条件:
// - 自分自身でない場合: myLevel > targetLevel
// - 自分自身の場合: myLevel === 2 のときのみ
        const canDelete = isSelf
            ? myLevel === 2
            : myLevel > user.can_manage_admin_users;

        if (canDelete) {
            deleteBtn.onclick = async () => {
                if (!confirm("この管理ユーザーを削除しますか？")) {
                    return;
                }

                const response = await fetch(`/api/admin-user/delete/${user.id}`, {
                    method: "DELETE"
                });

                if (response.ok) {
                    alert("管理ユーザーを削除しました。");
                    location.href = "/adminUsers.html";
                } else {
                    const data = await response.json().catch(() => ({}));
                    alert(data.message || "削除に失敗しました。");
                }
            };
        } else {
            deleteBtn.style.display = "none";
        }

    } catch (err) {
        console.error(err);
        alert("管理ユーザー情報の取得に失敗しました");
    }
}

window.addEventListener("DOMContentLoaded", loadAdminUser);