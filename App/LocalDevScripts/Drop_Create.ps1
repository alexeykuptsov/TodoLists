Push-Location (Join-Path (Split-Path $MyInvocation.MyCommand.Path) ".." -Resolve)
try
{
    . LocalDevScripts/Common.ps1

    dotnet ef database drop --force
    ThrowOnNativeFailure
    
    dotnet ef database update
    ThrowOnNativeFailure
}
finally
{
    Pop-Location
}
