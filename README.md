![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![Alpine Linux](https://img.shields.io/badge/Alpine_Linux-%230D597F.svg?style=for-the-badge&logo=alpine-linux&logoColor=white) 
![Wireguard](https://img.shields.io/badge/wireguard-%2388171A.svg?style=for-the-badge&logo=wireguard&logoColor=white)
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
# rest2wireguard
### REST2WG is an Alpine-based WebAPI-managed Wireguard server.
<br/>

## Quick start:
    docker run --cap-add NET_ADMIN -p 51850:51820/udp -p 51851:5000/tcp --env REST2WG_ALLOW_NOAUTH=true --env REST2WG_HANDSHAKE_AGO_LIMIT=0 luminodiode/rest2wireguard
You will get the image listening 51850 by Wireguard and 51851 by WebAPI without authorization request header required. Dont forget to allow it in your UFW if there is some:

    ufw allow 51850/udp && ufw allow 51851/tcp
    
Now you can access next endpoints:
- **GET /api/peers** - get list of all peers
- **PUT /api/peers** - add new peer
- **DELETE /api/peers** - remove existing peer

Where for each POST/PUT/DELETE endpoint you must provide application/json body in the next format:

    {
        "publicKey":"LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8="
    }
    
    
And the response format for peers POST and PUT endpoints will be:

    {
        "peerPublicKey": "LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8=",
        "allowedIps": "10.6.0.29/32",
        "interfacePublicKey": "3CTcRIiXHORtMLdYJ0F1AqsQITiH6WEPXZxHjuMgqDY="
    }
    
Now you can connect wireguard client to your server. Example configuration:

    [Interface]
    PrivateKey = YBth+re5L1qqO+O6kjB72RbZUZMmu8KxL0ppjyGyfmk=
    Address = 10.6.0.29/32
    DNS = 8.8.8.8

    [Peer]
    PublicKey = 3CTcRIiXHORtMLdYJ0F1AqsQITiH6WEPXZxHjuMgqDY=
    AllowedIPs = 0.0.0.0/0
    Endpoint = YOUR.SERVER.IP.ADDRESS:51850

