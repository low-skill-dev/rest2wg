if ! test -e "/etc/wireguard/wg0.conf"; then
    echo "Wireguard configuration file not detected. Generating..."
    cp /etc/rest2wg/pre-wg0.conf /etc/wireguard/wg0.conf
    wg genkey >> /etc/wireguard/wg0.conf
fi

echo "Generating self-signed x509 sertificate..."
openssl req -x509 -nodes -days 3650 -newkey rsa:2048 -keyout /etc/ssl/private/nginx-selfsigned.key -out /etc/ssl/certs/nginx-selfsigned.crt
echo "Spinning up the nginx reverse-proxy..."
nginx
echo "Spinning up the Wireguard service..."
wg-quick up wg0 && wg show wg0
echo "Spinning up the ASP WebAPI..."
dotnet /app/vdb_node_api.dll -no-launch-profile

tail -f /dev/null