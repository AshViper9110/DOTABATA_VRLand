const mysql = require("mysql2/promise"); // PromiseベースのMySQLクライアント

let pool; // コネクションプールを保持（シングルトン）

// DB接続取得関数
async function getDB() {

  // 既にプールが作成済みなら再利用
  if (pool) return pool;

  // 最大10回までリトライ
  for (let i = 0; i < 10; i++) {
    try {

      // コネクションプール作成
      pool = mysql.createPool({
        host: process.env.DB_HOST,       // DBホスト
        user: process.env.DB_USER,       // ユーザー名
        password: process.env.DB_PASSWORD, // パスワード
        database: process.env.DB_NAME,   // DB名
      });

      // 接続確認（簡易ヘルスチェック）
      await pool.query("SELECT 1");

      console.log("MySQL connected");

      return pool;

    } catch (err) {

      // 接続失敗時はログ出力して待機
      console.log("Waiting for MySQL...");

      // 2秒待ってリトライ
      await new Promise(r => setTimeout(r, 2000));
    }
  }

  // すべて失敗した場合は例外
  throw new Error("DB connection failed");
}

// 外部から利用できるようにエクスポート
module.exports = getDB;