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

// 他の管理ユーザーの情報(名前・パスワード)を編集できるか（レベル1以上）
function requireManagePermission(req, res, next) {
  if ((req.session.canManageAdminUsers ?? 0) < 1) {
    return res.status(403).json({ message: "編集権限がありません。" });
  }
  next();
}

// 他の管理ユーザーの権限レベル自体を変更できるか（レベル2以上）
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

// セッションチェック（ログイン状態 + 自分のID・権限レベルを返す）
app.get("/check", (req, res) => {
  res.json({
    loggedIn: !!req.session.user,
    userId: req.session.userId ?? null,
    canManageAdminUsers: req.session.canManageAdminUsers ?? 0,
  });
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
 * 管理ユーザー削除
 * - 自分自身は削除不可
 * - 自分より権限レベルが高い/同じ相手は削除不可
 */
app.delete("/api/admin-user/delete/:id", requireLogin, requireManagePermission, async (req, res) => {
  try {
    const id = req.params.id;
    const myLevel = req.session.canManageAdminUsers ?? 0;
    const isSelf = String(req.session.userId) === String(id);

    // 自分自身の削除は、ランク2の場合のみ許可
    if (isSelf && myLevel < 2) {
      return res.status(403).json({ message: "自分自身は削除できません。" });
    }

    const [rows] = await db.query(
        "SELECT can_manage_admin_users FROM admin_users WHERE id = ?",
        [id]
    );

    if (rows.length === 0) {
      return res.status(404).json({ message: "管理ユーザーが見つかりません。" });
    }

    const targetLevel = rows[0].can_manage_admin_users;

    // 自分自身でない場合のみ、相手のランクをチェック
    if (!isSelf && myLevel <= targetLevel) {
      return res.status(403).json({ message: "削除権限がありません。" });
    }

    // 管理ユーザーが1人しかいない場合は削除不可
    const [[{ count }]] = await db.query(
        "SELECT COUNT(*) AS count FROM admin_users"
    );

    if (count <= 1) {
      return res.status(403).json({ message: "最後の管理ユーザーは削除できません。" });
    }

    // 削除対象がレベル2の場合、他にレベル2がいなければ削除不可
    if (targetLevel === 2) {
      const [[{ level2Count }]] = await db.query(
          "SELECT COUNT(*) AS level2Count FROM admin_users WHERE can_manage_admin_users = 2"
      );

      if (level2Count <= 1) {
        return res.status(403).json({ message: "最後の最上位権限（レベル2）の管理ユーザーは削除できません。" });
      }
    }

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

/**
 * ミニゲーム一覧取得
 */
app.get("/api/minigames/get", requireLogin, async (req, res) => {
  try {
    const [rows] = await db.query(`
      SELECT id, name, type, scene_name, playable, updated_at
      FROM miniGames
      ORDER BY id
    `);

    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: err.message });
  }
});

/**
 * ミニゲーム詳細取得
 */
app.get("/api/Minigames/get/:id", requireLogin, async (req, res) => {
  const { id } = req.params;
  try {
    const [rows] = await db.query(
        `SELECT
                id,
                name,
                rule,
                type,
                scene_name,
                playable,
                created_at,
                updated_at
             FROM miniGames
             WHERE id = ?`,
        [id]
    );

    if (rows.length === 0) {
      return res.status(404).json({
        success: false,
        message: "ミニゲームが見つかりません"
      });
    }

    res.json(rows[0]);

  } catch (err) {
    console.error(err);
    res.status(500).json({
      success: false,
      message: "サーバーエラー"
    });
  }
});

/**
 * ミニゲーム追加
 */
app.post("/api/minigames/add", requireLogin, async (req, res) => {
  try {
    const { name, rule, type, scene_name, playable } = req.body;

    if (!name || !type || !scene_name) {
      return res.status(400).json({ message: "名前・タイプ・シーン名は必須です。" });
    }

    if (![1, 2].includes(Number(type))) {
      return res.status(400).json({ message: "タイプの値が不正です。" });
    }

    const [exists] = await db.query(
        "SELECT id FROM miniGames WHERE scene_name = ?",
        [scene_name]
    );

    if (exists.length > 0) {
      return res.status(409).json({ message: "そのシーン名は既に使用されています。" });
    }

    const [result] = await db.query(
        `INSERT INTO miniGames (name, rule, type, scene_name, playable)
         VALUES (?, ?, ?, ?, ?)`,
        [name, rule || null, Number(type), scene_name, playable === false ? 0 : 1]
    );

    // 作成したレコードのIDを返す（このあと画像アップロードに使う）
    res.json({ success: true, message: "ミニゲームを作成しました。", id: result.insertId });

  } catch (err) {
    console.error(err);
    res.status(500).json({ message: "サーバーエラー" });
  }
});

/**
 * ミニゲームアイコン画像アップロード（生バイナリを直接受け取る）
 */
app.put(
    "/api/minigames/icon/:id",
    requireLogin,
    express.raw({ type: "image/*", limit: "5mb" }),
    async (req, res) => {
      try {
        const id = req.params.id;
        const iconBuffer = req.body;

        if (!iconBuffer || iconBuffer.length === 0) {
          return res.status(400).json({ message: "画像データがありません。" });
        }

        const [result] = await db.query(
            "UPDATE miniGames SET icon = ? WHERE id = ?",
            [iconBuffer, id]
        );

        if (result.affectedRows === 0) {
          return res.status(404).json({ message: "ミニゲームが見つかりません。" });
        }

        res.json({ success: true, message: "画像を保存しました。" });

      } catch (err) {
        console.error(err);
        res.status(500).json({ message: "サーバーエラー" });
      }
    }
);

/**
 * ミニゲームアイコン画像取得
 */
app.get("/api/minigames/icon/:id", requireLogin, async (req, res) => {
  try {
    const id = req.params.id;

    const [rows] = await db.query(
        "SELECT icon FROM miniGames WHERE id = ?",
        [id]
    );

    if (rows.length === 0 || !rows[0].icon) {
      return res.status(404).send();
    }

    res.set("Content-Type", "image/jpeg");
    res.send(rows[0].icon);

  } catch (err) {
    console.error(err);
    res.status(500).send();
  }
});

/**
 * ミニゲーム削除
 */
app.delete("/api/Minigames/delete/:id", requireLogin, async (req, res) => {
  try {
    const id = req.params.id;

    const [result] = await db.query(
        "DELETE FROM miniGames WHERE id = ?",
        [id]
    );

    if (result.affectedRows === 0) {
      return res.status(404).json({ success: false, message: "ミニゲームが見つかりません。" });
    }

    res.json({ success: true, message: "ミニゲームを削除しました。" });

  } catch (err) {
    console.error(err);
    res.status(500).json({ success: false, message: "サーバーエラー" });
  }
});

/**
 * ミニゲーム更新
 */
app.put("/api/minigames/update/:id", requireLogin, async (req, res) => {
  try {
    const id = req.params.id;
    const { name, rule, type, scene_name, playable } = req.body;

    if (!name || !type || !scene_name) {
      return res.status(400).json({ message: "名前・タイプ・シーン名は必須です。" });
    }

    if (![1, 2].includes(Number(type))) {
      return res.status(400).json({ message: "タイプの値が不正です。" });
    }

    const [rows] = await db.query(
        "SELECT id FROM miniGames WHERE id = ?",
        [id]
    );

    if (rows.length === 0) {
      return res.status(404).json({ message: "ミニゲームが見つかりません。" });
    }

    // 同じシーン名が他のレコードで使われていないか確認（自分自身は除外）
    const [exists] = await db.query(
        "SELECT id FROM miniGames WHERE scene_name = ? AND id <> ?",
        [scene_name, id]
    );

    if (exists.length > 0) {
      return res.status(409).json({ message: "そのシーン名は既に使用されています。" });
    }

    await db.query(
        `UPDATE miniGames
         SET name = ?, rule = ?, type = ?, scene_name = ?, playable = ?, updated_at = NOW()
         WHERE id = ?`,
        [
          name,
          rule || null,
          Number(type),
          scene_name,
          playable === false ? 0 : 1,
          id
        ]
    );

    res.json({ success: true, message: "ミニゲームを更新しました。" });

  } catch (err) {
    console.error(err);
    res.status(500).json({ message: "サーバーエラー" });
  }
});

/**
 * 管理ユーザー詳細
 * - 自分自身は常に閲覧可
 * - 他人は requireManagePermission（レベル1以上）が必要
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
 * 管理ユーザー登録（要:管理権限。新規作成時のレベルは常に0固定）
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
 * 他人     → 自分のレベルが相手より高い場合のみ許可（対象者の現在パスワードは不要）
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
        "SELECT password, can_manage_admin_users FROM admin_users WHERE id = ?",
        [id]
    );

    if (rows.length === 0) {
      return res.status(404).json({ message: "管理ユーザーが見つかりません。" });
    }

    if (isSelf) {
      if (!currentPassword) {
        return res.status(400).json({ message: "現在のパスワードを入力してください。" });
      }
      const currentHash = hashPassword(currentPassword);
      if (rows[0].password !== currentHash) {
        return res.status(400).json({ message: "現在のパスワードが正しくありません。" });
      }
    } else {
      const myLevel = req.session.canManageAdminUsers ?? 0;
      const targetLevel = rows[0].can_manage_admin_users;

      if (myLevel <= targetLevel) {
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
 * 他人     → 自分のレベルが相手より高い場合のみ許可（対象者のパスワードは不要）
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
        "SELECT name, password, can_manage_admin_users FROM admin_users WHERE id = ?",
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
      const myLevel = req.session.canManageAdminUsers ?? 0;
      const targetLevel = rows[0].can_manage_admin_users;

      if (myLevel <= targetLevel) {
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
 * 管理ユーザー権限レベル変更
 * - レベル2の管理者のみ実行可能
 * - 対象がレベル2の場合は変更不可（自分自身も含め、レベル2同士は対象外）
 */
app.put("/api/admin-user/permission/:id", requireLogin, requireGrantPermission, async (req, res) => {
  try {
    const id = req.params.id;
    const { can_manage_admin_users } = req.body;

    if (![0, 1, 2].includes(can_manage_admin_users)) {
      return res.status(400).json({ message: "権限レベルの値が不正です。" });
    }

    const [rows] = await db.query(
        "SELECT can_manage_admin_users FROM admin_users WHERE id = ?",
        [id]
    );

    if (rows.length === 0) {
      return res.status(404).json({ message: "管理ユーザーが見つかりません。" });
    }

    const targetLevel = rows[0].can_manage_admin_users;

    // 対象がレベル2の場合は変更不可（自分自身も含む）
    if (targetLevel === 2) {
      return res.status(403).json({ message: "レベル2の管理者の権限は変更できません。" });
    }

    await db.query(
        "UPDATE admin_users SET can_manage_admin_users = ?, updated_at = NOW() WHERE id = ?",
        [can_manage_admin_users, id]
    );

    res.json({ message: "権限を変更しました。" });

  } catch (err) {
    console.error(err);
    res.status(500).json({ message: "サーバーエラー" });
  }
});

app.listen(3000, () => {
  console.log("Server started");
});