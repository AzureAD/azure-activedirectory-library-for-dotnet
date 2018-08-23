### Strong-naming general notes

1. Strong naming is NOT a security feature
2. Signing and strong naming are orthogonal. Signing (e.g. with Authenticode) is a security feature.
3. Changing the strong naming key on a library is discouraged because the public key becomes part of the name of the assembly. The name of an assembly is used in many configuration blocks, for example assmbly binding and assembly redirects.
4. When strong naming an assembly, it can be done in 2 ways: 

a. use an snk file that has both a public and a private key (MSAL)
b. use an snk file that has a public key only (ADAL) - in this case a technique called delay signing is used

When publishing to Open Source, it is recommended to publish in the repo both public and private keys, so that other developers can fork the repo and build their own version.

### Strong Naming ADAL and MSAL

For historical reasons, ADAL is signed with a public only MS key. 
MSAL however is now signed with using an .snk file containing both public and private keys. 

