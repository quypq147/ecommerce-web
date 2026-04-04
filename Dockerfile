FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy file csproj trước để tận dụng cache restore
COPY ["EcommerceApp.csproj", "./"]
RUN dotnet restore "EcommerceApp.csproj"

# Copy toàn bộ source và publish
COPY . .
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EcommerceApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Azure App Service / Azure Container Apps thường map vào cổng 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Chạy dưới user non-root để tăng bảo mật
RUN useradd -m appuser
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcommerceApp.dll"]
