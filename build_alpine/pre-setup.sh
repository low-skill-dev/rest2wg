if ! test -e "/etc/wireguard/wg0.conf"; then
    echo "Wireguard configuration file not detected. Generating..."
    cp /etc/wireguard/pre-wg0.conf /etc/wireguard/wg0.conf
    wg genkey >> /etc/wireguard/wg0.conf
fi

echo "Spinning up the Wireguard service..."
wg-quick up wg0 && wg show wg0
echo "Spinning up the ASP WebAPI..."
dotnet /app/vdb_node_api.dll -no-launch-profile

tail -f /dev/null