const id = new URLSearchParams(location.search).get("id");

async function loadDetail() {
    try {
        const res = await fetch(`/api/Minigames/get/${id}`);

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

    } catch (err) {
        console.error(err);
        alert("ミニゲーム情報の取得に失敗しました");
    }
}

document.getElementById("editButton").addEventListener("click", () => {
    location.href = `/minigame_edit.html?id=${id}`;
});

document.getElementById("deleteButton").addEventListener("click", async () => {

    if (!confirm("このミニゲームを削除しますか？")) return;

    const res = await fetch(`/api/Minigames/delete/${id}`, {
        method: "DELETE"
    });

    const result = await res.json();

    if (result.success) {
        alert("削除しました");
        location.href = "/minigame.html";
    } else {
        alert("削除に失敗しました");
    }
});

window.addEventListener("DOMContentLoaded", loadDetail);