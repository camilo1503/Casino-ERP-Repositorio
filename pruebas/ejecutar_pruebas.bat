@echo off
REM Ejecuta todas las pruebas de PitBoss ERP en Windows y guarda las salidas en evidencias\
chcp 65001 >nul
setlocal
cd /d "%~dp0"
set "EV=%~dp0evidencias"
set "LOG=%EV%\ejecucion-windows.txt"
echo === Herramientas ===> "%LOG%"
java -version >> "%LOG%" 2>&1
dotnet --list-sdks >> "%LOG%" 2>&1

set "MVN=mvn"
where mvn >nul 2>&1
if errorlevel 1 (
  if not exist "%USERPROFILE%\.m2\wrapper\apache-maven-3.9.9\bin\mvn.cmd" (
    echo Descargando Maven 3.9.9...>> "%LOG%"
    powershell -NoProfile -Command "Invoke-WebRequest https://repo.maven.apache.org/maven2/org/apache/maven/apache-maven/3.9.9/apache-maven-3.9.9-bin.zip -OutFile $env:TEMP\maven-3.9.9.zip; Expand-Archive $env:TEMP\maven-3.9.9.zip -DestinationPath $env:USERPROFILE\.m2\wrapper -Force"
  )
  set "MVN=%USERPROFILE%\.m2\wrapper\apache-maven-3.9.9\bin\mvn.cmd"
)

echo === mvn test ===>> "%LOG%"
pushd java
call "%MVN%" -B test > "%EV%\maven-salida.txt" 2>&1
echo mvn termino con codigo %ERRORLEVEL%>> "%LOG%"
popd

echo === dotnet test ===>> "%LOG%"
pushd dotnet\PitBoss.Specs
dotnet test --logger "trx;LogFileName=reqnroll.trx" --results-directory "%EV%\dotnet" > "%EV%\dotnet-salida.txt" 2>&1
echo dotnet termino con codigo %ERRORLEVEL%>> "%LOG%"
popd

echo LISTO>> "%LOG%"
echo Pruebas terminadas. Revisa la carpeta evidencias.
