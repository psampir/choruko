## Stage 1: Build the .NET EF migrations bundle

# Use the .NET SDK image for building the EF migrations bundle
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

# Set the working directory in the container
WORKDIR /src

# Determine target architecture
ARG TARGETARCH
RUN case ${TARGETARCH} in \
  "amd64")  ARCH=musl-x64 ;; \
  "arm64")  ARCH=musl-arm64 ;; \
  "arm")    ARCH=musl-arm ;; \
  *)        ARCH=musl-x64 ;; \
  esac && \
  echo "Building for architecture: linux-${ARCH}" && \
  echo "linux-${ARCH}" > /tmp/target-arch

# Copy the csproj file 
COPY backend/ChorukoAPI/ChorukoAPI.csproj ./

# Install dependencies
RUN dotnet restore

# Copy the source code
COPY backend/ChorukoAPI/ ./

# Install the Entity Framework Core tools
RUN dotnet tool restore

# Ensure the .NET tools are in the PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Build the EF migrations bundle (self-contained)
RUN TARGET_ARCH=$(cat /tmp/target-arch) && \
  dotnet ef migrations bundle --self-contained -r ${TARGET_ARCH} -o /app/efbundle

### Stage 2: Run migrations and provide admin tools

# Use a lightweight alpine image
FROM alpine:3.24

# Add architecture labels for better tracking
ARG TARGETARCH
LABEL architecture=${TARGETARCH}

# Copy admin tools, EF migrations bundle, and entry point script
COPY docker/Dockerfiles/manager/admin-tools/ /app/admin-tools/
COPY docker/Dockerfiles/manager/run-manager.sh /app/
COPY --from=build /app/efbundle /app/efbundle

# Set permissions
RUN chmod +x /app/admin-tools/*.py /app/efbundle /app/run-manager.sh

# Install Python and pip
RUN apk add --no-cache python3 py3-pip && \
  ln -sf python3 /usr/bin/python

# Create and use a virtual environment for Python packages
RUN python3 -m venv /app/venv && \
  /app/venv/bin/pip install --no-cache-dir -r /app/admin-tools/requirements.txt

# Set the working directory
WORKDIR /app

# Update the entry point to include the venv activation
ENV PATH="/app/venv/bin:${PATH}"

# Set up globalization support in .NET
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Run migrations and keep container alive to provide admin tools
CMD ["./run-manager.sh"]