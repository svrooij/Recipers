---
theme: quantum
layout: section
Title: Protect against MITM attacks 🛡  ️
---

# Protect against MITM attacks 🛡️

---
layout: default
transition: slideLeft
---

# Protect against MITM attacks 🛡  

## Force HTTPS for metadata ✅

```csharp
.AddJwtBearer(options =>
{
    // Bind the JWT section in the configuration to JwtBearerOptions
    builder.Configuration.Bind("JWT", options);

    // Override some options we don't want to set in appsettings.json
    options.TokenValidationParameters.ValidateIssuer = false; // Disable issuer validation for common endpoint
    options.TokenValidationParameters.ValidateAudience = true; // Enable audience validation
    
    // 👇 Add this line
    options.RequireHttpsMetadata = true; // Require HTTPS for token validation
});
```

---
layout: default
transition: slideLeft
---

# Protect against MITM attacks 🛡  

## Force start of URL ✅

```csharp
.AddJwtBearer(options =>
{
    // Bind the JWT section in the configuration to JwtBearerOptions
    builder.Configuration.Bind("JWT", options);
    ...    
    options.RequireHttpsMetadata = true; // Require HTTPS for token validation
    // 👇 Add this block
    if (!options.Authority!.StartsWith("https://login.microsoftonline.com/", StringComparison.OrdinalIgnoreCase))
    {
      throw new InvalidOperationException("Authority must start with 'https://'");
    }
});
```

---
layout: two-columns
transition: slideLeft
---

# Protect against MITM attacks 🛡  

## Block https proxies 💣

See: https://svrooij.io/2024/02/22/nuke-man-middle-attack/

::right::

```csharp
// 👇 Add this block
options.BackchannelHttpHandler = new HttpClientHandler
{
  // No default ssl errors and root cert is DigiCert Global Root G2
  ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
    errors == System.Net.Security.SslPolicyErrors.None && chain!.ChainElements.Last().Certificate.Thumbprint == "A8985D3A65E5E5C4B2D7D66D40C6DD2FB19C5436"
};
```