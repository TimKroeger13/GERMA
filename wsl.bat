@echo off
powershell -NoExit -Command "wsl bash -c \"sleep 6; docker start gsp; docker start germalocal; docker ps; exec bash\""
