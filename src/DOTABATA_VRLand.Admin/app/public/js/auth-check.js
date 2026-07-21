// bfcache（戻る/進むボタンでのキャッシュ復元）からの表示を検知してセッションを再チェックする
window.addEventListener("pageshow", async (event) => {
    if (event.persisted) {
        const res = await fetch("/check", { cache: "no-store" });
        const data = await res.json();

        if (!data.loggedIn) {
            location.replace("/index.html");
        }
    }
});