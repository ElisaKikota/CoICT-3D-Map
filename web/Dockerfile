FROM node:18-alpine AS builder

WORKDIR /app

# Copy package files first for better caching
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the source code
COPY . .

# Debug: Show what's in the directory
RUN ls -la
RUN echo "Node version:" && node --version
RUN echo "NPM version:" && npm --version

# Check if build script exists
RUN npm run

# Try building with explicit error handling
RUN npm run build || (echo "Build failed, checking logs..." && ls -la && exit 1)

# Production stage
FROM nginx:alpine

# Remove default nginx configuration
RUN rm /etc/nginx/conf.d/default.conf

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/

# Copy the built React app from builder stage
COPY --from=builder /app/dist/ /usr/share/nginx/html/

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
