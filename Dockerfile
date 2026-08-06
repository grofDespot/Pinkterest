FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Domain/Domain.csproj Domain/
COPY CrossCutting/CrossCutting.csproj CrossCutting/
COPY App/App.csproj App/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Web/Web.csproj Web/
RUN dotnet restore Web/Web.csproj

COPY Domain/ Domain/
COPY CrossCutting/ CrossCutting/
COPY App/ App/
COPY Infrastructure/ Infrastructure/
COPY Web/ Web/
RUN dotnet publish Web/Web.csproj -c Release -o /publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /publish .

USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_gcServer=1

EXPOSE 8080
ENTRYPOINT ["dotnet", "Pinkterest.Web.dll"]
