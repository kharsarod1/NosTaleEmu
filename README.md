# NosTaleEmu

Base de servidor privado de NosTale desarrollada en **C# / .NET 10**.

## Estructura

```text
NosTaleEmu.sln
database/
  schema.sql
src/
  NosTaleEmu.Core/
  NosTaleEmu.Dto/
  NosTaleEmu.Database/
  NosTaleEmu.Services/
  NosTaleEmu.LoginServer/
  NosTaleEmu.WorldServer/
```

## Base de datos

Solo es necesario tener un servidor **MySQL** en funcionamiento.

Al iniciar el **LoginServer** por primera vez, la base de datos y las tablas necesarias se crearán automáticamente si no existen.

Para crear una cuenta:

```bash
dotnet run --project src/NosTaleEmu.LoginServer -- create-account <usuario> <contraseña>
```

## Compilar

```bash
dotnet build NosTaleEmu.sln
```

## Ejecutar

```bash
# LoginServer
dotnet run --project src/NosTaleEmu.LoginServer

# WorldServer
dotnet run --project src/NosTaleEmu.WorldServer
```

Puertos por defecto:

* **LoginServer:** `4005 (ES)`
* **WorldServer:** `4001`

Los archivos `config.json` se generan automáticamente la primera vez que se inicia cada servidor.

## Estado

Este proyecto proporciona una base funcional sobre la que continuar el desarrollo. Muchas características del juego aún no están implementadas y se irán añadiendo progresivamente.

## Donaciones

Si este proyecto te resulta útil y quieres apoyar su desarrollo, puedes hacer una donación.

Las donaciones ayudan a dedicar más tiempo al proyecto, mantenerlo actualizado y seguir implementando nuevas características.

**❤️ Gracias por apoyar el proyecto :)**
