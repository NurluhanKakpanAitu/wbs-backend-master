FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

COPY ["src/BonusSystem.Shared/BonusSystem.Shared.csproj", "src/BonusSystem.Shared/"]
COPY ["src/BonusSystem.Core/BonusSystem.Core.csproj", "src/BonusSystem.Core/"]
COPY ["src/BonusSystem.Infrastructure/BonusSystem.Infrastructure.csproj", "src/BonusSystem.Infrastructure/"]
COPY ["src/BonusSystem.Api/BonusSystem.Api.csproj", "src/BonusSystem.Api/"]

RUN dotnet restore "src/BonusSystem.Api/BonusSystem.Api.csproj"

COPY . .

WORKDIR /app/src/BonusSystem.Api
RUN dotnet build -c Release -o /app/build

RUN dotnet tool install dotnet-ef --tool-path /app/tools
ENV PATH="${PATH}:/app/tools"
ENV HOME=/tmp
ENV DOTNET_CLI_HOME=/tmp

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
RUN dotnet publish /app/src/BonusSystem.Api/BonusSystem.Api.csproj -c Release -o /app/publish

WORKDIR /app/publish
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
ENTRYPOINT ["dotnet", "BonusSystem.Api.dll"]
