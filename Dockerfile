FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ITECH-Auto-Attendance.csproj", "./"]
RUN dotnet restore "ITECH-Auto-Attendance.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "ITECH-Auto-Attendance.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ITECH-Auto-Attendance.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ITECH-Auto-Attendance.dll"]
