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

    // 1. 今まで通りJSONでミニゲーム本体を作成
    const response = await fetch("/api/minigames/add", {
        method: "POST",
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

    // 2. 画像が選択されていれば、作成されたIDに対して画像だけ別送信
    if (iconFile) {
        const iconRes = await fetch(`/api/minigames/icon/${data.id}`, {
            method: "PUT",
            headers: {
                "Content-Type": iconFile.type
            },
            body: iconFile
        });

        if (!iconRes.ok) {
            alert("ミニゲームは作成されましたが、画像の保存に失敗しました。");
            location.href = "/minigame.html";
            return;
        }
    }

    alert(data.message);
    location.href = "/minigame.html";
};