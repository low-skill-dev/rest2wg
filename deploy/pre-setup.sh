if ! test -e "/etc/wireguard/privatekey"; then
    echo "Wireguard privatekey file not detected. Installing wg..."
    apk add wireguard-tools
    cp /etc/wireguard/pre-wg0.conf /etc/wireguard/wg0.conf
    wg genkey > /etc/wireguard/privatekey
    cat /etc/wireguard/privatekey >> /etc/wireguard/wg0.conf
    echo "Installing dotnet-runtime & aspnetcore7-runtime..."
    apk add aspnetcore7-runtime

    echo "Copying the WebAPI build..."
    cp /home/node_api_ro /home/node_api -r
fi

# remove if release
apk add tcpdump
apk add curl
apk add wget
apk add git
# end if

echo "Spinning up the Wireguard service..."
wg-quick up wg0 && wg show wg0
echo "Spinning up the ASP WebAPI..."
#dotnet /home/node_api/vdb_node_api.dll -no-launch-profile --urls "http://192.168.6.1:5001"


apk add dotnet7-sdk
apk add dotnet7-build
/bin/sh -c "cd /home && git clone https://github.com/LuminoDiode/vdb_vpn_server"
dotnet run --project /home/vdb_vpn_server/vdb_node_api/vdb_node_api.csproj -c "Release" --no-launch-profile --urls "http://192.168.6.1:5001"



echo "Setup completed."
tail -f /dev/null