FOR /d /r . %%d IN ("bin") DO @IF EXIST "%%d" rd /s /q "%%d"
FOR /d /r . %%d IN ("obj") DO @IF EXIST "%%d" rd /s /q "%%d"
FOR /d /r . %%d IN ("packages") DO @IF EXIST "%%d" rd /s /q "%%d"
FOR /d /r . %%d IN (".vs") DO @IF EXIST "%%d" rd /s /q "%%d"