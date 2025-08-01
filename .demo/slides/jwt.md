---
theme: quantum
layout: section
title: How does JWT Authentication work? 🔐
---

# How does JWT Authentication work? 🔐

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
  opt Load oidc and jwks (+/- 230 ms)
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
layout: image-right
image: .demo/slides/entra-openid-configuration.png
transition: slideUp
---

# OpenID Configuration

## Details about IDP

- Issuer
- Authorization endpoint
- Token endpoint
- JWKS URI
- ...

---
layout: image-right
image: .demo/slides/entra-jwks.png
transition: slideUp
---

# JSON Web Key Set (JWKS)

Just a bunch of public keys that can be used to validate JWTs.

---
layout: section
title: Configure JWT Authentication in .NET
---

# Configure JWT Authentication in .NET

## 🧑‍💻 Demo Time! 🧑‍💻
