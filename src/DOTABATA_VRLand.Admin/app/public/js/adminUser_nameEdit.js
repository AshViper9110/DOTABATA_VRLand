const params = new URLSearchParams(location.search);
const id = params.get("id");

// 対象ユーザーを編集する権限があるか確認し、なければ一覧へ戻す
async function checkEditPermission(targetId) {
    const meRes = await fetch("/check", { cache: "no-store" });
    const me = await meRes.json();

    const isSelf = String(me.userId) === String(targetId);

    if (isSelf) {
        return true;
    }

    const detailRes = await fetch(`/api/admin-user/detail?id=${targetId}`);
    if (!detailRes.ok) {
        alert("ユーザー情報の取得に失敗しました");
        location.href = "/adminUsers.html";
        return false;
    }

    const target = await detailRes.json();
    const myLevel = me.canManageAdminUsers ?? 0;

    if (myLevel <= target.can_manage_admin_users) {
        alert("このユーザーを編集する権限がありません");
        location.href = "/adminUsers.html";
        return false;
    }

    return true;
}

async function loadUser() {
    const response = await fetch(`/api/admin-user/detail?id=${id}`);
    const user = await response.json();

    document.getElementById("currentName").textContent = user.name;
}

async function init() {
    const allowed = await checkEditPermission(id);
    if (!allowed) return;

    await loadUser();

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
            location.replace(`/adminUser_detail.html?id=${id}`);
        } else {
            alert(data.message);
        }
    };
}

init();