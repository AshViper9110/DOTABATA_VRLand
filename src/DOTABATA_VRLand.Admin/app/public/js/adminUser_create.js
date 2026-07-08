document.addEventListener("DOMContentLoaded", () => {
    const registerForm = document.getElementById("registerForm");
    const message = document.getElementById("message");

    registerForm.addEventListener("submit", async (event) => {
        event.preventDefault();

        const name = document.getElementById("userName").value.trim();
        const password = document.getElementById("password").value;

        if (name === "" || password === "") {
            message.style.color = "red";
            message.textContent = "名前とパスワードを入力してください。";
            return;
        }

        if (!confirm("この内容で管理ユーザーを作成しますか？")) {
            return;
        }

        try {
            const response = await fetch("/api/admin-users/add", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    name: name,
                    password: password
                })
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.message || "登録に失敗しました。");
            }

            message.style.color = "green";
            message.textContent = "管理ユーザーを作成しました。";

            registerForm.reset();

        } catch (error) {
            message.style.color = "red";
            message.textContent = error.message;
        }
    });
});