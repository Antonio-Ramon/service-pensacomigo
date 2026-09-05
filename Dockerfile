# Build e runtime separados: a imagem final leva só o runtime, sem o SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/PensaComigo.Web -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Cloud Run injeta a porta em $PORT (8080 por padrão); o 5001 do launchSettings
# não vale aqui, então o Kestrel é apontado explicitamente.
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PensaComigo.Web.dll"]
