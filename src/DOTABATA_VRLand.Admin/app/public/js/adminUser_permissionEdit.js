const params = new URLSearchParams(location.search);
const id = params.get("id");

// このページに入る権限があるか確認（自分がレベル2、かつ対象がレベル2でないこと）
async function checkPermissionEditAccess(targetId) {
    const meRes = await fetch("/check", { cache: "no-store" });
    const me = await meRes.json();
    const myLevel = me.canManageAdminUsers ?? 0;

    if (myLevel !== 2) {
        alert("このページにアクセスする権限がありません");
        location.href = "/adminUsers.html";
        return null;
    }

    const detailRes = await fetch(`/api/admin-user/detail?id=${targetId}`);
    if (!detailRes.ok) {
        alert("ユーザー情報の取得に失敗しました");
        location.href = "/adminUsers.html";
        return null;
    }

    const target = await detailRes.json();

    if (target.can_manage_admin_users === 2) {
        alert("レベル2の管理者の権限は変更できません");
        location.href = `/adminUser_detail.html?id=${targetId}`;
        return null;
    }

    return target;
}

async function init() {
    const target = await checkPermissionEditAccess(id);
    if (!target) return;

    document.getElementById("targetName").textContent = target.name;
    document.getElementById("currentLevel").textContent = target.can_manage_admin_users;
    document.getElementById("newLevel").value = String(target.can_manage_admin_users);

    document.getElementById("saveButton").onclick = async () => {

        const newLevel = Number(document.getElementById("newLevel").value);

        const response = await fetch(`/api/admin-user/permission/${id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                can_manage_admin_users: newLevel
            })
        });

        const data = await response.json();

        if (response.ok) {
            alert(data.message);
            location.replace(`/adminUser_detail.html?id=${id}`);
        } else {
            alert(data.message);
        }
    };
}

init();