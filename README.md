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

## Cliente

Esta fuente está destinada para la versión 3075 del cliente (2017). No tendrá soporte para versiones actuales del juego.

## Base de datos

Solo es necesario tener un servidor **MySQL** en funcionamiento.

Al iniciar el **LoginServer** y **WorldServer** por primera vez, la base de datos y las tablas necesarias se crearán automáticamente si no existen.

Puertos por defecto:

* **LoginServer:** `4005 (ES)`
* **WorldServer:** `4001`

Los archivos `config.json` se generan automáticamente la primera vez que se inicia cada servidor.

## Estado

Este proyecto proporciona una base funcional sobre la que continuar el desarrollo. Muchas características del juego aún no están implementadas y se irán añadiendo progresivamente.

## Donaciones

Si quieres apoyar el desarrollo del proyecto, puedes hacer una donación.

Las donaciones ayudan a dedicar más tiempo al proyecto, mantenerlo actualizado y seguir implementando nuevas características.

**❤️ Gracias por apoyar el proyecto :)**

## Aviso

Gran parte del código está hecho por una IA para ahorrar tiempo, aún así el código es revisado.

Las pruebas del código están bajo conocimientos de C#.

Es importante aclarar esto porque la honestidad debe permanecer.
