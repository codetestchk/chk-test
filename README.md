# chk-test

.NET Challenge. A simple Payments gateway which allows a Merchant to sign up ( create an account ), post a new payment ( which is then forwarded to a simulated bank ), and query a previously created payment.

## Usage

### Requirements

1. .net core 3.1
2. Access to a SqlServer database ( I used a docker SqlServer image - see 'Sql Server Docker Setup' section within this Readme )
3. .net core ef CLI tools in order to execute database migrations

### Running Application

_If you are more comfortable in running/testing/executing using Visual Studio then you are free to use that method, I will write instructions here mainly using command line interface_

#### 1. Build application

```bash
cd ChkDemo
dotnet build
```
Solution should restore all nuget packages and build successfully

#### 2. Update/Create Sql Server database

I have chosen _not_ to include automatic migrations within the Gateway API upon first-run, therefore we need to create the Sql Server database before the Gateway will run.

_( I prefer migrations to take place within the CI/CD pipeline rather than have the end-product execute the database migrations )_

From Solution root folder:

```bash
dotnet ef database update --project ChkDatabase
```

Please note, you should check the SqlServer connection string used in `ChkDatabase\appsettings.json` and alter if necessary, specifically the password, port, SQL Server host name

#### 3. Run ChkGateway

We first need to run the gateway project application `ChkGateway` which will launch an IIS Express instance, and bind the application to ports 5000 ( http ) and 5001 ( https ).

_These launch settings are described ( and checked into source control ) in the location: `ChkDemo\ChkGateway\Properties\launchSettings.json`_

Run the application using command line ( or visual studio GUI ):

```bash
dotnet run --project ChkGateway
```

From here the API is accessible by using a HttpUtility such as PostMan, however in order to simulate/test the API easily, a simulator can be run to ensure the application behaves as expected

#### 4. Run ChkMerchantSimulator

The merchant simulator registes a number of Merchants ( 5 ) with the Gateway - this is akin to 'signing up' with the Payments gateway. Merchant ID and API Key ( password ) are given back to the Merchant. The Merchant then uses these credentials to Authenticate ( bearer token auth ) with the Gateway API on subsequent calls to the API.

One the Merchant's are setup, the simulator can post a new payment ( a random merchant is selected ) to the payments platform. The payments API returns a unique Payment ID, and a success/failure message.

_Note : the 'Fake Bank' is set to return a 'failure to process payment' message randomly in 10% of requests._

After posting a new payment, the simulated merchant then does a `GetExistingPayment` call to retrieve details of an existing payment. All payments submitted to the payments platform are stored in the Sql Server database for future recovery/reading.

To run the ChkMerchantSimulator:

```bash
dotnet run --project ChkMerchantSimulator
```

The instructions/info is then available within the console/terminal window, hitting the enter key posts a new payment to the payments API, typing 'exit' exits the applicaiton.

### Sql Server Docker Setup

```bash
sudo docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=INSERT_LOCAL_PASSWORD_HERE123!" -p 1433:1433 --name localdocsql -h localdocsql -d mcr.microsoft.com/mssql/server:2019-latest
```
(remove sudo on windows)


You should use whatever password you wish, but make sure it is complex enough or the docket container will not start, also note the port binding ( you may already have SQL Express or something running using 1433.

## Limitations/TOOD's

*TODO's - all of which are listed as // TODO within the code*
1. Card validation is simplified brevity, only validates card number, not any other value, also associated Unit tests
2. Logger is injected into controllers for use, but logger is not called anywhere, typically application logging would take place probably to disk, and or an appropriate logging NoSql database
3. Within the PaymentsController there is a concrete dependency upon HttpContext that I would like to extract away
4. New Merchant - no validation takes place on that

*Architectural TODOs*

1. Small change on binding of `IBankServiceProxy` currently is hard-bound in code to the `FakeBankServiceProxy`, ideally this would be decided upon ( real or fake ) on either a: appsettings.json configuration or b: environment settings
2. Synchronicity betwen Payment service and payments, currently the PostNewPayment within the PaymentsController holds open a Http session waiting on a response from the bank. Typically I would expect this would be more asynchronous, as in posting a bank request, then checking back on it at a later date ( 5 seconds later or so... ) or posting a callback API endpoint to the Bank Provider for them to post information to the Payments API, rather than holding a http session call open whilst the bank replies.
3. Performance on database - very little perf work has been done on the database, indexes, query optimization etc
4. Card information is stored in plain text, this is a no-no, and so data would need to be encrypted at rest

## Architectural considerations/notes

### Scaling

The API can easily be containerized as the only common resource is the database ( which has ACID behaviours given that it is a SqlServer DB ), even the authentication is a scalable - server doesn't store session data ( so long as the secret key is shared ).
