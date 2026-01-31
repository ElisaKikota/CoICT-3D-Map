FROM nginx:alpine

# Remove default nginx configuration
RUN rm /etc/nginx/conf.d/default.conf

# Copy custom nginx configuration from web subdirectory
COPY web/nginx.conf /etc/nginx/conf.d/

# Copy the pre-built React app from web subdirectory
COPY web/dist/ /usr/share/nginx/html/

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
