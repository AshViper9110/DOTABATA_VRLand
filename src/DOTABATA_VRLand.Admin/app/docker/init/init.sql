-- 1. users
CREATE TABLE `users` (
 `id`          INT NOT NULL AUTO_INCREMENT,
 `name`        VARCHAR(255) NOT NULL,
 `steam_id`    VARCHAR(255) NOT NULL,
 `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 `updated_at`    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
 PRIMARY KEY (`id`),
 UNIQUE KEY uk_users_steam_id (steam_id)
);

-- 2. rooms
CREATE TABLE `rooms` (
 `id`          INT NOT NULL AUTO_INCREMENT,
 `pass`        VARCHAR(255),
 `type`        INT NOT NULL,
 `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 `updated_at`    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
 PRIMARY KEY (`id`)
) ;

-- 3. miniGames
CREATE TABLE `miniGames` (
 `id`           INT NOT NULL AUTO_INCREMENT,
 `name`         VARCHAR(255) NOT NULL,
 `rule`         TEXT,
 `type`         INT NOT NULL,
 `scene_number` INT NOT NULL,
 `playable`     TINYINT(1) NOT NULL DEFAULT 1,
 `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 `updated_at`    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
 PRIMARY KEY (`id`)
) ;

-- 4. daily_active_users
CREATE TABLE `daily_active_users` (
  `id`            INT NOT NULL AUTO_INCREMENT,
  `user_id`       INT NOT NULL,
  `activity_date` DATE NOT NULL,
  `created_at`   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at`    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_user_date` (`user_id`, `activity_date`),
  CONSTRAINT `fk_dau_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
) ;

-- 5. room_users
CREATE TABLE `room_users` (
  `id`          INT NOT NULL AUTO_INCREMENT,
  `room_id`     INT NOT NULL,
  `user_id`     INT NOT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at`    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_room_user` (`room_id`, `user_id`),
  CONSTRAINT `fk_ru_room` FOREIGN KEY (`room_id`) REFERENCES `rooms` (`id`),
  CONSTRAINT `fk_ru_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
) ;

-- 6. miniGame_logs
CREATE TABLE `miniGame_logs` (
 `id`          INT NOT NULL AUTO_INCREMENT,
 `room_id`     INT NOT NULL,
 `miniGame_id` INT NOT NULL,
 `user_id`     INT NOT NULL,
 `rank`        INT,
 `score`       INT,
 `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 `updated_at`    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
 PRIMARY KEY (`id`),
 CONSTRAINT `fk_ml_room`     FOREIGN KEY (`room_id`)     REFERENCES `rooms` (`id`),
 CONSTRAINT `fk_ml_miniGame` FOREIGN KEY (`miniGame_id`) REFERENCES `miniGames` (`id`),
 CONSTRAINT `fk_ml_user`     FOREIGN KEY (`user_id`)     REFERENCES `users` (`id`)
) ;