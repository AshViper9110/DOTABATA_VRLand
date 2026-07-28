async function loadMinigames() {
    try {
        const res = await fetch("/api/minigames/get");
        const games = await res.json();

        const tbody = document.getElementById("minigame-list");
        tbody.innerHTML = "";

        games.forEach(game => {
            const tr = document.createElement("tr");

            const updatedAt = new Date(game.updated_at)
                .toLocaleString("ja-JP");

            const playable = game.playable ? "〇" : "✕";
            const typeLabel = game.type === 1 ? "スコア型" : "タイム型";

            tr.innerHTML = `
        <td>${game.id}</td>
        <td>${game.name}</td>
        <td>${typeLabel}</td>
        <td>${game.scene_name}</td>
        <td>${playable}</td>
        <td>${updatedAt}</td>
        <td>
         <button
            class="detail-btn"
            onclick="location.href='/minigame_detail.html?id=${game.id}'">
            詳細
        </button>
        </td>
      `;

            tbody.appendChild(tr);
        });

    } catch (err) {
        console.error(err);
        alert("ミニゲーム一覧の取得に失敗しました");
    }
}

window.addEventListener("DOMContentLoaded", loadMinigames);