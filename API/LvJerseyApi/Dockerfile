FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY LvJerseyStore.sln .
COPY API/LvJerseyApi/LvJerseyApi.csproj API/LvJerseyApi/
COPY API/LvJersey.Infrastructure/LvJersey.Infrastructure.csproj API/LvJersey.Infrastructure/
COPY Authentication/Authentication.Application/Authentication.Application.csproj Authentication/Authentication.Application/
COPY Authentication/Authentication.Domain/Authentication.Domain.csproj Authentication/Authentication.Domain/
COPY Authentication/Authentication.Infrastructure/Authentication.Infrastructure.csproj Authentication/Authentication.Infrastructure/
COPY Users/Users.Application/Users.Application.csproj Users/Users.Application/
COPY Users/Users.Domain/Users.Domain.csproj Users/Users.Domain/
COPY Users/Users.Infrastructure/Users.Infrastructure.csproj Users/Users.Infrastructure/
COPY Jerseys/Jerseys.Application/Jerseys.Application.csproj Jerseys/Jerseys.Application/
COPY Jerseys/Jerseys.Domain/Jerseys.Domain.csproj Jerseys/Jerseys.Domain/
COPY Jerseys/Jerseys.Infrastructure/Jerseys.Infrastructure.csproj Jerseys/Jerseys.Infrastructure/
COPY Files/Files.Application/Files.Application.csproj Files/Files.Application/
COPY Files/Files.Domain/Files.Domain.csproj Files/Files.Domain/
COPY Files/Files.Infrastructure/Files.Infrastructure.csproj Files/Files.Infrastructure/
COPY Orders/Orders.Application/Orders.Application.csproj Orders/Orders.Application/
COPY Orders/Orders.Domain/Orders.Domain.csproj Orders/Orders.Domain/
COPY Orders/Orders.Infrastructure/Orders.Infrastructure.csproj Orders/Orders.Infrastructure/
COPY Payments/Payments.Application/Payments.Application.csproj Payments/Payments.Application/
COPY Payments/Payments.Domain/Payments.Domain.csproj Payments/Payments.Domain/
COPY Payments/Payments.Infrastructure/Payments.Infrastructure.csproj Payments/Payments.Infrastructure/
COPY Reviews/Reviews.Application/Reviews.Application.csproj Reviews/Reviews.Application/
COPY Reviews/Reviews.Domain/Reviews.Domain.csproj Reviews/Reviews.Domain/
COPY Reviews/Reviews.Infrastructure/Reviews.Infrastructure.csproj Reviews/Reviews.Infrastructure/
COPY Shared/Shared.Application/Shared.Application.csproj Shared/Shared.Application/
COPY Shared/Shared.Infrastructure/Shared.Infrastructure.csproj Shared/Shared.Infrastructure/

# Restore
RUN dotnet restore API/LvJerseyApi/LvJerseyApi.csproj

# Copy all source
COPY . .

# Publish
WORKDIR /src/API/LvJerseyApi
RUN dotnet publish LvJerseyApi.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LvJerseyApi.dll"]
