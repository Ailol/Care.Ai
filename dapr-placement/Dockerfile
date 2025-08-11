FROM alpine:latest

# Download Dapr binaries directly - matching sidecar version 1.15.6
RUN apk add --no-cache curl supervisor && \
    mkdir -p /dapr/bin && \
    curl -L https://github.com/dapr/dapr/releases/download/v1.15.6/daprd_linux_amd64.tar.gz | tar -xz -C /dapr/bin && \
    curl -L https://github.com/dapr/dapr/releases/download/v1.15.6/placement_linux_amd64.tar.gz | tar -xz -C /dapr/bin && \
    curl -L https://github.com/dapr/dapr/releases/download/v1.15.6/scheduler_linux_amd64.tar.gz | tar -xz -C /dapr/bin && \
    chmod +x /dapr/bin/*

# Create supervisor config
COPY supervisord.conf /etc/supervisord.conf

EXPOSE 50005 6050 50006 6060

# Run supervisor
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisord.conf"]