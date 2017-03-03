robocopy "mod-data" "Wah!Core\bin\Debug\mod-data" /E
set rce=%errorlevel%

if not %rce%==1 exit %rce% else exit 0
