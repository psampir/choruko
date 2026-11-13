### Stage 1: Build React app

# Use the Node.js image for building the app
FROM node:26.12-alpine AS build

# Set the working directory in the container
WORKDIR /src

# Copy package files first for better caching
COPY frontend/ChorukoWeb/package.json frontend/ChorukoWeb/package-lock.json ./

# Install dependencies - this will be cached unless package files change
RUN npm install

# Copy the rest of the source code - changes here won't invalidate npm install cache
COPY frontend/ChorukoWeb/ ./

# Build the app
RUN npm run build

#### Stage 2: Serve the React app with Nginx

# Use a lightweight Nginx image for serving the app
FROM nginx:1.28.0-alpine-slim

# Copy the built app from the build stage
COPY --from=build /src/dist /usr/share/nginx/html

# Copy Nginx configuration file
COPY docker/Dockerfiles/frontend/nginx.conf /etc/nginx/nginx.conf

# Expose the port the app runs on
EXPOSE 80

# Run Nginx on container start
CMD ["nginx", "-g", "daemon off;"]
