### Stage 1: Build ASP.NET Core app

# Use the .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:11.0-alpine AS build

# Set the working directory in the container
WORKDIR /src

# Copy the csproj file
COPY backend/ChorukoAPI/ChorukoAPI.csproj ./ChorukoAPI.csproj

# Install dependencies
RUN dotnet restore ./ChorukoAPI.csproj

# Copy the source code
COPY backend/ChorukoAPI/ ./

# Build the app
RUN dotnet publish -c Release -o out

### Stage 2: Run ASP.NET Core app

# Use a lightweight ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine

WORKDIR /app

# Set up globalization support in .NET
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy the built app from the build stage
COPY --from=build /src/out .

# Expose the port the app runs on
EXPOSE 8080

# Run the app on container start
ENTRYPOINT ["dotnet", "ChorukoAPI.dll"]