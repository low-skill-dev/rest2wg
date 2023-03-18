if ! test -e "/etc/wireguard/wg0.conf"; then
    echo "Wireguard configuration file not detected. Generating..."
    cp /etc/rest2wg/pre-wg0.conf /etc/wireguard/wg0.conf
    wg genkey >> /etc/wireguard/wg0.conf
fi


if ! ((test -e /etc/ssl/private/nginx-selfsigned.key) && (test -e /etc/ssl/certs/nginx-selfsigned.crt)); then
    echo "Self-signed x509 sertificate files not detected. Generating..."
    openssl req -x509 -nodes -days 36500 -newkey rsa:2048 -subj "/CN=US/C=US/L=San Fransisco" -keyout /etc/ssl/private/nginx-selfsigned.key -out /etc/ssl/certs/nginx-selfsigned.crt
fi


if !((REST2WG_LIMIT_REQ > 0)) && ((REST2WG_LIMIT_REQ <= 9999999)); then
    echo "Incorrect value of REST2WG_LIMIT_REQ environment variable was ignored."
    echo "REST2WG_LIMIT_REQ was set to 100k."
    REST2WG_LIMIT_REQ=100000
fi

if ! test -e "/etc/nginx/snippets/nginx-limit_req.conf"; then
    echo "Nginx limit_req configuration file not detected. Generating..."
    cp /etc/rest2wg/pre-nginx-limit_req.conf.template /etc/nginx/snippets/nginx-limit_req.conf
    echo "${REST2WG_LIMIT_REQ}r/s;" >> /etc/nginx/snippets/nginx-limit_req.conf
fi

if (testvar='all' && [[ $REST2WG_ALLOWED_IP = $testvar ]]); then
    unset testvar;
elif !(ipcalc -n "${REST2WG_ALLOWED_IP}"); then
    echo "Incorrect value of REST2WG_ALLOWED_IP environment variable was ignored."
    echo "REST2WG_ALLOWED_IP was set to ALL."
    REST2WG_ALLOWED_IP="all";
fi

if ! test -e "/etc/nginx/snippets/while_list.conf"; then
    echo "Nginx while_list configuration file not detected. Generating..."
    echo "allow ${REST2WG_ALLOWED_IP}; deny all;" > /etc/nginx/snippets/while_list.conf
fi


echo "Spinning up the Nginx reverse-proxy..."
nginx
echo "Spinning up the Wireguard service..."
wg-quick up wg0 && wg show wg0
echo "Spinning up the ASP WebAPI..."
dotnet /app/vdb_node_api.dll -no-launch-profile

tail -f /dev/null