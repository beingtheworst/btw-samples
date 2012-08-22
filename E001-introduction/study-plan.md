# Draft - Learning Practical Software Design 

This is a map of the territory that we'll be eventually trying to cover in the 
podcast. It will change as we learn more (links to the updated version will
be updated and announced)

##  Introduction

##  Block 1: Walking over important concepts (talks and drawings)

### Strategic Viewpoint: Concepts and decisions of DDD

* Important factors
* Good design
  * Practical stories
  * What is good design
  * Why is it imports
* Factors that affect design
  * Natural Boundaries
    * Languages
    * Teams
    * Organizations
  * Enabling Resources
    * Teams
    * Technologies
    * Knowledge
  * Constraints
    * Budget
    * Time
    * Scope and Quality
    * Political aspects
* Strategic concepts related to design
  * Domain Model
  * Ubiquitous Language
  * Bounded Context
  * Context Map
  * Project deliverables
* Processes

### Tactical Viewpoint: Concepts and factors inside BC

* Concepts
  * Long Running Process
  * Behavior
    * Aggregate
    * Aggregate Root
    * Identity
    * Value Object
    * use case
  * Integration
    * anti-corruption
  * Task-based UI
    * inductive vs. deductive
    * eventual consistency
  * "big data" processing
  * Continuous delivery

### Blueprint Viewpoint: Consistent patterns and practices

* TODO based on block 3

### Practical Viewpoint: Devil is in the details

* TODO based on block 4

##  Block 2: Practical Foundation (with isolated samples)

### Module: Messaging

* Definition and characteristics
  * vs RPC (sockets, web services)
  * Immutability
* Delivering messages
  * Transactional queues
  * Scalable Cloud queues
  * ACK+NACK
  * Visibility timeout
  * Temporal Decoupling
  * Direct dispatch
  * Pub/Sub and multiplexing
  * append-only streams
* Consuming messages
  * Idempotency
  * Competing consumers and load balancing
  * Message failures and quarantine
  * Message handlers and application services
* Defining messages
  * Contracts and serialization
  * Message builders
  * DSL
  * Human-readability
  * Versioning and evolution
* **[Related Podcast Episodes]**
  * [Episode 2 - Messaging Basics](http://beingtheworst.com/2012/episode-2-messaging-basics)
  
### Module: Event Sourcing

* Definition
* Aggregates with Event Sourcing
* Domain Services
* Specifications
  * Unit tests
  * Documentation
* Evolving A+ES
* Basic performance concerns
  * Caching
  * Snapshots / Memoization
* Building event store
* Concurrency problems
* Eventual replication
* Master - slave - clone
* Failover modes
* Indexing and caching
* Conflict resolution

### Module: Distributed Architecture Elements

* CQS
* CQRS perspective
  * Elements: Client - Application Server - Projection Host
  * Contracts: Command - Event - View
  * Relation to Bounded contexts and UL
* Modeling distributed world
  * Temporal decoupling
  * Eventual consistency
  * Volatility
  * Transactional scopes
* Team scalability patterns
  * Outsourcing CQRS elements
  * Developing CQRS elements in parallel
  * Developing features in parallel
  * Risk mitigation
* Performance scalability patterns
  * Application Server - Partitioning and load balancing 
  * Client - Load balancing
  * Projection Host - multiplexing and partitioning

### Module: Deriving state with Projections, Queries and Indexes

* Definition
* Simple implementation
  * Document storage interfaces
  * Projection Code
  * Performance optimizations and tricks
  * Building automated rebuilder
  * Tracking changes in code
* Eventual consistency
* Concurrent changes
* Scaling and replication

### Module: Capturing the Domain

* Domain modeling exercises

### Module: Dealing with Time and Uncertainty

* TODO

### Module: Applied Machine Learning

* Core principles of "learning"
  * deterministic
  * non-deterministic
* Supervised vs. unsupervised learning
* Validation and experiments
* Dealing with data
* Classical approaches
  * Regressions
  * Classification
* "Brute-force" approaches
  * Neural Networks
  * Genetic Algorithms
* Hybrid approaches
  * Evolutionary Neural networks
  * Self-recovering distributed systems
* Distributing CPU-intensive machine learning tasks

##  Block 3: Bringing it all together (diving into large sample)

### Module: Development Process

* Source code versioning
* Unit testing
* Continuous integration
* Lowering development friction
* Rapid iterations
* Keeping things simple and separate
* Distributing work and outsourcing

### Module: Breaking Down Common System Archetypes

* Application Server
* Thin client
  * API
  * Web
  * Native
    * Mobile
    * Desktop
* Hybrid systems
  * Smart "Fat" client
  * Occasionally connected system

### Module: Application Server Blocks

* Application Service
* Aggregate
* View Projection
* Long Running Process (Task)
* Event Port
* Domain Service
* Bounded Context

### Module: Application Server Infrastructure

* Atomic document storage
  * Atomicity
  * Retrial and conflict resolution
  * Local files
  * Cloud / PaaS
* Append-only storage
  * Local files
  * Cloud / PaaS
  * SQL
* Streaming blob storage
  * Local files
  * Cloud / PaaS
* Message dispatcher
  * Quarantine
  * Deduplication manager
  * Message overflows
  * Debugging and replays
* Task runner
* Startup tasks

### Module: Big data systems

* Partitioned storage
* Distributed processing
* Message and record streaming
* Real-time aspects
* Performance improvements
* Data Visualization

### Module: Practical Machine Learning

* Basics
  * Supervised vs unsupervised learning
  * Error
  * Data quality and validation
* Deterministic heuristics
* Non-deterministic brute-force
  * Neural networks
  * Genetic algorithms
* Scaling out CPU-intensive learning
* Practical applications
* Deployment, maintenance and life hacks
 
##  Block 4: Keeping System Alive and Growing (case studies)

### Module: Project Case Studies and Lessons Learned

* Lokad Salescast
* Lokad Hub
* Lokad Watchtower
* Lokad Salescast2
* Event Day
* Human Record
* TODO: ask for more external cases
### Module: Deployment Routines
* Options and costs
  * With downtime
  * With reduced SLAs
  * With partial functionality
  * Without downtime
* Managing versions
* Incremental deployments
* Rolling back on failures

### Module: Managing Failures

* TODO: gather from Lokad experience

### Module: Reducing Development Friction and Lifehacks

* TODO: gather from Lokad experience

### Module: Specialised technology overview

* Persistence
  * Relational
    * MS SQL
    * mySQL
    * Postgres
    * Oracle
  * NoSQL
    * Redis
    * CouchDb
    * MongoDB
    * Azure
    * Amazon
  * NoDB
    * Files / Blobs
    * Memory caching
* Messaging
  * Broker-based
  * Broker-less
* Push and pull infrastructures
* Hardware factors
  * Rust drives
  * SSD
  * RAM
  * Multi-core processing (including low-power ARM scenarios)
  * Dedicated hardware
