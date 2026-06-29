// ログイン状態チェック（ページ読み込み時に実行）
async function checkLogin() {
    const res = await fetch("/check");
    const data = await res.json();

    if (!data.loggedIn) {
        location.href = "/index.js";
    }
}

// 初回実行
checkLogin();