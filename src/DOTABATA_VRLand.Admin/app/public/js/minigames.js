async function loadMinigames() {
    try {
        const res = await fetch("/api/Minigames/get");
        const games = await res.json();

        const tbody = document.getElementById("minigame-list");
        tbody.innerHTML = "";

        games.forEach(game => {
            const tr = document.createElement("tr");

            const updatedAt = new Date(game.updated_at)
                .toLocaleString("ja-JP");

            const playable = game.playable ? "〇" : "✕";

            tr.innerHTML = `
        <td>${game.id}</td>
        <td>${game.name}</td>
        <td>${game.type}</td>
        <td>${playable}</td>
        <td>${updatedAt}</td>
        <td>
          <button
            class="detail-btn"
            onclick="location.href='/minigame-detail.html?id=${game.id}'">
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