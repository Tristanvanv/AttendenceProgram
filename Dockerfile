# 1) Build stage: app compileren en publiceren
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet publish -c Release -o /app/out

# 2) Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
# startscript zorgt dat ASP.NET luistert op de poort die Render geeft
COPY start.sh .
RUN chmod +x start.sh
# niet verplicht voor Render, maar handig als default
EXPOSE 10000
CMD ["./start.sh"]
