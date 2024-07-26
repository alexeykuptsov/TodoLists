# To-Do Lists

To-Do Lists is just another application for managing tasks with lists of check boxes.
The app is implemented as a web application (ASP.NET Core + Vue.js + DevExtreme) with a PostgreSQL database behind.
Also there is a WPF launcher which is used to run the app and the database on Windows (in the same manner as pgAdmin is run).

The purpose of the project is practicing with programming languages, technologies and libraries used in the app implementation.

## Getting Started

_(to be implemented)_

Download and unpack ZIP-archive.

To use the app run the launcher app `TodoLists.Launcher/bin/Debug/net7.0-windows/TodoLists.Launcher.exe` and open
https://localhost:7147 in a browser.

## Issue Tracking

Issues: [https://github.com/alexeykuptsov/TodoLists/issues](https://github.com/alexeykuptsov/TodoLists/issues)

### User Story Personas

The following personas are used in user stories bug tracking and implementation of manual and automated tests.

*Kevin* uses To-Do Lists app hosted on his local machine.
His OS is Windows.
He installs the app by unpacking a ZIP distributive and following section Getting Started.

## Development

### After-Checkout Development Environment Setup

Install Postgres 14, .NET 7, Node.js and JetBrains Rider (as a default IDE for this project).

```shell
dotnet tool install --global dotnet-ef --version 8.0.0
npm install -g @vue/cli
cd App\vue
npm install
```

```postgresql
create user dev__todo_lists_app password 'dev__todo_lists_app' createdb;
```

```shell
cd App
dotnet ef database update
```

Also for proper running of Launcher you should set environment variable `TODO_LISTS_PGSQL_DIR` so that it refers to folder `pgsql` of unarchived
[Postgres ZIP distributive](https://www.enterprisedb.com/download-postgresql-binaries).
For example: *C:\Tools\postgresql-15.1-1-windows-x64-binaries\pgsql*

### Running in a Development Environment

In Rider:

1. Run configuration "buildDevWatch".

2. Debug configuration "App: index.html" (`Ctrl+F9` in Rider).

### Tests

Integration tests (project `Tests.Integration`) are supposed to be run locally and the web app is running.
For example, you may run configuration "App: index.html" and then run all tests from solution (`Ctrl+;,L` in Rider).

### Working with Local Dev Database

#### Drop and Create

⚠
The following script runs `database drop` so you need to close all connections to database `todo_lists`.

```shell
PS> cd App
PS> .\LocalDevScripts\Drop_Create.ps1
```

#### Add Migration

```shell
PS> cd App
PS> dotnet ef migrations add AddProjectsIsDeleted
PS> dotnet ef database update
```

### Branching strategy

We use [Git Flow](http://danielkummer.github.io/git-flow-cheatsheet/index.html).

We use classic merges and avoid rebasing, amend-committing and other Git commands that change history.
