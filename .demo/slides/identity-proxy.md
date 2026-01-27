---
layout: section
title: Identity proxy 🔐
---

# 🔑 Introducing IdentityProxy 🔑

## Men-in-the-middle for OpenID Connect

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
title: Inject extra signing keys
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

# Inject extra signing keys

1. Modify the `authority` to point to the proxy
1. Act as a proxy for OpenID Configuration
1. Change the `jwks_uri` to point to the proxy
1. Act as a proxy for JWKS
1. Inject extra signing keys into the JWKS response

---
layout: two-columns
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
transition: slideUp
---

# Introducing IdentityProxy

- Open-source project
- Docker container
- **TestContainer support** for integration tests
- Tested with Entra ID and Azure AD B2C
- DO NOT use in production 💣
 
https://github.com/svrooij/identityproxy/

---
layout: two-columns
title: Inject extra signing keys
transition: slideUp
---

```mermaid
sequenceDiagram
  actor U as Tester
  participant Api as API
  participant P as Proxy
  participant En as Entra ID

  U ->> P: Give me a token
  P -->>P: Have signing keys cached?
  P->> En: .../.well-known/openid-configuration
  En->> P: OpenID Configuration
  P->> En: .../discovery/v2.0/keys
  En->> P: Public JWT keys
  P -->>P: Generate extra signing key
  P ->>U: Token signed with extra key
  U ->> Api: Request with token
  Api -->>Api: Have signing keys cached?
  opt OpenID Configuration & JWKS
  Api->> P: .../.well-known/openid-configuration
  P->> Api: OpenID Configuration (modified jwks_uri)
  Api->> P: .../proxy/jwks
  P->> Api: JWT keys + 1 extra
  end
  Api -->>Api: Validate token against keys
  Api -->>Api: Validate lifetime & ....
  Api->> U: Response
```

::right::

# Get token during tests

1. Ask the proxy for a token with these claims
1. Proxy fetches OpenID Configuration and JWKS
1. Proxy generates an extra signing key
1. Proxy signs the token with the extra key
1. Proxy returns the token to the test
1. API validates the token as usual


---
layout: section
transition: slideUp
---

# 🔑 IdentityProxy 🔑

## 🧑‍💻 Demo time 🧑‍💻
