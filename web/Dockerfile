FROM nginx:alpine

# Copy the built React app
COPY dist/ /usr/share/nginx/html/

# Copy custom nginx configuration for Unity WebGL MIME types
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
