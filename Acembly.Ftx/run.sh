#!/bin/sh

dotnet restore

dotnet publish -r osx.10.11-x64 --output bin/dist/osx

cd ui

CMD /C npm install

CMD /C npm start