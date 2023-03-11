[![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)](https://hub.docker.com/repository/docker/luminodiode/rest2wireguard)
[![Alpine Linux](https://img.shields.io/badge/Alpine_Linux-%230D597F.svg?style=for-the-badge&logo=alpine-linux&logoColor=white)](https://www.alpinelinux.org)
[![Wireguard](https://img.shields.io/badge/wireguard-%2388171A.svg?style=for-the-badge&logo=wireguard&logoColor=white)](https://www.wireguard.com)
[![Nginx](https://img.shields.io/badge/nginx-%23009639.svg?style=for-the-badge&logo=nginx&logoColor=white)](https://nginx.org)
[![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)](https://asp.net)
# rest2wireguard
### Alpine-based TLS-securely WebAPI-managed Wireguard server.
<br/>

## Quick start:
    docker run --cap-add NET_ADMIN -p 51850:51820/udp -p 51851:51821/tcp --env REST2WG_ALLOW_NOAUTH=true --env REST2WG_HANDSHAKE_AGO_LIMIT=0 luminodiode/rest2wireguard
You will get the image listening 51850 by Wireguard and 51851 by WebAPI with TLS encryption without authorization request header required. You can also expose 51822 port if you want to make requests without TLS encryption. Dont forget to allow ports in your UFW if there is one:

    ufw allow 51850/udp && ufw allow 51851/tcp
    
Now you can access next endpoints:
- **GET /api/peers** - get list of all peers
- **PUT /api/peers** - add new peer
- **DELETE /api/peers** - remove existing peer

Where for PUT and DELETE endpoints you must provide application/json body in the next format:

    {
        "publicKey":"LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8="
    }
    
    
And the response format for PUT endpoint will be:

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
#### If you want to use authorization
You need to leave REST2WG_ALLOW_NOAUTH unset (false is the default value). Then generate the key and its hash. You can [open this url](https://dotnetfiddle.net/ldbnVB), click 'Run' and copy generated key-hash pair or run [this code](ApiKeyGenerator/Program.cs) on your machine locally. You will get the next output:

    Key base64 (for client):
    zXCzMDwZt/bMTrT+rt08cH6XH+ut61LJRKEa+OLRJEMgegUv4HLxp9sB1+FnKJYkImn7Sh64eDRs1PtwV5ptmQ==

    Key hash base64 (for server):
    EbnejPeYabvB709y/3a/ubyUHqiCwjJqLWw0PE0AzSDTxHF+fXrKIagzSBKMF/2pwkrKk2KUhUNm6mhyUajFlA==
    
The first value is the key itself, base64 encoded. You will need to pass it with your requests Authorization header in the next format:

    Authorization: Basic zXCzMDwZt/bMTrT+rt08cH6XH+ut61LJRKEa+OLRJEMgegUv4HLxp9sB1+FnKJYkImn7Sh64eDRs1PtwV5ptmQ==
    
The second value is the key hash, base64 encoded. You will need to pass it to your docker container with the enviroment variable (if you want to pass it using docker secrets - read the full reference below):

    --env REST2WG_AUTH_KEYHASH_BASE64=EbnejPeYabvB709y/3a/ubyUHqiCwjJqLWw0PE0AzSDTxHF+fXrKIagzSBKMF/2pwkrKk2KUhUNm6mhyUajFlA==
   
Be aware, the data is not encrypted here, just encoded. If you are using auth you should pass your requests to the 51821 port only, using TLS.
