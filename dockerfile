FROM alpine:3 AS base



FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
COPY ./vdb_node_api /vdb_node_api
COPY ./vdb_node_wireguard_manipulator /vdb_node_wireguard_manipulator
RUN dotnet publish /vdb_node_api/vdb_node_api.csproj -c "Release" -r linux-musl-x64 --no-self-contained -o /app/publish



FROM base AS final

RUN apk add -q --no-progress nginx
RUN apk add -q --no-progress openssl
RUN apk add -q --no-progress wireguard-tools
RUN apk add -q --no-progress aspnetcore7-runtime

COPY --from=build /app/publish /app
COPY ./build_alpine/pre-setup.sh ./etc/rest2wg/pre-setup.sh
COPY ./build_alpine/pre-wg0.conf ./etc/rest2wg/pre-wg0.conf
COPY ./build_alpine/pre-nginx-limit_req.conf.template ./etc/rest2wg/pre-nginx-limit_req.conf.template
COPY ./build_alpine/pre-nginx.conf/ ./etc/nginx/nginx.conf
COPY ./build_alpine/pre-nginx-ssl-params.conf ./etc/nginx/snippets/ssl-params.conf
COPY ./build_alpine/pre-nginx-self-signed.conf ./etc/nginx/snippets/self-signed.conf

ENV ASPNETCORE_ENVIRONMENT=Production
ENV REST2WG_LIMIT_REQ=100000
ENV REST2WG_ALLOWED_IP='all'
CMD ["bash", "-c", "umask 077 && chmod +x /etc/rest2wg/pre-setup.sh && /etc/rest2wg/pre-setup.sh"]