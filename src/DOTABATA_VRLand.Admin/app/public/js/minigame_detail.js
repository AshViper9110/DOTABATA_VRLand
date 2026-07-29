const id = new URLSearchParams(location.search).get("id");

async function loadDetail() {
    try {
        const res = await fetch(`/api/Minigames/get/${id}`, { cache: "no-store" });

        if (!res.ok) {
            throw new Error("取得に失敗しました");
        }

        const game = await res.json();

        document.getElementById("id").textContent = game.id;
        document.getElementById("name").textContent = game.name;
        document.getElementById("rule").textContent = game.rule;
        document.getElementById("type").textContent = game.type;
        document.getElementById("scene_name").textContent = game.scene_name;
        document.getElementById("playable").textContent =
            game.playable ? "〇" : "✕";
        document.getElementById("created_at").textContent = game.created_at;
        document.getElementById("updated_at").textContent = game.updated_at;

        const iconImg = document.getElementById("iconImage");
        iconImg.src = `/api/minigames/icon/${id}?t=${Date.now()}`;
        iconImg.style.display = "block";
        iconImg.onerror = () => { iconImg.style.display = "none"; };

    } catch (err) {
        console.error(err);
        alert("ミニゲーム情報の取得に失敗しました");
    }
}

document.getElementById("editButton").addEventListener("click", () => {
    location.replace(`/minigame_edit.html?id=${id}`);
});

document.getElementById("deleteButton").addEventListener("click", async () => {

    if (!confirm("このミニゲームを削除しますか？")) return;

    const res = await fetch(`/api/Minigames/delete/${id}`, {
        method: "DELETE"
    });

    const result = await res.json();

    if (result.success) {
        alert("削除しました");
        location.replace("/minigame.html");
    } else {
        alert("削除に失敗しました");
    }
});

window.addEventListener("DOMContentLoaded", loadDetail);

// bfcacheから復元された場合、データを取り直す（戻るボタンで古い内容が出るのを防ぐ）
window.addEventListener("pageshow", (event) => {
    if (event.persisted) {
        loadDetail();
    }
});