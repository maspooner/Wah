del "Wah!Core\bin\Debug\Wah!Commands.dll"
copy "Wah!Commands\bin\Debug\Wah!Commands.dll" "Wah!Core\bin\Debug\Wah!Commands.dll"
set rce=%errorlevel%
if not %rce%==1 exit %rce% else exit 0