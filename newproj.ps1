dotnet new globaljson;

dotnet new classlib -o vdb_node_wireguard_manipulator;
dotnet new webapi -o vdb_node_api;
dotnet new xunit -o vdb_node_wireguard_manipulator.tests;
dotnet new xunit -o vdb_node_api.tests;

dotnet new sln --name vdb_node;
dotnet sln add vdb_node_wireguard_manipulator/vdb_node_wireguard_manipulator.csproj;
dotnet sln add vdb_node_api/vdb_node_api.csproj;
dotnet sln add vdb_node_wireguard_manipulator.tests/vdb_node_wireguard_manipulator.tests.csproj;
dotnet sln add vdb_node_api.tests/vdb_node_api.tests.csproj;

dotnet new gitignore;

New-Item dockerfile;
New-Item docker-compose.yml;
New-Item docker-compose.override.yml;
New-Item .dockerignore;
Set-Content .dockerignore '**/.classpath
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
**/*secret*.json
LICENSE
README.md'

git init;
pause "Press any key to exit"