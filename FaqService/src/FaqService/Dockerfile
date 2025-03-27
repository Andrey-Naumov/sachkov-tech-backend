FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8006

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
WORKDIR /src

RUN dotnet nuget add source https://nuget.pkg.github.com/SachkovTech/index.json \
    -n github \
    --username $NUGET_USERNAME \
    --password $NUGET_PASSWORD \
    --store-password-in-clear-text

COPY ["src/FaqService/FaqService.csproj", "src/FaqService/"]
COPY ["src/FaqService.Contracts/FaqService.Contracts.csproj", "src/FaqService.Contracts/"]
COPY ["src/FaqService/etc/key.pub", "src/FaqService/etc/"]

RUN dotnet restore "src/FaqService/FaqService.csproj"
COPY . .
WORKDIR "/src/src/FaqService"
RUN dotnet build "FaqService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FaqService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FaqService.dll"]
