robocopy "mod-data" "Wah!Core\bin\Debug\mod-data" /E
set rce=%errorlevel%

if %rce% leq 2 exit 0 else exit %rce%
