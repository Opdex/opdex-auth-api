
## Running the API

### Dependencies

#### Required
- .NET 6
- Stratis Cirrus full node
- Opdex Auth UI
- Azure Subscription
- Azure SignalR service
- Azure Key Vault service
- MySQL server

### Getting Started

1. Set up configuration values. These can be configured as either environment variables, user secrets or key vault secret.

| Configuration Key                      | Description                                                            | Secure |
|----------------------------------------|------------------------------------------------------------------------|--------|
| ApplicationInsights:InstrumentationKey | Optional application insights instrumentation key, for collecting logs | No     |
| Encryption:Key                         | A random 16-octet secret used for encryption sensitive data            | Yes    |
| Jwt:SigningKeyName                     | Azure certificate secret name                                          | No     |
| Jwt:SigningKeyVersion                  | Azure certificate secret version string                                | No     |
| Database:ConnectionString              | MySQL auth API database connection string	                             | Yes    |
| CirrusConfiguration:ApiUrl             | Base URL for the cirrus full node API                                  | 	No    |
| CirrusConfiguration:ApiPort            | Port number used to access the cirrus full node API                    | 	No    |
| Azure:SignalR:ConnectionString         | ConnectionString for Azure SignalR instance                            | 	Yes   |
| Azure:KeyVault:Name                    | Name of the Azure key vault used for storing secrets                   | 	No    |
| Authority                              | Base URL of the hosted API, used as the JWT issuer value               | 	No    |
| Prompt                                 | Base URL of the auth UI QR code prompt                                 | 	No    |
| CommitHash                             | Commit hash of the currently deployed code, ideally set by CI/CD pipeline | 	No    |

2. Ensure all dependencies are set up and configured correctly
3. Run the API using `dotnet run`