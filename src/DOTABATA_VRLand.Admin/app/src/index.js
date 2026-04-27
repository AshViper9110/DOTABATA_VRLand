// src/index.js
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
app.get("/api/users", async (req, res) => {
  const [rows] = await db.query("SELECT * FROM Users");
  res.json(rows);
});

/**
 * API: id指定取得
 */
app.get("/api/user", async (req, res) => {
  const id = req.query.id;
  const [rows] = await db.query("SELECT * FROM Users WHERE id = ?", [id]);
  res.json(rows);
});

/**
 * API: 追加
 */
app.post("/api/users", async (req, res) => {
  const { name } = req.body;
  await db.query("INSERT INTO Users (name) VALUES (?)", [name]);
  res.json({ success: true });
});

app.listen(3000, () => {
  console.log("Server started");
});
