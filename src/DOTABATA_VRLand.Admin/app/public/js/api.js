async function loadUsers() {
  const res = await fetch("/api/users");
  const users = await res.json();

  const list = document.getElementById("list");
  list.innerHTML = "";

  users.forEach((user) => {
    const li = document.createElement("li");
    li.textContent = user.name;
    list.appendChild(li);
  });
}

document.getElementById("form").addEventListener("submit", async (e) => {
  e.preventDefault();

  const name = document.getElementById("name").value;

  await fetch("/api/users", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ name }),
  });

  loadUsers();
});

loadUsers();
