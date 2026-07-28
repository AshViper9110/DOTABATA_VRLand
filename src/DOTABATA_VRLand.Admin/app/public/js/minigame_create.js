document.getElementById("saveButton").onclick = async () => {

    const name = document.getElementById("name").value.trim();
    const rule = document.getElementById("rule").value.trim();
    const type = document.getElementById("type").value;
    const sceneName = document.getElementById("sceneName").value.trim();
    const playable = document.getElementById("playable").checked;

    if (!name || !sceneName) {
        alert("ミニゲーム名とシーン名は入力してください。");
        return;
    }

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

    if (response.ok) {
        alert(data.message);
        location.href = "/minigame.html";
    } else {
        alert(data.message);
    }
};