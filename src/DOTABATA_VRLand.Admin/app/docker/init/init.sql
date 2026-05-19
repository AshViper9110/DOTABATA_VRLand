CREATE DATABASE IF NOT EXISTS DotabataVRLand;
USE DotabataVRLand;

CREATE TABLE IF NOT EXISTS users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE IF NOT EXISTS site_action_logs (
  id INT AUTO_INCREMENT PRIMARY KEY,
  content VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE IF NOT EXISTS server_logs (
  id INT AUTO_INCREMENT PRIMARY KEY,
  content VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
create table if not exists mainGame_logs (
   id int auto_increment primary key ,
   start_time timestamp not null ,
   finish_time timestamp,
   win_user_id int,
   created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
   updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

create table if not exists mainGame_user_logs (
    id int auto_increment primary key ,
    user_ID int,
    mainGame_ID int,
    score int default 0,
    user_rank int,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_ID)        REFERENCES users(id),
    FOREIGN KEY (mainGame_ID)    REFERENCES mainGame_logs(id)
);

CREATE TABLE IF NOT EXISTS miniGame_status (
    id           INT AUTO_INCREMENT PRIMARY KEY,
    name         VARCHAR(255) NOT NULL,
    rule         TEXT,
    type         INT,
    Scene_number INT
);

CREATE TABLE IF NOT EXISTS miniGame_logs (
     id                 INT AUTO_INCREMENT PRIMARY KEY,
     miniGame_status_id INT NOT NULL,
     user_id            INT NOT NULL,
     meinGame_log_ID    INT,
     Score              INT DEFAULT 0,
     user_rank               INT,
     WinOrLose          BOOLEAN,
     created_at         TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
     updated_at         TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

     FOREIGN KEY (miniGame_status_id) REFERENCES miniGame_status(id),
     FOREIGN KEY (user_id)            REFERENCES users(id),
     FOREIGN KEY (meinGame_log_ID)    REFERENCES mainGame_logs(id)
    );

CREATE TABLE IF NOT EXISTS dailyActiveUsers (
    id         INT AUTO_INCREMENT PRIMARY KEY,
    day        DATE NOT NULL UNIQUE,
    user_count  INT NOT NULL DEFAULT 0,
    play_count INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);