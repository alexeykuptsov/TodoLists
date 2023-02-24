# To-Do Lists

## Getting Started

To use the app run the launcher app `TodoLists.Launcher/bin/Debug/net7.0-windows/TodoLists.Launcher.exe` and open
https://localhost:7147 in a browser.

## Создание базы с нуля

Нужна база Postgres (проверялось на версии 14, на других тоже должно работать).

```postgresql
create user todo_lists_app password 'pass' createdb;
```

```shell
cd App
dotnet ef database update
```

## Пересоздание базы

Перед выполнением `database drop` нужно закрыть все подключения к БД.

```shell
PS> cd App
PS> .\LocalDevScripts\Drop_UpdateMigrations_Create.ps1
```

## Пересоздание последней миграции

До первого релиза последняя миграция также является и единственной.

Перед выполнением `database drop` нужно закрыть все подключения к БД.

```shell
PS> cd App
PS> .\LocalDevScripts\Drop_UpdateMigrations_Create.ps1
```
