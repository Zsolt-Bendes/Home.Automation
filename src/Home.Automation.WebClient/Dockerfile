FROM nginx:alpine

# Remove default config
RUN rm /etc/nginx/conf.d/default.conf

# Add your custom config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy Blazor WASM published files
COPY dist/wwwroot /usr/share/nginx/html

EXPOSE 80