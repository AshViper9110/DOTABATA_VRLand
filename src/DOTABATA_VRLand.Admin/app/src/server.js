const express = require("express");
const session = require("express-session");
const path = require("path");
const db = require("./db");
const crypto = require("crypto");
const app = express();

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

function hashPassword(password) {
  return crypto.createHash("sha256").update(password).digest("hex");
}

// セッション設定（staticより前に必要）
app.use(
    session({
      secret: "secret-key",
      resave: false,
      saveUninitialized: false,
      cookie: {
        maxAge: 1000 * 60 * 30,
      },
    }),
);

// ログイン不要なページ(ログインページ自体など)
const publicPages = ["/", "/index.html"];

// HTMLページへのアクセスを一括チェック（staticより前に置くのが重要）
app.use((req, res, next) => {
  const isHtmlRequest = req.path.endsWith(".html") || req.path === "/";

  if (!isHtmlRequest) return next();
  if (publicPages.includes(req.path)) return next();

  // キャッシュさせない（戻るボタンでの表示を防ぐ）
  res.set("Cache-Control", "no-store, no-cache, must-revalidate, private");
  res.set("Pragma", "no-cache");

  if (!req.session.user) {
    return res.redirect("/index.html");
  }

  next();
});
// この後にstaticを置く
app.use(express.static("public"));

// --- 権限チェック用ミドルウェア ---

// ログイン済みかどうかチェック
function requireLogin(req, res, next) {
  if (!req.session.user) {
    return res.status(401).json({ message: "ログインが必要です。" });
  }
  next();
}

// 他の管理ユーザーの情報(名前・パスワード)を編集できるか
function requireManagePermission(req, res, next) {
  if ((req.session.canManageAdminUsers ?? 0) < 1) {
    return res.status(403).json({ message: "編集権限がありません。" });
  }
  next();
}

// 他の管理ユーザーの権限レベル自体を変更できるか
function requireGrantPermission(req, res, next) {
  if ((req.session.canManageAdminUsers ?? 0) < 2) {
    return res.status(403).json({ message: "権限変更の権限がありません。" });
  }
  next();
}

// --- 認証系 ---

app.post("/login", async (req, res) => {
  const { username, password } = req.body;

  try {
    const [rows] = await db.query(
        "SELECT * FROM admin_users WHERE BINARY name = ?",
        [username]
    );

    if (rows.length === 0) {
      return res.status(401).json({ success: false });
    }

    const user = rows[0];
    const hashedPassword = hashPassword(password);

    if (user.password !== hashedPassword) {
      return res.status(401).json({ success: false });
    }

    // ログイン成功
    req.session.user = user.name;
    req.session.userId = user.id;
    req.session.canManageAdminUsers = user.can_manage_admin_users; // 0, 1, 2

    res.json({ success: true });

  } catch (err) {
    console.error(err);
    res.status(500).json({ success: false });
  }
});

app.get("/check", (req, res) => {
  res.json({ loggedIn: !!req.session.user });
});

app.post("/logout", (req, res) => {
  req.session.destroy(() => {
    res.json({ success: true });
  });
});

// --- ページアクセス制御 ---

app.get("/main", (req, res) => {
  if (!req.session.user) {
    return res.redirect("/index.html");
  }
  res.sendFile(path.join(__dirname, "../public/main.html"));
});

// --- API（ユーザー操作） ---

