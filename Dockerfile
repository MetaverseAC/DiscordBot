FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY ./DiscordBot/*.csproj .
RUN dotnet restore

# copy and publish app and libraries
COPY ./DiscordBot .
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app .

# set git metadata
ARG SET_GIT_HASH=""
ENV GIT_HASH=$SET_GIT_HASH
ARG SET_GIT_DATE=""
ENV GIT_DATE=$SET_GIT_DATE

ENTRYPOINT ["./DiscordBot"]