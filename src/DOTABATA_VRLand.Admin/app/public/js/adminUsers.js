async function loadAdminUsers() {
  try {
    const res = await fetch("/api/admin-users/get");
    const users = await res.json();

    const tbody = document.getElementById("admin-user-list");
    tbody.innerHTML = "";

    users.forEach(user => {
      const tr = document.createElement("tr");

      // 日付を見やすく整形
      const createdAt = new Date(user.created_at)
        .toLocaleString("ja-JP");

      tr.innerHTML = `
<td>${user.id}</td>
<td>${user.name}</td>
<td>${createdAt}</td>
<td>
    <button
        class="detail-btn"
        onclick="location.href='/adminUser_detail.html?id=${user.id}'">
        詳細
    </button>
</td>
    `;

      tbody.appendChild(tr);
    });

  } catch (err) {
    console.error(err);
    alert("管理ユーザー一覧の取得に失敗しました");
  }
}

// ページ読み込み時に一覧取得
window.addEventListener("DOMContentLoaded", () => {
  loadAdminUsers();
});

