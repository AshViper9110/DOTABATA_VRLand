const express = require("express");
const db = require("./db");

const app = express();

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// 静的HTML公開
app.use(express.static("public"));

/**
 * API: 一覧取得
 */
app.get("/api/users/get", async (req, res) => {
  const [rows] = await db.query("SELECT * FROM Users");
  res.json(rows);
});

/**
 * API: id指定取得
 */
app.get("/api/user/search/id", async (req, res) => {
  const value = req.query.value;
  const [rows] = await db.query("SELECT * FROM Users WHERE id = ?", [value]);
  res.json(rows);
});

/**
 * API: level指定取得
 */
app.get("/api/user/search/level", async (req, res) => {
  const value = req.query.value;
  const [rows] = await db.query("SELECT * FROM Users WHERE level = ?", [value]);
  res.json(rows);
});

/**
 * API: user追加
 */
app.post("/api/user/add", async (req, res) => {
  const { name, level } = req.body;
  await db.query("INSERT INTO Users (name, level) VALUES (?, ?)", [
    name,
    level,
  ]);
  res.json({ success: true });
});

app.listen(3000, () => {
  console.log("Server started");
});
