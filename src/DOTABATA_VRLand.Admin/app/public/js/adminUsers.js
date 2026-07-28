let myLevel = 0;
let myId = null;

async function loadMyInfo() {
  const res = await fetch("/check", { cache: "no-store" });
  const data = await res.json();
  myLevel = data.canManageAdminUsers ?? 0;
  myId = data.userId ?? null;
}

async function loadAdminUsers() {
  try {
    const res = await fetch("/api/admin-users/get");
    const users = await res.json();

    const tbody = document.getElementById("admin-user-list");
    tbody.innerHTML = "";

    users.forEach(user => {
      const tr = document.createElement("tr");

      const createdAt = new Date(user.created_at)
          .toLocaleString("ja-JP");

      // 自分自身、または自分より権限レベルが高い相手なら詳細（編集）に進める
      const isSelf = String(myId) === String(user.id);
      const canEdit = isSelf || myLevel > user.can_manage_admin_users;

      const detailButton = canEdit
          ? `<button class="detail-btn" onclick="location.href='/adminUser_detail.html?id=${user.id}'">詳細</button>`
          : `<span style="color:#999;">権限なし</span>`;

      tr.innerHTML = `
<td>${user.id}</td>
<td>${user.name}</td>
<td>${createdAt}</td>
<td>${detailButton}</td>
    `;

      tbody.appendChild(tr);
    });

  } catch (err) {
    console.error(err);
    alert("管理ユーザー一覧の取得に失敗しました");
  }
}

window.addEventListener("DOMContentLoaded", async () => {
  await loadMyInfo();
  await loadAdminUsers();
});