app.get("/api/users/get", requireLogin, async (req, res) => {
  try {
    const [rows] = await db.query("SELECT * FROM users");
    res.json(rows);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

/**
 * 管理ユーザー一覧取得
 */
app.get("/api/admin-users/get", requireLogin, async (req, res) => {
  try {
    const [rows] = await db.query(`SELECT id,name,can_manage_admin_users,created_at FROM admin_users ORDER BY id`);
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: err.message });
  }
});

app.get("/api/user/search/id", requireLogin, async (req, res) => {
  try {
    const value = req.query.value;
    const [rows] = await db.query("SELECT * FROM users WHERE id = ?", [value]);
    res.json(rows);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

app.post("/api/user/add", requireLogin, async (req, res) => {
  try {
    const { name } = req.body;
    if (!name) {
      return res.status(400).json({ error: "name and level required" });
    }
    await db.query("INSERT INTO users (name) VALUES (?)", [name]);
    res.json({ success: true });
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

/**
 * 管理ユーザー削除（要:管理権限）
 */
app.delete("/api/admin-user/delete/:id", requireLogin, requireManagePermission, async (req, res) => {
  try {
    const id = req.params.id;

    const [result] = await db.query(
        "DELETE FROM admin_users WHERE id = ?",
        [id]
    );

    if (result.affectedRows === 0) {
      return res.status(404).json({ message: "管理ユーザーが見つかりません。" });
    }

    res.json({ message: "管理ユーザーを削除しました。" });

  } catch (err) {
    console.error(err);
    res.status(500).json({ message: "サーバーエラー" });
  }
});

// --- ミニゲーム系（変更なし） ---

app.get("/api/minigames/get", requireLogin, async (req, res) => {
  try {
    const [rows] = await db.query(`
      SELECT id, name, type, playable, updated_at
      FROM miniGames
      ORDER BY id
    `);
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: err.message });
  }
});

app.get("/api/Minigames/get/:id", requireLogin, async (req, res) => {
  const { id } = req.params;
  try {
    const [rows] = await db.query(
        `SELECT id, name, rule, type, scene_number, playable, created_at, updated_at
             FROM miniGames
             WHERE id = ?`,
        [id]
    );
    if (rows.length === 0) {
      return res.status(404).json({ success: false, message: "ミニゲームが見つかりません" });
    }
    res.json(rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).json({ success: false, message: "サーバーエラー" });
  }
});

/**
 * 管理ユーザー詳細（要:管理権限。自分自身の場合は許可）
 */
app.get("/api/admin-user/detail", requireLogin, async (req, res) => {
  try {
    const { id } = req.query;
    const isSelf = String(req.session.userId) === String(id);

    if (!isSelf && (req.session.canManageAdminUsers ?? 0) < 1) {
      return res.status(403).json({ error: "閲覧権限がありません" });
    }

    const [rows] = await db.query(
        `SELECT id, name, can_manage_admin_users, created_at, updated_at
       FROM admin_users
       WHERE id = ?`,
        [id]
    );

    if (rows.length === 0) {
      return res.status(404).json({ error: "ユーザーが見つかりません" });
    }

    res.json(rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: err.message });
  }
});

/**
 * 管理ユーザー登録（要:管理権限）
 */
app.post("/api/admin-users/add", requireLogin, requireManagePermission, async (req, res) => {
  try {
    const { name, password } = req.body;

    if (!name || !password) {
      return res.status(400).json({ message: "名前とパスワードを入力してください。" });
    }

    const [rows] = await db.query(
        "SELECT id FROM admin_users WHERE name = ?",
        [name]
    );

    if (rows.length > 0) {
      return res.status(409).json({ message: "その管理ユーザーは既に存在します。" });
    }

    const hashedPassword = hashPassword(password);

    // 新規作成時は can_manage_admin_users = 0 固定（権限付与は別エンドポイントで行う）
    await db.query(
        "INSERT INTO admin_users (name, password, can_manage_admin_users) VALUES (?, ?, 0)",
        [name, hashedPassword]
    );

    res.json({ success: true, message: "管理ユーザーを作成しました。" });

  } catch (err) {
    console.error(err);
    res.status(500).json({ message: "サーバーエラー" });
  }
});

/**
 * 管理ユーザーパスワード変更
 * 自分自身 → 現在のパスワード確認が必要
 * 他人     → requireManagePermission が必要（対象者の現在パスワードは不要）
 */
app.put("/api/admin-user/password/:id", requireLogin, async (req, res) => {
  try {
    const id = req.params.id;
    const { currentPassword, newPassword } = req.body;
    const isSelf = String(req.session.userId) === String(id);

    if (!newPassword) {
      return res.status(400).json({ message: "新しいパスワードを入力してください。" });
    }

    const [rows] = await db.query(
        "SELECT password FROM admin_users WHERE id = ?",
        [id]
    );

    if (rows.length === 0) {
      return res.status(404).json({ message: "管理ユーザーが見つかりません。" });
    }

    if (isSelf) {
      // 自分自身の変更 → 現在のパスワード確認が必須
      if (!currentPassword) {
        return res.status(400).json({ message: "現在のパスワードを入力してください。" });
      }
      const currentHash = hashPassword(currentPassword);
      if (rows[0].password !== currentHash) {
        return res.status(400).json({ message: "現在のパスワードが正しくありません。" });
      }
    } else {
      // 他人の変更 → 管理権限が必須
      if ((req.session.canManageAdminUsers ?? 0) < 1) {
        return res.status(403).json({ message: "編集権限がありません。" });
      }
    }

    const newHash = hashPassword(newPassword);

    await db.query(
        "UPDATE admin_users SET password = ?, updated_at = NOW() WHERE id = ?",
        [newHash, id]
    );

    res.json({ message: "パスワードを変更しました。" });

  } catch (err) {
    console.error(err);
    res.status(500).json({ message: "サーバーエラー" });
  }
});

/**
 * 管理ユーザー名前変更
 * 自分自身 → 現在のパスワード確認が必要
 * 他人     → requireManagePermission が必要（対象者のパスワードは不要）
 */
app.put("/api/admin-user/name/:id", requireLogin, async (req, res) => {
  try {
    const id = req.params.id;
    const { newName, password } = req.body;
    const isSelf = String(req.session.userId) === String(id);

    if (!newName) {
      return res.status(400).json({ message: "入力内容が不足しています。" });
    }

    const [rows] = await db.query(
        "SELECT name, password FROM admin_users WHERE id = ?",
        [id]
    );

    if (rows.length === 0) {
      return res.status(404).json({ message: "管理ユーザーが見つかりません。" });
    }

    if (isSelf) {
      if (!password) {
        return res.status(400).json({ message: "パスワードを入力してください。" });
      }
      const hashedPassword = hashPassword(password);
      if (rows[0].password !== hashedPassword) {
        return res.status(400).json({ message: "パスワードが正しくありません。" });
      }
    } else {
      if ((req.session.canManageAdminUsers ?? 0) < 1) {
        return res.status(403).json({ message: "編集権限がありません。" });
      }
    }

    const [exists] = await db.query(
        "SELECT id FROM admin_users WHERE name = ? AND id <> ?",
        [newName, id]
    );

    if (exists.length > 0) {
      return res.status(409).json({ message: "そのユーザー名は既に使用されています。" });
    }

    await db.query(
        "UPDATE admin_users SET name = ?, updated_at = NOW() WHERE id = ?",
        [newName, id]
    );

    res.json({ message: "ユーザー名を変更しました。" });

  } catch (err) {
    console.error(err);
    res.status(500).json({ message: "サーバーエラー" });
  }
});

/**
 * 管理ユーザー権限レベル変更（新規・要:権限付与権限）
 */
app.put("/api/admin-user/permission/:id", requireLogin, requireGrantPermission, async (req, res) => {
  try {
    const id = req.params.id;
    const { can_manage_admin_users } = req.body;

    if (![0, 1, 2].includes(can_manage_admin_users)) {
      return res.status(400).json({ message: "権限レベルの値が不正です。" });
    }

    const [result] = await db.query(
        "UPDATE admin_users SET can_manage_admin_users = ?, updated_at = NOW() WHERE id = ?",
        [can_manage_admin_users, id]
    );

    if (result.affectedRows === 0) {
      return res.status(404).json({ message: "管理ユーザーが見つかりません。" });
    }

    res.json({ message: "権限を変更しました。" });

  } catch (err) {
    console.error(err);
    res.status(500).json({ message: "サーバーエラー" });
  }
});

app.listen(3000, () => {
  console.log("Server started");
});