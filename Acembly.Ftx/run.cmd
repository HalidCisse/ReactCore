@echo off

dotnet restore

dotnet publish -r win10-x64 --output bin/dist/win

cd ui

CMD /C npm install

CMD /C npm start