FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS base

WORKDIR /src

ARG PAT
RUN wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true

COPY ["NuGet.Config", "/root/.nuget/NuGet/NuGet.Config"]

COPY ["./Api/Api.csproj", "./Api/"]
COPY ["./Domain/Domain.csproj", "./Domain/"]
COPY ["./CQRS/CQRS.csproj", "./CQRS/"]
COPY ["./Rest/Rest.csproj", "./Rest/"]
COPY ["./Application/Application.csproj", "./Application/"]

RUN dotnet restore "./Api/Api.csproj"

COPY ["./Api/", "./Api/"]
COPY ["./Domain/", "./Domain/"]
COPY ["./CQRS/", "./CQRS/"]
COPY ["./Rest/", "./Rest/"]
COPY ["./Application/", "./Application/"]

RUN dotnet publish "Api/Api.csproj" -c Release -o /out --runtime linux-musl-x64 --self-contained true /p:PublishSingleFile=true



FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final

# Add curl command to image
RUN apk add curl
RUN apk add --no-cache tzdata

# Configure Kestrel web server to bind to port 80 when present
ENV ASPNETCORE_URLS="http://+:80"
ENV ASPNETCORE_HTTP_PORTS=80
# Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true
# Default Timezone
ENV TZ=Europe/Berlin					

WORKDIR /app
COPY --from=base /out ./

EXPOSE 80

ENTRYPOINT ["./Api"]