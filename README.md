![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![Alpine Linux](https://img.shields.io/badge/Alpine_Linux-%230D597F.svg?style=for-the-badge&logo=alpine-linux&logoColor=white) 
![Wireguard](https://img.shields.io/badge/wireguard-%2388171A.svg?style=for-the-badge&logo=wireguard&logoColor=white)
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![License - GPL v3](https://img.shields.io/badge/License-GPL_3-2ea44f?style=for-the-badge)
![Stage - ALPHA](https://img.shields.io/badge/stage-alpha-ADD8E6?style=for-the-badge)
# rest2wireguard
### REST2WG is an Alpine-based WebAPI-managed Wireguard server.
<br/>

## Quick start:
    docker run --cap-add NET_ADMIN -p 51850:51820/udp -p 51851:5000/tcp --env REST2WG_ALLOW_NOAUTH=true luminodiode/rest2wireguard
You will get the image listening 51850 by Wireguard and 51851 by WebAPI without authorization request header required. Dont forget to allow it in your UFW if there is some:

    ufw allow 51850/udp && ufw allow 51851/tcp
    
Now you can access next endpoints:
- **GET /api/peers** - get list of all peers
- **POST /api/peers** - get info of peer
- **PUT /api/peers** - add new peer
- **DELETE /api/peers** - remove existing peer

Where for each POST/PUT/DELETE endpoint you must provide application/json body in the next format:

    {
        "publicKey":"AjSn6JHWRiGcllTCaqOPTst1WpPUb//5O3aG/qD1nkM="
    }
    
    
And the response format for peers POST and PUT endpoints will be:

    {
        "peerPublicKey": "AjSn6JHWRiGcllTCaqOPTst1WpPUb//5O3aG/qD1nkM=",
        "allowedIps": "10.6.0.0/32",
        "interfacePublicKey": "3CTcRIiXHORtMLdYJ0F1AqsQITiH6WEPXZxHjuMgqDY="
    }

By default, rest2wg checks for disconnected peers every <ins>1 hour</ins> and removes all peers which handshake was more than <ins>600 seconds</ins> ago. To prevent this, set environment variable REST2WG_HANDSHAKE_AGO_LIMIT to 0 or specifie any value in seconds you want your server to delete peers after.
