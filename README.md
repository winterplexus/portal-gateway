Portal Gateway for .NET
=======================

Portal gateway is a forward proxy server for authenticating and authorizing users to a private site (i.e., a portal gateway for users who do not need to know the address or URL of the private site).

Portal gateway is based on .NET Framework platform (4.8) using the following components:

* Portal Gateway

* Portal Gateway Module

* Portal Gateway User Roles Server

## Portal Gateway
                
- Authenticates users using credentials against one or more Active Directory (AD) domains.
- Authorizes users using comma-delimited list of roles returned from user roles server.
- Writes activities to log file.
- Writes exceptions to Windows Event Log file.

## Portal Gateway Module
                
- Creates a TLS connection to the private site using a certifcate installed on portal gateway server.
- Forwards HTTP requests and responses to private site.
- Injects the following HTTP request headers:
  - X-Forwarded-For (portal gateway server IP address)
  - X-Forwarded-Host (private site hostname)
  - UserId (Active Directory username)
  - UserRole (comma-delimited list of roles from portal gateway user roles server)
  - RequestServer (portal gateway server IP address)
  - CompanyName (company name from configuration file)
  - CompanyRequestor (company requestor from configuration file)
  - CompanyGeneratedIdentifier (generated identifier using current date time in clock ticks).
- Writes events to log file.
- Writes network data (HTTP) to log file (for diagnostics purposes only).
- Writes exceptions to Windows Event Log file.

## Portal Gateway User Roles Server

- Returns comma-delimited list of roles based on given Active Directory username.
- Writes exceptions to Windows Event Log file.             

### Project Components

| Name                         | Technology                                                     |
| ---                          | ---                                                            |
| PortalGateway                | ASP.NET MVC (Model View Controller) application server         |
| PortalGatewayModule          | Custom IIS (Internet Information Services) HTTP module         |
| PortalGatewayUserRolesServer | Windows Communication Foundation (WCF) REST-ful service server |
