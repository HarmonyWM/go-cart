@echo off
echo Starting MaliMove Backend...
start "MaliMove Backend" cmd /k "cd /d "c:\Users\MOGAU MADISHA\go-cart\backend" && dotnet run"

echo Waiting for backend to start...
timeout /t 5 /nobreak >nul

echo Starting MaliMove Frontend...
start "MaliMove Frontend" cmd /k "cd /d "c:\Users\MOGAU MADISHA\go-cart\frontend" && npm run dev"

echo.
echo Both servers starting...
echo Backend:  http://localhost:5000
echo Frontend: http://localhost:5173
echo.
timeout /t 8 /nobreak >nul

start http://localhost:5173
