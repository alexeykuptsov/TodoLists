Push-Location (Join-Path (Split-Path $MyInvocation.MyCommand.Path) ".." -Resolve)
try
{
    . LocalDevScripts/Common.ps1

    dotnet ef database drop --force
    ThrowOnNativeFailure
    
    dotnet ef database update
    ThrowOnNativeFailure
    
    $env:PGPASSWORD = 'pass'
    
    psql -h localhost -U todo_lists_app -p 5432 -d todo_lists -c "insert into super_users (username, username_lower_case, password_hash, password_salt) values ('admin', 'admin', '\x1e74dc25b2243411bd209730beae7c01aeace028c7c5654d162fff4ed93dba04a22e16177a4111912ebb150b9d3ed90dc86ad998a8a5ad8b6d343a86112a7839', '\x0cea919c97e407157913188cf4efa2bf696cfcec2a141e32be319c84048cb6646c3053ec217f4e10b5d3e8a007e62d6ca2917d774094f37029a981878dc00d0678265cad8ccb1a3421191bf0fbd9132b430ffd56c56e09628d06386d3af2cb173b8508be177af71cef8bcb541e7011ba95bbc374152d36cbd56a90c609e73c0e');"
    ThrowOnNativeFailure
    
    psql -h localhost -U todo_lists_app -p 5432 -d todo_lists -c "insert into profiles (name, created_at) values ('dev', 1669497925237);"
    ThrowOnNativeFailure
    
    psql -h localhost -U todo_lists_app -p 5432 -d todo_lists -c "insert into users (profile_id, username, username_lower_case, password_hash, password_salt) values (1, 'user', 'user', '\xb6c447ff438318d2e8168cc9e7bd6c978a7bd989a541af2a58b79798a4106b0c59de80a1ea7fc79860508b2024a8ff0b214d792f7572f2a8860ae2ed2141b61f', '\xc38cf1c6bbb3454871256244a9ee5f6849ef4770cb2ad2243542e21dc8692dd29c989b9f377e435b4152844d818ba88aeda111d1408dedc536c694b5addb1f6f95bf4a1702dceddb9479f4c899e339d2ab64872e6da1ebb76f8e06e3cdbb3fc4c9ae0bfc0fc8f9924bb2eabb3113e35331989b17c0ea8cd95e9bdee517778955');"
    ThrowOnNativeFailure
}
finally
{
    Pop-Location
}
