async function loadUsers() {
  const res = await fetch("/api/users/get");
  const users = await res.json();

  const list = document.getElementById("list");
  list.innerHTML = "";

  if (users != null) {
    users.forEach((user) => {
      const li = document.createElement("li");
      li.textContent = user.name;
      list.appendChild(li);
      const li2 = document.createElement("li");
      li2.textContent = user.level;
      list.appendChild(li2);
    });
  }
}

//データの送信のやり方
document.getElementById("form").addEventListener("submit", async (e) => {
  e.preventDefault();

  const name = document.getElementById("name").value;
  const level = document.getElementById("level").value;

  await fetch("/api/user/add", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ name, level }),
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
      li.textContent = user.name;
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
