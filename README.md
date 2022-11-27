# Списки To-Do

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

```shell
cd App
dotnet ef database drop --force
dotnet ef database update
```

Создаем суперпользователя `admin:pass` и профиль Dev:

```postgresql
insert into super_users (username, username_lower_case, password_hash, password_salt)
values ('admin', 'admin', '\x1e74dc25b2243411bd209730beae7c01aeace028c7c5654d162fff4ed93dba04a22e16177a4111912ebb150b9d3ed90dc86ad998a8a5ad8b6d343a86112a7839', '\x0cea919c97e407157913188cf4efa2bf696cfcec2a141e32be319c84048cb6646c3053ec217f4e10b5d3e8a007e62d6ca2917d774094f37029a981878dc00d0678265cad8ccb1a3421191bf0fbd9132b430ffd56c56e09628d06386d3af2cb173b8508be177af71cef8bcb541e7011ba95bbc374152d36cbd56a90c609e73c0e');

insert into public.profiles (name, created_at)
values ('dev', 1669497925237);
```

## Пересоздание последней миграции

До первого релиза последняя миграция также является и единственной.

Перед выполнением `database drop` нужно закрыть все подключения к БД.

```shell
PS> cd App
PS> .\LocalDevScripts\Drop_UpdateMigrations_Create.ps1
```
