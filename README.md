# NosTaleEmu

Base mínima de servidor privado (LoginServer + WorldServer) en C# / .NET 10.

## Estructura

```
NosTaleEmu.sln
database/
  schema.sql             -> crea la base de datos y el usuario de MySQL
  INSTALL.md              -> guía paso a paso para instalar MySQL y la DB
src/
  NosTaleEmu.Core/         -> Ciphers (LoginCipher, WorldCipher) + networking base + config JSON
  NosTaleEmu.Dto/          -> DTOs (objetos de transferencia, independientes de la DB)
  NosTaleEmu.Database/     -> EF Core + Pomelo: DbContext, entidades, fábrica de conexión
  NosTaleEmu.Services/     -> Lógica de negocio (AccountService: valida credenciales, crea cuentas)
  NosTaleEmu.LoginServer/  -> Autenticación contra MySQL + listado de canales (paquete NsTeST)
  NosTaleEmu.WorldServer/  -> Servidor de mundo (handshake + dispatcher de paquetes)
```

## Base de datos

Antes de arrancar el LoginServer necesitás MySQL. Ver **`database/INSTALL.md`** para la guía paso a paso (instalar MySQL, crear la base, crear una cuenta de prueba). Resumen ultra rápido:

```bash
mysql -u root -p < database/schema.sql
dotnet run --project src/NosTaleEmu.LoginServer -- create-account test test
```

La tabla `accounts` se crea sola la primera vez que arrancás el LoginServer (EF Core `EnsureCreated`).

## Compilar y ejecutar

```bash
dotnet build NosTaleEmu.sln

# Terminal 1
dotnet run --project src/NosTaleEmu.LoginServer

# Terminal 2
dotnet run --project src/NosTaleEmu.WorldServer
```

- LoginServer escucha en el puerto **4005** (configurable en `src/NosTaleEmu.LoginServer/config.json`, se crea solo la primera vez que corrés el server).
- WorldServer escucha en el puerto **4001** (configurable en `src/NosTaleEmu.WorldServer/config.json`).
- No hay cuentas hardcodeadas: creá las que necesites con `dotnet run --project src/NosTaleEmu.LoginServer -- create-account <usuario> <contraseña>`.

## Notas importantes

1. **Sobre la compilación**: este sandbox no tiene acceso a `nuget.org`, así que no pude bajar y compilar de verdad los paquetes de EF Core/Pomelo acá. Sí compilé y verifiqué: (a) todo lo que no depende de MySQL (Core, WorldServer, Dto) contra el SDK real, y (b) Database/Services/LoginServer contra un stub local que imita exactamente la superficie de la API de EF Core + Pomelo (mismos nombres y firmas de `DbContext`, `DbSet<T>`, `ModelBuilder`, `UseMySql`, etc.), para pescar errores de sintaxis. Aun así, la primera vez que corras `dotnet build` en tu máquina con internet real, puede que haya que resolver algún detalle menor de versión de paquete.
2. **Protocolo simplificado**: el formato exacto de paquetes (separadores, orden de campos, handshake de entrada al mundo) varía según la versión del cliente NosTale. Lo que hay aquí es un esqueleto funcional con la lógica de cifrado correcta y un parser básico — vas a necesitar ajustar los nombres de paquete/campos exactos capturando tráfico de tu cliente (con Wireshark o un proxy) para que calce 100%.
3. **Cifrado**: `LoginCipher` y `WorldCipher` son reescrituras propias, no una copia literal del código de OpenNos — la lógica matemática es la misma (es la que exige el protocolo del cliente para poder hablar con él), pero reestructurada en métodos separados y nombrados.
4. **Licencia**: si en algún momento reutilizas o te inspiras en código de OpenNos (GPL), ese proyecto exige que cualquier derivado mantenga la misma licencia y créditos — vale la pena tenerlo en cuenta si vas a distribuir esto públicamente.
