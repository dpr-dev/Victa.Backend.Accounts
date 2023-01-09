FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
 
COPY ["Victa.Backend.Accounts/Victa.Backend.Accounts.csproj", "Victa.Backend.Accounts/"]
COPY ["Victa.Backend.Accounts.Contracts/Victa.Backend.Accounts.Contracts.csproj", "Victa.Backend.Accounts.Contracts/"]

RUN dotnet restore "Victa.Backend.Accounts/Victa.Backend.Accounts.csproj"

COPY . .

WORKDIR "/src/Victa.Backend.Accounts"
RUN dotnet build "Victa.Backend.Accounts.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Victa.Backend.Accounts.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Victa.Backend.Accounts.dll"]