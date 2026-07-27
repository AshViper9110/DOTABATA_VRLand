const params = new URLSearchParams(location.search);
const id = params.get("id");

let isSelfEdit = false;

// 対象ユーザーを編集する権限があるか確認し、なければ一覧へ戻す
async function checkEditPermission(targetId) {
    const meRes = await fetch("/check", { cache: "no-store" });
    const me = await meRes.json();

    const isSelf = String(me.userId) === String(targetId);
    isSelfEdit = isSelf;

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

async function init() {
    const allowed = await checkEditPermission(id);
    if (!allowed) return;

    // 自分以外の編集なら「現在のパスワード」欄は不要なので隠す
    if (!isSelfEdit) {
        const currentPasswordField = document.getElementById("currentPassword");
        currentPasswordField.closest(".form-group").style.display = "none";
    }

    document.getElementById("saveButton").onclick = async () => {

        const currentPassword = document.getElementById("currentPassword").value;
        const newPassword = document.getElementById("newPassword").value;
        const confirmPassword = document.getElementById("confirmPassword").value;

        if (isSelfEdit && !currentPassword) {
            alert("現在のパスワードを入力してください。");
            return;
        }

        if (!newPassword || !confirmPassword) {
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
}

init();