const express = require("express"); // Webサーバーフレームワーク
const session = require("express-session"); // セッション管理ミドルウェア
const path = require("path"); // パス操作用（Node標準）
const db = require("./db");
const crypto = require("crypto");
const app = express();


// --- ミドルウェア設定 ---

// JSONリクエストボディをパース
app.use(express.json());

// application/x-www-form-urlencoded をパース
app.use(express.urlencoded({ extended: true }));

// publicフォルダを静的配信
app.use(express.static("public"));

// セッション設定
app.use(
  session({
    secret: "secret-key", // セッション署名用キー（本番は環境変数推奨）
    resave: false, // セッション変更がない場合は再保存しない
    saveUninitialized: false, // 未使用セッションは保存しない
    cookie: {
      maxAge: 1000 * 60 * 30, // 有効期限：30分
    },
  }),
);

// --- 認証系 ---

// ログイン処理
app.post("/login", async (req, res) => {
  const { username, password } = req.body;

  try {
    const [rows] = await db.query(
        "SELECT * FROM admin_users WHERE name = ?",
        [username]
    );

    // ユーザーが存在しない
    if (rows.length === 0) {
      return res.status(401).json({ success: false });
    }

    const user = rows[0];

    // 入力されたパスワードをSHA-256でハッシュ化
    const hashedPassword = crypto
        .createHash("sha256")
        .update(password)
        .digest("hex");

    // ハッシュ値を比較
    if (user.password !== hashedPassword) {
      return res.status(401).json({ success: false });
    }

    // ログイン成功
    req.session.user = user.name;

    res.json({ success: true });

  } catch (err) {
    console.error(err);
    res.status(500).json({ success: false });
  }
});

// セッションチェックAPI
app.get("/check", (req, res) => {
  // ログイン状態をbooleanで返す
  res.json({ loggedIn: !!req.session.user });
});

// ログアウト処理
app.post("/logout", (req, res) => {
  // セッション破棄
  req.session.destroy(() => {
    res.json({ success: true });
  });
});

// --- ページアクセス制御 ---

app.get("/main", (req, res) => {
  // 未ログインならログイン画面へリダイレクト
  if (!req.session.user) {
    return res.redirect("/index.html");
  }

  // ログイン済みならメイン画面を返す
  res.sendFile(path.join(__dirname, "../public/main.html"));
});

// --- API（ユーザー操作） ---

/**
 * 一覧取得
 */
app.get("/api/users/get", async (req, res) => {
  try {
    // 全ユーザー取得
    const [rows] = await db.query("SELECT * FROM users");

    res.json(rows);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});


/**
 * 管理ユーザー一覧取得
 */
app.get("/api/admin-users/get", async (req, res) => {
  try {
    const [rows] = await db.query(`SELECT id,name,created_at FROM admin_users ORDER BY id `)
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: err.message });
  }
});


/**
 * ID検索
 */
app.get("/api/user/search/id", async (req, res) => {
  try {
    // クエリパラメータからID取得
    const value = req.query.value;

    // プレースホルダでSQLインジェクション対策
    const [rows] = await db.query("SELECT * FROM users WHERE id = ?", [value]);

    res.json(rows);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

/**
 * ユーザー追加
 */
app.post("/api/user/add", async (req, res) => {
  try {
    const { name } = req.body;

    // バリデーション（最低限）
    if (!name) {
      return res.status(400).json({ error: "name and level required" });
    }

    // INSERT処理
    await db.query("INSERT INTO users (name) VALUES (?)", [name]);

    res.json({ success: true });
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

/**
 * ミニゲーム一覧取得
 */
app.get("/api/minigames/get", async (req, res) => {
  try {
    const [rows] = await db.query(`
      SELECT
        id,
        name,
        type,
        playable,
        updated_at
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
 * 管理ユーザー詳細
 */
app.get("/api/admin-user/detail", async (req, res) => {
  try {
    const { id } = req.query;

    const [rows] = await db.query(
        `SELECT id, name, password, created_at, updated_at
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
 * 管理ユーザー登録
 */
app.post("/api/admin-users/add", async (req, res) => {
  try {
    const { name, password } = req.body;

    if (!name || !password) {
      return res.status(400).json({
        message: "名前とパスワードを入力してください。"
      });
    }

    // 同じ名前が存在するか確認
    const [rows] = await db.query(
        "SELECT id FROM admin_users WHERE name = ?",
        [name]
    );

    if (rows.length > 0) {
      return res.status(409).json({
        message: "その管理ユーザーは既に存在します。"
      });
    }

    // SHA-256でハッシュ化
    const hashedPassword = crypto
        .createHash("sha256")
        .update(password)
        .digest("hex");

    await db.query(
        "INSERT INTO admin_users (name, password) VALUES (?, ?)",
        [name, hashedPassword]
    );

    res.json({
      success: true,
      message: "管理ユーザーを作成しました。"
    });

  } catch (err) {
    console.error(err);
    res.status(500).json({
      message: "サーバーエラー"
    });
  }
});

// サーバー起動
app.listen(3000, () => {
  console.log("Server started");
});
