const id = new URLSearchParams(location.search).get("id");

async function loadGame() {
    const res = await fetch(`/api/Minigames/get/${id}`);

    if (!res.ok) {
        alert("ミニゲーム情報の取得に失敗しました");
        location.href = "/minigame.html";
        return;
    }

    const game = await res.json();

    document.getElementById("name").value = game.name;
    document.getElementById("rule").value = game.rule || "";
    document.getElementById("type").value = String(game.type);
    document.getElementById("sceneName").value = game.scene_name;
    document.getElementById("playable").checked = !!game.playable;

    const currentIcon = document.getElementById("currentIcon");
    currentIcon.src = `/api/minigames/icon/${id}?t=${Date.now()}`;
    currentIcon.style.display = "block";
    currentIcon.onerror = () => { currentIcon.style.display = "none"; };
}

// 戻る（キャンセル）ボタン: 詳細ページへ明示的に置き換え遷移
document.getElementById("backButton").onclick = () => {
    location.replace(`/minigame_detail.html?id=${id}`);
};

document.getElementById("saveButton").onclick = async () => {

    const name = document.getElementById("name").value.trim();
    const rule = document.getElementById("rule").value.trim();
    const type = document.getElementById("type").value;
    const sceneName = document.getElementById("sceneName").value.trim();
    const playable = document.getElementById("playable").checked;
    const iconFile = document.getElementById("icon").files[0];

    if (!name || !sceneName) {
        alert("ミニゲーム名とシーン名は入力してください。");
        return;
    }

    const response = await fetch(`/api/minigames/update/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            name,
            rule,
            type: Number(type),
            scene_name: sceneName,
            playable
        })
    });

    const data = await response.json();

    if (!response.ok) {
        alert(data.message);
        return;
    }

    if (iconFile) {
        const iconRes = await fetch(`/api/minigames/icon/${id}`, {
            method: "PUT",
            headers: {
                "Content-Type": iconFile.type
            },
            body: iconFile
        });

        if (!iconRes.ok) {
            alert("ミニゲームは更新されましたが、画像の保存に失敗しました。");
            location.replace(`/minigame_detail.html?id=${id}`);
            return;
        }
    }

    alert(data.message);
    location.replace(`/minigame_detail.html?id=${id}`);
};

window.addEventListener("DOMContentLoaded", loadGame);