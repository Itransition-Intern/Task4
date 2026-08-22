FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Task4.Web/Task4.Web.csproj src/Task4.Web/
RUN dotnet restore src/Task4.Web/Task4.Web.csproj

COPY src/Task4.Web/ src/Task4.Web/
RUN dotnet publish src/Task4.Web/Task4.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_hostBuilder__reloadConfigOnChange=false

ENTRYPOINT ["dotnet", "Task4.Web.dll"]