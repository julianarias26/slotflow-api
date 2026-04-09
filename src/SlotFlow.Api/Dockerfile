FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/SlotFlow.Api/SlotFlow.Api.csproj", "src/SlotFlow.Api/"]
RUN dotnet restore "src/SlotFlow.Api/SlotFlow.Api.csproj"
COPY . .
WORKDIR "/src/src/SlotFlow.Api"
RUN dotnet build "SlotFlow.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SlotFlow.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SlotFlow.Api.dll"]