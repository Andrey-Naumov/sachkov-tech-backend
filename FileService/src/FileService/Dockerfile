FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8003

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
WORKDIR /src

# Установка необходимых пакетов для работы с архивом
RUN apt-get update && apt-get install -y xz-utils

# Добавляем и распаковываем FFmpeg
ENV FFMPEG_DIR=/app/ffmpeg
COPY ffmpeg-master-latest-linux64-lgpl.tar.xz .
RUN mkdir -p $FFMPEG_DIR && \
    tar -xf ffmpeg-master-latest-linux64-lgpl.tar.xz -C $FFMPEG_DIR && \
    rm ffmpeg-master-latest-linux64-lgpl.tar.xz && \
    chmod +x $FFMPEG_DIR/ffmpeg-master-latest-linux64-lgpl/bin/ffmpeg && \
    chmod +x $FFMPEG_DIR/ffmpeg-master-latest-linux64-lgpl/bin/ffprobe && \
    ln -s $FFMPEG_DIR/ffmpeg-master-latest-linux64-lgpl/bin/ffmpeg /usr/local/bin/ffmpeg && \
    ln -s $FFMPEG_DIR/ffmpeg-master-latest-linux64-lgpl/bin/ffprobe /usr/local/bin/ffprobe

RUN dotnet nuget add source https://nuget.pkg.github.com/SachkovTech/index.json \
    -n github \
    --username $NUGET_USERNAME \
    --password $NUGET_PASSWORD \
    --store-password-in-clear-text

COPY ["src/FileService/FileService.csproj", "src/FileService/"]
COPY ["src/FileService.Contracts/FileService.Contracts.csproj", "src/FileService.Contracts/"]
RUN dotnet restore "src/FileService/FileService.csproj"
COPY . .
WORKDIR "/src/src/FileService"
RUN dotnet build "FileService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FileService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Копируем FFmpeg из build stage
COPY --from=build /app/ffmpeg /app/ffmpeg

USER root

RUN ln -s /app/ffmpeg/ffmpeg-master-latest-linux64-lgpl/bin/ffmpeg /usr/local/bin/ffmpeg && \
    ln -s /app/ffmpeg/ffmpeg-master-latest-linux64-lgpl/bin/ffprobe /usr/local/bin/ffprobe

USER $APP_UID

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.dll"]