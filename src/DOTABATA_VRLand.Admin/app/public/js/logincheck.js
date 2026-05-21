// ログイン状態チェック（ページ読み込み時に実行）
async function checkLogin() {

    // サーバーにログイン状態を問い合わせるAPI
    const res = await fetch("/check");

    // JSONレスポンスを取得（例: { loggedIn: true/false }）
    const data = await res.json();

    // --- タブ単位のセッション制御 ---
    // sessionStorageは「タブ単位」で保持される

    // tabAlive が存在しない場合（= 新しいタブで開かれた）
    if (!sessionStorage.getItem("tabAlive")) {

        // サーバー側セッションを破棄（強制ログアウト）
        fetch("/logout", { method: "POST" });
    }

    // このタブが有効であることを記録
    sessionStorage.setItem("tabAlive", "true");

    // --- ログイン状態チェック ---
    // 未ログインの場合はログイン画面へリダイレクト
    if (!data.loggedIn) {
        location.href = "/login.html";
    }
}

// 初回実行
checkLogin();