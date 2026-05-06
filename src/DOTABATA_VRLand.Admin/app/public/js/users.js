// ログアウト処理
function logout() {
  // /logout エンドポイントにPOST送信（セッション破棄想定）
  fetch("/logout", { method: "POST" })
    // 完了後にログイン画面へ遷移
    .then(() => (location.href = "/login.html"));
}

// ユーザー一覧を取得して表示
async function loadUsers() {
  // APIから全ユーザー取得
  const res = await fetch("/api/users/get");
  const users = await res.json();

  // 表示先のul/ol要素を取得
  const list = document.getElementById("list");

  // 一旦リストを初期化
  list.innerHTML = "";

  // データが存在する場合
  if (users != null) {
    users.forEach((user) => {
      // ユーザー名表示用li
      const li = document.createElement("li");
      li.textContent = user.name;
      list.appendChild(li);

      // レベル表示用li
      const li2 = document.createElement("li");
      li2.textContent = user.level;
      list.appendChild(li2);
    });
  }
}

// --- ユーザー登録処理 ---
document.getElementById("form").addEventListener("submit", async (e) => {
  // フォームのデフォルト送信を停止
  e.preventDefault();

  // 入力値取得
  const name = document.getElementById("name").value;
  const level = document.getElementById("level").value;

  // APIへPOST送信（JSON形式）
  await fetch("/api/user/add", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ name, level }),
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
      li.textContent = `${user.name} (Lv.${user.level})`;
      list.appendChild(li);
    });
  } else {
    // 該当データなし
    list.innerHTML = "<li>データなし</li>";
  }
});

// 初回ロード時に一覧取得
loadUsers();
