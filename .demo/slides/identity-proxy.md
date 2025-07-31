---
theme: quantum
layout: section
title: Identity proxy 🔐
---

# 🥁 Introducing IdentityProxy 

---
layout: two-columns
transition: slideUp
title: Doing a request with token
---

```mermaid
sequenceDiagram
  actor U as User
  participant Api as API
  participant En as Entra ID

  U ->> Api: Request with token
  Api -->>Api: Have signing keys cached?
  opt OpenID Configuration & JWKS
  activate En
  Api->> En: .../.well-known/openid-configuration
  En->> Api: OpenID Configuration
  Api->> En: .../discovery/v2.0/keys
  En->> Api: Public JWT keys
  deactivate En
  end
  Api -->>Api: Validate token against keys
  Api -->>Api: Validate lifetime & ....
  Api->> U: Response
```

::right::

# Token validation

1. Have signing keys cached?
1. If not, fetch OpenID Configuration
1. Fetch JWKS (JSON Web Key Set)
1. Validate token against keys
1. Validate lifetime & other claims

---
layout: two-columns
transition: slideUp
title: Inject extra signing keys
---

```mermaid
sequenceDiagram
  actor U as User
  participant Api as API
  participant P as Proxy
  participant En as Entra ID

  U ->> Api: Request with token
  Api -->>Api: Have signing keys cached?
  opt OpenID Configuration & JWKS
  Api->> P: .../.well-known/openid-configuration
  P->> En: .../.well-known/openid-configuration
  En->> P: OpenID Configuration
  P->> Api: OpenID Configuration (modified jwks_uri)
  Api->> P: .../proxy/jwks
  P->> En: .../discovery/v2.0/keys
  En->> P: Public JWT keys
  P->> Api: JWT keys + 1 extra
  end
  Api -->>Api: Validate token against keys
  Api -->>Api: Validate lifetime & ....
  Api->> U: Response
```

::right::

# Inject extra signing keys

1. Modify the `authority` to point to the proxy
1. Act as a proxy for OpenID Configuration
1. Change the `jwks_uri` to point to the proxy
1. Act as a proxy for JWKS
1. Inject extra signing keys into the JWKS response

---
layout: section
transition: zoomIn
---

# Introducing IdentityProxy 🔑

---
layout: default
transition: slideUp
---

# Introducing IdentityProxy 🔑

- Open-source project
- https://github.com/svrooij/identityproxy/
- Docker container
- **TestContainer support** for integration tests
- Tested with Entra ID and Azure AD B2C

## Demo Time 🧑‍💻
