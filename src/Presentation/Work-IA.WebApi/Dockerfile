FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Presentation/Work-IA.WebApi/Work-IA.WebApi.csproj", "src/Presentation/Work-IA.WebApi/"]
COPY ["src/Application/Work-IA.Application/Work-IA.Application.csproj", "src/Application/Work-IA.Application/"]
COPY ["src/Domain/Work-IA.Domain/Work-IA.Domain.csproj", "src/Domain/Work-IA.Domain/"]
COPY ["src/Infrastructure/Work-IA.Infrastructure/Work-IA.Infrastructure.csproj", "src/Infrastructure/Work-IA.Infrastructure/"]
COPY ["src/Aspire/Work-IA.ServiceDefaults/Work-IA.ServiceDefaults.csproj", "src/Aspire/Work-IA.ServiceDefaults/"]
RUN dotnet restore "src/Presentation/Work-IA.WebApi/Work-IA.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Presentation/Work-IA.WebApi"
RUN dotnet build "Work-IA.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Work-IA.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Work-IA.WebApi.dll"]
