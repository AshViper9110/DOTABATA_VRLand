// ログアウト処理
function logout() {
  // /logout エンドポイントにPOST送信（セッション破棄想定）
  fetch("/logout", { method: "POST" })
    // 完了後にログイン画面へ遷移
    .then(() => (location.href = "/main.html"));
}

// ユーザー一覧を取得して表示
async function loadUsers() {
  const res = await fetch("/api/users/get");
  const users = await res.json();

  const list = document.getElementById("list");

  list.innerHTML = "";

  if (users != null) {
    users.forEach((user) => {
      const tr = document.createElement("tr");

      tr.innerHTML = `
        <td>${user.id}</td>
        <td>${user.name}</td>
        <td>
          <button onclick="detailUser(${user.id})">詳細</button>
          <button onclick="deleteUser(${user.id})">削除</button>
        </td>
      `;

      list.appendChild(tr);
    });
  }
}
// --- ユーザー編集処理 ---
function detailUser(id) {
  console.log(id);
}

// --- ユーザー削除処理 ---
function deleteUser(id) {
  console.log(id);
}
// --- ユーザー登録処理 ---
document.getElementById("form").addEventListener("submit", async (e) => {
  // フォームのデフォルト送信を停止
  e.preventDefault();

  // 入力値取得
  const name = document.getElementById("name").value;

  // APIへPOST送信（JSON形式）
  await fetch("/api/user/add", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ name }),
  });

  // 登録後に一覧を再取得
  loadUsers();
});

// --- ID検索 ---
document.getElementById("idSearch").addEventListener("submit", async (e) => {
  e.preventDefault();

  // 入力されたID取得
  const id = document.getElementById("id").value;

  // クエリパラメータでAPI呼び出し
  const res = await fetch(`/api/user/search/id?value=${id}`);
  const user = await res.json();

  const list = document.getElementById("list");
  list.innerHTML = "";

  // 結果が存在する場合
  if (user && user.length > 0) {
    user.forEach((user) => {
      const li = document.createElement("li");
      li.textContent = `${user.name}`;
      list.appendChild(li);
    });
  } else {
    // 該当データなし
    list.innerHTML = "<li>データなし</li>";
  }
});

// 初回ロード時に一覧取得
loadUsers();
