FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AppWebSistemaComandasDigital.csproj ./
RUN dotnet restore AppWebSistemaComandasDigital.csproj

COPY . .
RUN dotnet publish AppWebSistemaComandasDigital.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

CMD ASPNETCORE_URLS=http://0.0.0.0:${PORT:-3000} dotnet AppWebSistemaComandasDigital.dll
