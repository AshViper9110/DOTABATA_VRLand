// ログアウト処理
 async function logout() {
  const result = confirm("ログアウトしますか？");
  if (!result) {
    return;
  }
  // サーバーにログアウト通知
  await fetch("/logout", {
    method: "POST"
  });

  // ログイン画面へ戻る
  location.href = "/index.html";
}
