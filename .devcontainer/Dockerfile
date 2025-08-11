FROM alpine:3.18

# Download Dapr binaries
RUN apk add --no-cache curl && \
    mkdir -p /usr/local/bin && \
    curl -L https://github.com/dapr/dapr/releases/download/v1.15.6/placement_linux_amd64.tar.gz | tar -xz -C /usr/local/bin && \
    curl -L https://github.com/dapr/dapr/releases/download/v1.15.6/scheduler_linux_amd64.tar.gz | tar -xz -C /usr/local/bin && \
    chmod +x /usr/local/bin/placement /usr/local/bin/scheduler && \
    apk del curl

EXPOSE 50005 50006

# Run both services
CMD placement --port 50005 & scheduler --port 50006 & wait