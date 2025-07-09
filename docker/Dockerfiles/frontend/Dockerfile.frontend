### Stage 1: Build React app

# Use the Node.js image for building the app
FROM node:24.2-alpine AS build

# Set the working directory in the container
WORKDIR /src

# Copy the source code
COPY frontend/ChorukoWeb/ ./

# Install dependencies
RUN npm install

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
