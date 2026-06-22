const express = require("express"); // Webサーバーフレームワーク
const session = require("express-session"); // セッション管理ミドルウェア
const path = require("path"); // パス操作用（Node標準）
const db = require("./db");
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

// ログイン処理（※現在は固定認証）
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

    // パスワード確認
    if (user.password !== password) {
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

// サーバー起動
app.listen(3000, () => {
  console.log("Server started");
});
