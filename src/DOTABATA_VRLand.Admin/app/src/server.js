const express = require("express"); // Webサーバーフレームワーク
const db = require("./db");
const app = express();

// --- ミドルウェア設定 ---

// JSONリクエストボディをパース
app.use(express.json());

// application/x-www-form-urlencoded をパース
app.use(express.urlencoded({ extended: true }));

// publicフォルダを静的配信
app.use(express.static("public"));

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
