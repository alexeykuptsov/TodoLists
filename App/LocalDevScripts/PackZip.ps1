Push-Location (Join-Path (Split-Path $MyInvocation.MyCommand.Path) ".." -Resolve)
try
{
    . LocalDevScripts/Common.ps1

    $PsqlFolderName = "bin\Debug\net7.0\pgsql"
    if (Test-Path $PsqlFolderName)
    {
        Remove-Item -Recurse -Force $PsqlFolderName
    }
    New-Item -ItemType Directory -Force -Path $PsqlFolderName

    $excludes = "pgAdmin 4"
    Get-ChildItem "C:\Tools\postgresql-15.1-1-windows-x64-binaries\pgsql" -Directory |
            Where-Object{$_.Name -notin $excludes} |
            Copy-Item -Destination $PsqlFolderName -Recurse -Force
}
finally
{
    Pop-Location
}
