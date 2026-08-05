# Instalar la base de datos

Guía rápida para alguien que nunca instaló MySQL.

## 1. Instalar MySQL (o MariaDB, es compatible)

**Windows:** descargá el instalador de https://dev.mysql.com/downloads/installer/ y seguí el wizard (elegí "Server only" si no querés herramientas extra). Al final te va a pedir que pongas una contraseña para el usuario `root` — anotala.

**Linux (Ubuntu/Debian):**
```bash
sudo apt update
sudo apt install mysql-server
sudo mysql_secure_installation   # te va a pedir configurar una contraseña de root
```

**macOS:**
```bash
brew install mysql
brew services start mysql
```

## 2. Crear la base de datos

Desde una terminal, parado en la carpeta del proyecto:

```bash
mysql -u root -p < database/schema-login.sql
mysql -u root -p < database/schema-world.sql
```

Te va a pedir la contraseña de `root` que pusiste en el paso 1. Esto crea:
- Las bases `login` y `world`
- Un usuario `nostaleemu` con contraseña `changeme` (¡cambiala!)
- La tabla `accounts`

## 3. Configurar el LoginServer y el WorldServer

Al correr cada server por primera vez (`dotnet run --project src/NosTaleEmu.LoginServer` / `.../NosTaleEmu.WorldServer`), si no existe `config.json` se crea uno solo con valores por defecto. Ajustá `MySqlConnectionString` en cada uno:

```json
// src/NosTaleEmu.LoginServer/config.json
"MySqlConnectionString": "Server=127.0.0.1;Port=3306;Database=login;Uid=nostaleemu;Pwd=TU_CONTRASEÑA_REAL;"

// src/NosTaleEmu.WorldServer/config.json
"MySqlConnectionString": "Server=127.0.0.1;Port=3306;Database=world;Uid=nostaleemu;Pwd=TU_CONTRASEÑA_REAL;"
```

Si cambiaste la contraseña del usuario `nostaleemu` en el paso 2 (recomendado), actualizala en ambos `config.json` y en MySQL:

```sql
ALTER USER 'nostaleemu'@'%' IDENTIFIED BY 'tu_nueva_contraseña';
```

El WorldServer también trae `"Rates"` en su `config.json` (ExpRate, DropRate, GoldRate, etc.) — se ajustan ahí directamente, sin tocar código.

También trae `"DisplayLogs"` (true/false: muestra u oculta los logs de conexiones y paquetes) y `"EnableCommands"` (true/false: activa la consola interactiva de comandos). Con el server corriendo, escribí `help` en la consola para ver todos los comandos disponibles (crear cuentas, mandar paquetes a un jugador conectado, ver los rates actuales, apagar el server prolijamente, etc.). Si vas a usar la consola de comandos seguido, es buena idea poner `DisplayLogs` en `false` para que los logs de paquetes no se mezclen con lo que estás escribiendo.

## 4. Crear una cuenta de prueba

No hace falta escribir SQL a mano — el LoginServer trae un modo de línea de comandos:

```bash
dotnet run --project src/NosTaleEmu.LoginServer -- create-account test test
```

Esto crea el usuario `test` con contraseña `test`, ya con el hash correcto (SHA-512), listo para loguearse con el cliente.

## 5. Arrancar todo

```bash
dotnet run --project src/NosTaleEmu.LoginServer
dotnet run --project src/NosTaleEmu.WorldServer
```
