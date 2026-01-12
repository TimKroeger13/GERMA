FROM nginx:alpine
# COPY /WebClient/GERMAG/index.html /usr/share/nginx/html/index.html
# COPY /WebClient/GERMAG/js.js /usr/share/nginx/html/js.js
# COPY /WebClient/GERMAG/styles.css /usr/share/nginx/html/styles.css
# COPY /WebClient/GERMAG/pic /usr/share/nginx/html/pic
# # COPY /WebClient/GERMAG/video /usr/share/nginx/html/video
# COPY /WebClient/GERMAG/Information.html /usr/share/nginx/html/Information.html

COPY /WebClient/GERMAG /usr/share/nginx/html

# Expose the default HTTP port
EXPOSE 80