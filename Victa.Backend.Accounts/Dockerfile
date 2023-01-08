FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_USE_POLLING_FILE_WATCHER=false
ENV ASPNETCORE_URLS=http://+:80

ARG BUILD_CONFIGURATION=Release

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["Victa.Backend.Accounts/Victa.Backend.Accounts.csproj", "Victa.Backend.Accounts/"]


RUN dotnet restore "Victa.Backend.Accounts/Victa.Backend.Accounts.csproj"

COPY . .

WORKDIR "/src/Victa.Backend.Accounts"

RUN dotnet build "Victa.Backend.Accounts.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "Victa.Backend.Accounts.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Victa.Backend.Accounts.dll"]