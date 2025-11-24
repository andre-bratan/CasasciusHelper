ARG DOTNET_REPO=mcr.microsoft.com/dotnet
ARG DOTNET_VERSION=8.0.300
ARG ASPNET_VERSION=8.0.3

FROM $DOTNET_REPO/aspnet:$ASPNET_VERSION AS base

# Data directory (make it writable by the application)
RUN mkdir -p /app/Data && \
    chown -R $APP_UID:root /app/Data && \
    chmod -R 755 /app/Data

USER $APP_UID

WORKDIR /app

EXPOSE 8080
EXPOSE 8081

FROM $DOTNET_REPO/sdk:$DOTNET_VERSION AS restore-subset
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global --no-cache dotnet-subset --version 0.3.2
WORKDIR /src
COPY . .
RUN dotnet subset restore ./CasasciusHelper/CasasciusHelper.csproj --root-directory . --output restore_subset/

FROM $DOTNET_REPO/sdk:$DOTNET_VERSION AS build-and-publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# without "subset" (you need to figure out and copy restore-related files by yourself)
#COPY ["nuget.config", "." ]
#COPY ["Directory.Packages.props", "." ]
#COPY ["Casascius.Coins/Casascius.Coins.csproj", "Casascius.Coins/"]
#COPY ["Casascius.Port/Casascius.Port.csproj", "Casascius.Port/"]
#COPY ["CasasciusHelper.sln", "CasasciusHelper.sln"]
#COPY ["CasasciusHelper/CasasciusHelper.csproj", "CasasciusHelper/"]
#COPY ["CasasciusHelper.Core/CasasciusHelper.Core.csproj", "CasasciusHelper.Core/"]
#COPY ["CasasciusHelper.Database/CasasciusHelper.Database.csproj", "CasasciusHelper.Database/"]
#RUN dotnet restore --locked-mode "CasasciusHelper/CasasciusHelper.csproj"
# with "subset" (everything is done automatically, but the context gets copied twice)
COPY --from=restore-subset /src/restore_subset .
RUN dotnet restore --locked-mode "CasasciusHelper/CasasciusHelper.csproj"

COPY . .
WORKDIR "/src/CasasciusHelper"

#RUN dotnet build "./CasasciusHelper.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build
#RUN dotnet publish "./CasasciusHelper.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false
# OR (the following line is compatible with "--no-build" option on "publish")
RUN dotnet build "./CasasciusHelper.csproj" -c $BUILD_CONFIGURATION --no-restore
RUN dotnet publish "./CasasciusHelper.csproj" -c $BUILD_CONFIGURATION --no-restore --no-build -o /app/publish /p:UseAppHost=false

FROM base AS final

WORKDIR /app
COPY --from=build-and-publish /app/publish .

ARG ARG_COMMIT_SHA
ARG ARG_VERSION

ENV COMMIT_SHA="$ARG_COMMIT_SHA"
ENV VERSION="$ARG_VERSION"

VOLUME /app/Data

ENTRYPOINT ["dotnet", "CasasciusHelper.dll"]
