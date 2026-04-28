// ログアウト処理
function logout() {
  // /logout エンドポイントにPOST送信（セッション破棄想定）
  fetch("/logout", { method: "POST" })
    // 完了後にログイン画面へ遷移
    .then(() => (location.href = "/login.html"));
}
