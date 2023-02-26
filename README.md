# To-Do Lists

To-Do Lists in just another appllication for managing tasks with lists of check boxes.
The app is implemented as an web application (ASP.NET Core 7 + Vue.js + DevExtreme) with a PostgreSQL database behind.
Also there is a WPF launcher which is used to run the app and the database on Windows (in the same manner as pgAdmin is run).

The purpose of the project is practicing with programming languages, technologies and libraries used in the app implementation.

## Getting Started

To use the app run the launcher app `TodoLists.Launcher/bin/Debug/net7.0-windows/TodoLists.Launcher.exe` and open
https://localhost:7147 in a browser.

## Development

### Branching strategy

We use [Git Flow](http://danielkummer.github.io/git-flow-cheatsheet/index.html).

We use classic merges and avoid rebasing, amend-committing and other Git commands that change history.

### User Story Personas

The following personas are used in user stories bug tracking and implementation of manual and automated tests.

*Kevin* uses To-Do Lists app hlsted on his local machine.
His OS is Windows.
He installs the app by unpacking a ZIP distributive and followinf section Getting Started.

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
PS> .\LocalDevScripts\Drop_Create.ps1
```

## Пересоздание последней миграции

До первого релиза последняя миграция также является и единственной.

Перед выполнением `database drop` нужно закрыть все подключения к БД.

```shell
PS> cd App
PS> .\LocalDevScripts\Drop_UpdateMigrations_Create.ps1
```
