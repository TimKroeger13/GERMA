# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all files into the container
COPY . .

# New addition: Generates a .version file with the latest commit info
RUN git log -1 --format="%cd - %s" --date=iso > .version

# Ensure setup.sh has execution permissions
RUN chmod +x Confluence/Install/setup.sh && /bin/bash Confluence/Install/setup.sh

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy files from the build stage
COPY --from=build /app /app

# Ensure run.sh has execution permissions
RUN chmod +x Confluence/Install/run.sh

# Set the entrypoint
EXPOSE 443
EXPOSE 80
ENTRYPOINT [ "/bin/bash", "Confluence/Install/run.sh" ]