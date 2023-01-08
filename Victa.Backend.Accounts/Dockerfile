#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Accounts/Victa.Backend.Accounts/Victa.Backend.Accounts.csproj", "Accounts/Victa.Backend.Accounts/"]
RUN dotnet restore "Accounts/Victa.Backend.Accounts/Victa.Backend.Accounts.csproj"
COPY . .
WORKDIR "/src/Accounts/Victa.Backend.Accounts"
RUN dotnet build "Victa.Backend.Accounts.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Victa.Backend.Accounts.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Victa.Backend.Accounts.dll"]