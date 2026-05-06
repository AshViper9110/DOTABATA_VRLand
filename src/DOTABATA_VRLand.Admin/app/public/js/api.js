<<<<<<< HEAD:src/DOTABATA_VRLand.Admin/app/public/js/api.js
=======
// ユーザー一覧を取得して表示
>>>>>>> main:src/DOTABATA_VRLand.Admin/app/public/js/users.js
async function loadUsers() {
  const res = await fetch("/api/users/get");
  const users = await res.json();

  const list = document.getElementById("list");
  list.innerHTML = "";

  if (users != null) {
    users.forEach((user) => {
      const li = document.createElement("li");
      li.textContent = `ID:${user.id} ) Name:${user.name} (time:${user.created_at})`;
      list.appendChild(li);
<<<<<<< HEAD:src/DOTABATA_VRLand.Admin/app/public/js/api.js
      const li2 = document.createElement("li");
      li2.textContent = user.level;
      list.appendChild(li2);
=======
>>>>>>> main:src/DOTABATA_VRLand.Admin/app/public/js/users.js
    });
  }
}

//データの送信のやり方
document.getElementById("form").addEventListener("submit", async (e) => {
  e.preventDefault();

  const name = document.getElementById("name").value;
<<<<<<< HEAD:src/DOTABATA_VRLand.Admin/app/public/js/api.js
  const level = document.getElementById("level").value;

=======
  // APIへPOST送信（JSON形式）
>>>>>>> main:src/DOTABATA_VRLand.Admin/app/public/js/users.js
  await fetch("/api/user/add", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ name }),
  });

  loadUsers();
});

//id指定での取得方法
document.getElementById("search").addEventListener("submit", async (e) => {
  e.preventDefault();

  const id = document.getElementById("id").value;

  const res = await fetch(`/api/user/search/id?value=${id}`);
  const user = await res.json();

  const list = document.getElementById("list");
  list.innerHTML = "";

  if (user && user.length > 0) {
    user.forEach((user) => {
      const li = document.createElement("li");
<<<<<<< HEAD:src/DOTABATA_VRLand.Admin/app/public/js/api.js
      li.textContent = user.name;
=======
      li.textContent = `ID:${user.id} ) Name:${user.name} (time:${user.created_at})`;
>>>>>>> main:src/DOTABATA_VRLand.Admin/app/public/js/users.js
      list.appendChild(li);
      const li2 = document.createElement("li");
      li2.textContent = user.level;
      list.appendChild(li2);
    });
  } else {
    list.innerHTML = "<li>データなし</li>";
  }
});

loadUsers();
