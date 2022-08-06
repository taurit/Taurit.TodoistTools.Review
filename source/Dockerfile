# BUILD:
# docker build -f ./Dockerfile -t review:latest .
#
# CLEAN:
# docker stop review
# docker rm review
#
# RUN:
# docker run --detach --name review -p 8888:80/tcp --restart unless-stopped --env-file .env review:latest

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
RUN pwd

# to get the typescript to compile...
RUN apt-get update && apt-get install -y curl
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash -
RUN apt-get install -y nodejs

WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["Taurit.TodoistTools.Review/Taurit.TodoistTools.Review.csproj", "Taurit.TodoistTools.Review/"]
RUN dotnet restore "Taurit.TodoistTools.Review/Taurit.TodoistTools.Review.csproj"
COPY . .

WORKDIR /src/Taurit.TodoistTools.Review/wwwroot/js/
RUN npm install

WORKDIR "/src/Taurit.TodoistTools.Review"
RUN dotnet build "Taurit.TodoistTools.Review.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Taurit.TodoistTools.Review.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Taurit.TodoistTools.Review.dll"]