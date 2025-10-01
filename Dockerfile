# 公式の .NET SDK イメージを使う
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY . .

# 直接 publish する（restore は不要）
RUN dotnet publish -c Release -o /app

# 実行用のランタイムイメージ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=build /app .

# Bot を起動
ENTRYPOINT ["dotnet", "FirstBot.dll"]
