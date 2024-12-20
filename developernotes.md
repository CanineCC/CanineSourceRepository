C4Sharp
---------------
https://github.com/8T4/c4sharp/blob/main/README.md



## MISSING FEATURES (To be done)
- User handling, login/token, identity (including storing it in IEvent header)
- Ensure unique names for endpoints (i.e. no two features are allowed to have same name within a context)
- GenerateDefaultData from templates: "Hello world" context
- Create a Master db with templates for BpnEventStore (ex. template for a user-context including features - wizard needed for selecting target services/databases)
- Named services with settings (ex. Name:"MyPostgresDb", ConnectionString:".....")
- Add code snippets based on selected service for DI
- Add 'mock' implementation for selected service (with scenario store) to be used in automated testing.
- Add automatic handling of deprecation of endpoints (breaking changes to db model?)
- FRONTEND: Feature   :: Telemetry tab graphs showing (aggregate last 5 minutes, last hour, last day, last week, last year)
- Support: Rerun failed executions / debug
- FRONTEND: Dashboard :: Graphs: Server availability + Context/feature Success rate + Context/feature performance (min, max, avg)
- Include review/approval flow for release draft feature
- Include logic for release "all features" into new/next api version 
- FRONTEND: DRAFT Feature :: Add 'Commit' (i.e. snapshot) that can be reverted to

## LESS BOILERPLATE, SETUP and in general, things to think about
- Client throttling enabled
- Extensive logging 
- Response caching
- Automatic Dependency Injection
- Automatic Service registration
- BDD built in, with unit tests and integration tests
- Automatic C4 documentation (https://en.wikipedia.org/wiki/C4_model)
- Process map (Documentation for ISO Certification) in the form of a BPN diagram artifact is given
- Automatically API versioning
- Automatically change-log for change-management is preserved
- Gracefully (automatically) handle deprecation of API endpoints
- Automatically generated (structured and versioned) open-api documentation of api
- Service dashboard with breakdown into context, feature and even task level to identify issues.
- Debug graph with full log for failed nodes based on correlation id for efficient support handling and debug
- Automatic Api benchmarking
- Build in CI/CD 
- Near zero down-time server restart handling (sub millisecond downtime when updating)
- System environment handling build in (for Development, Test, Staging, Preprod and Prod)
- Security is by design, you only need to decide on scopes/groups.
- Audit readiness (everything is logged)
- RFC 9457 compliant
- (Automated) C4 modelling documentation


## Buzzword bing
- Functional Programming (we give you no choice - help you make better decisions)
- Vertical sliced architecture (making it a breeze to maintain the code)
- BDD/Behavior Driven Development (forcing you to work with a test first mindset)
- Domain Driven design (Bounded context and features are first class citizens)
- Event sourced (build in, easy to use, event source database)
- Event driven (TODO: make the features event driven!)
- 
