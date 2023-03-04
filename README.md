# To-Do Lists

To-Do Lists in just another appllication for managing tasks with lists of check boxes.
The app is implemented as an web application (ASP.NET Core 7 + Vue.js + DevExtreme) with a PostgreSQL database behind.
Also there is a WPF launcher which is used to run the app and the database on Windows (in the same manner as pgAdmin is run).

The purpose of the project is practicing with programming languages, technologies and libraries used in the app implementation.

## Getting Started

_(to be implemented)_

Unpack ZIP-archive.

To use the app run the launcher app `TodoLists.Launcher/bin/Debug/net7.0-windows/TodoLists.Launcher.exe` and open
https://localhost:7147 in a browser.

## Development

Kanban board: https://github.com/users/alexeykuptsov/projects/1/views/1

### After-Checkout Development Environment Setup

Install Postgres 14, .NET 7 and JetBrains Rider (as a default IDE for this project).

```shell
dotnet tool install --global dotnet-ef
```

```postgresql
create user todo_lists_app password 'pass' createdb;
```

```shell
cd App
dotnet ef database update
```

### Branching strategy

We use [Git Flow](http://danielkummer.github.io/git-flow-cheatsheet/index.html).

We use classic merges and avoid rebasing, amend-committing and other Git commands that change history.

### User Story Personas

The following personas are used in user stories bug tracking and implementation of manual and automated tests.

*Kevin* uses To-Do Lists app hosted on his local machine.
His OS is Windows.
He installs the app by unpacking a ZIP distributive and following section Getting Started.

### Working with Local Dev Database

#### Drop and Create

⚠
The following script runs `database drop` so you need not close all connections to database `todo_lists`.

```shell
PS> cd App
PS> .\LocalDevScripts\Drop_Create.ps1
```

#### Generate the Last Migration

Before version 1.0.0 the last migration is the single one.
So the script shall be improved after the release.

⚠
The following script runs `database drop` so you need not close all connections to database `todo_lists`.

```shell
PS> cd App
PS> .\LocalDevScripts\Drop_UpdateMigrations_Create.ps1
```
