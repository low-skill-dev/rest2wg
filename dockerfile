FROM alpine:3 AS base

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY ./vdb_node_api /vdb_node_api
COPY ./vdb_node_wireguard_manipulator /vdb_node_wireguard_manipulator
RUN dotnet publish /vdb_node_api/vdb_node_api.csproj -c "Release" -r linux-musl-x64 --no-self-contained -o /app/publish

FROM base AS final
COPY ./build_alpine/pre-wg0.conf ./etc/wireguard/pre-wg0.conf
COPY ./build_alpine/pre-setup.sh ./etc/wireguard/pre-setup.sh
COPY --from=build /app/publish /app
RUN apk add -q --no-progress wireguard-tools
RUN apk add -q --no-progress aspnetcore7-runtime
ENV ASPNETCORE_ENVIRONMENT=Production

CMD ["sh", "-c", "umask 077 && chmod +x /etc/wireguard/pre-setup.sh && /etc/wireguard/pre-setup.sh"]