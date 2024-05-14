#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SEP490_G28_SP24_WebSellingPaint/SEP490_G28_SP24_WebSellingPaint.csproj", "SEP490_G28_SP24_WebSellingPaint/"]
RUN dotnet restore "SEP490_G28_SP24_WebSellingPaint/SEP490_G28_SP24_WebSellingPaint.csproj"
COPY . .
WORKDIR "/src/SEP490_G28_SP24_WebSellingPaint"
RUN dotnet build "SEP490_G28_SP24_WebSellingPaint.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SEP490_G28_SP24_WebSellingPaint.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SEP490_G28_SP24_WebSellingPaint.dll"]
