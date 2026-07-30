-- ============================================================
-- NosTaleEmu - Base de datos del WorldServer
-- Ejecutar como root:
--   mysql -u root -p < database/schema-world.sql
--
-- Solo crea la base y el usuario. Las tablas (characters, etc.) las crea
-- sola EF Core en el primer arranque del WorldServer.
-- ============================================================

CREATE DATABASE IF NOT EXISTS world
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

CREATE USER IF NOT EXISTS 'nostaleemu'@'%' IDENTIFIED BY 'changeme';
GRANT ALL PRIVILEGES ON world.* TO 'nostaleemu'@'%';
FLUSH PRIVILEGES;
