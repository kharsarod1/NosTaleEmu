-- ============================================================
-- NosTaleEmu - Base de datos del LoginServer
-- Ejecutar como root (o un usuario con permisos de administración):
--   mysql -u root -p < database/schema-login.sql
--
-- Este script solo crea la base y el usuario de la app. La tabla
-- "accounts" la crea sola EF Core la primera vez que arrancás el
-- LoginServer (LoginDbContextFactory.EnsureDatabaseReady), así que no hace
-- falta definirla acá a mano.
-- ============================================================

CREATE DATABASE IF NOT EXISTS login
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

-- Usuario dedicado para la app (no uses root en el connection string).
-- Cambiá 'changeme' por una contraseña real antes de usarlo en producción,
-- y actualizá el mismo valor en config.json del LoginServer.
CREATE USER IF NOT EXISTS 'nostaleemu'@'%' IDENTIFIED BY 'changeme';
GRANT ALL PRIVILEGES ON login.* TO 'nostaleemu'@'%';
FLUSH PRIVILEGES;

