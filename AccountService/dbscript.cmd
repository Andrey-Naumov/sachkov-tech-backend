@echo off
setlocal enabledelayedexpansion

:: Запуск Docker Compose
docker-compose up -d
if errorlevel 1 (
    echo Возникла ошибка при запуске Docker Compose.
    exit /b 1
)

:: Удаление базы данных
dotnet-ef database drop -f -c AccountsDbContext -p .\src\AccountService.Infrastructure\ -s .\src\AccountService.Api\
if errorlevel 1 (
    echo Возникла ошибка при удалении базы данных.
    exit /b 1
)

:: Удаление миграций
dotnet-ef migrations remove -c AccountsDbContext -p .\src\AccountService.Infrastructure\ -s .\src\AccountService.Api\
if errorlevel 1 (
    echo Возникла ошибка при удалении миграций.
    exit /b 1
)

:: Добавление новой миграции
dotnet-ef migrations add Accounts_init -c AccountsDbContext -p .\src\AccountService.Infrastructure\ -s .\src\AccountService.Api\
if errorlevel 1 (
    echo Возникла ошибка при добавлении миграции.
    exit /b 1
)

:: Применение миграций к базе данных
dotnet-ef database update -c AccountsDbContext -p .\src\AccountService.Infrastructure\ -s .\src\AccountService.Api\
if errorlevel 1 (
    echo Возникла ошибка при обновлении базы данных.
    exit /b 1
)

:: Успешное завершение
echo Миграции успешно выполнены.
exit /b 0
