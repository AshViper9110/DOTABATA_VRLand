// ログイン処理（フォーム送信時に呼び出す想定）
async function login() {
  // 入力フォームからユーザー名を取得
  const username = document.getElementById("username").value;

  // 入力フォームからパスワードを取得
  const password = document.getElementById("password").value;

  // /login APIへPOSTリクエストを送信
  const res = await fetch("/login", {
    method: "POST",
    headers: {
      // JSON形式で送信することを明示
      "Content-Type": "application/json"
    },
    // リクエストボディに認証情報を付与
    body: JSON.stringify({ username, password })
  });

  // ステータスコードが200系なら成功
  if (res.ok) {
    // ログイン成功 → トップページへ遷移
    location.href = "/";
  } else {
    // ログイン失敗 → アラート表示
    alert("ログイン失敗");
  }
}