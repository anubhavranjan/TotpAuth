# TotpAuth
Tiny library that allows you to Generate and Check TOTP. Based on TwoStepsAuthenticator https://github.com/glacasa/TwoStepsAuthenticator

### Usage
```c#
// Generate Secret
TotpAuth.Authenticator.GenerateKey() 
// Example: 5PC4TPFPQPX7HPOS

// Generate OTP
var totp = new Totp.Authenticator();
totp.GetCode("SECRETKEY"); 
// Example: 788644

// Validate Code
var totp = new Totp.Authenticator();
totp.CheckCode("5PC4TPFPQPX7HPOS", "788644"); 
// Example: true
 
```
