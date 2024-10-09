# Canine Source Repository

**Canine Source Repository** is a revolutionary, web-based development platform that eliminates plumbing code, letting developers focus entirely on business logic. This opinionated tool ensures vertical slicing, functional programming, and automates all the low-level, repetitive tasks like database connections, transactions, logging, and API exposure, enabling developers to deliver clean, maintainable, and versioned features with ease.

## Key Features

- **No More Plumbing Code**:  
  Developers no longer need to deal with boilerplate tasks like opening and closing database connections, handling transactions (commit/rollback), or manually logging actions. All plumbing code is automated, allowing the developer to focus purely on business logic.

- **Flow-Chart Driven Development**:  
  Features are built using a **Business Process Notation (BPN)** editor, where workflows are represented visually as tasks connected by transitions. This ensures a clear, structured view of the feature's logic and flow.

- **Opinionated Development Tool**:  
  - **Vertical slicing** is enforced, ensuring each task represents a small, isolated unit of functionality.
  - A **functional programming mindset** is required: no classes—only records for inputs and outputs—forcing a clean, simple architecture.
  
- **Web-Based Editors**:  
  - **C# Editor** for writing business logic.
  - **BPN Editor** for defining the feature's workflow and task connections.
  
- **Automated API and Logging**:
  - Exposes versioned web APIs for features without any additional setup.
  - Automatically logs every API call and task execution, capturing performance metrics and providing strategic logging throughout the workflow for easy troubleshooting.
  
- **Feature and Task Definitions**:
  - Every feature starts with a **context**, ensuring high-level organization.
  - Features must be defined with an **objective** and a **FlowOverview**, capturing their purpose and expected behavior.
  - Features consist of tasks connected by transitions, and each task is defined by its:
    - **Business Purpose** and **Behavioral Goal**—these keep the focus on why the task exists, rather than how it is implemented.
    - **Input and Output** records—ensuring clean data handling.
    - **Test cases** (BDD-style) must be defined before implementation begins, ensuring that testing is focused on business outcomes from the start.

- **Eliminating Repetitive Work**:
  - **Dependency injection** is fully automated, and each task is limited to one injected service. This forces developers to design tasks that only do "one thing," keeping the codebase clean and simple.
  - The platform handles **database connections**, **transactions**, and **commit/rollback operations** automatically, so developers never have to worry about manual plumbing. This allows them to focus entirely on implementing business logic, without getting bogged down in infrastructure details.

- **Versioned Web API**:  
  The system automatically maintains and updates versioned APIs for every feature, ensuring consistent and backward-compatible interactions without manual intervention.

- **Immutable and Versioned System**:  
  Changes to contexts, features, tasks, and transitions are **immutable**—every modification creates a new version, ensuring historical traceability and safety.

- **CI/CD Automation**:  
  Integrated CI/CD pipeline with automatic deployment to the hosting server, removing the need for any manual setup. Every feature is automatically built, tested, and deployed, reducing friction and accelerating development.

- **BDD² Paradigm (Behavior-Driven Development Squared)**:  
  Canine Source Repository follows the **BDD²** philosophy, elevating traditional BDD by embedding business purpose throughout the development process. Developers are guided to focus first on defining the behavior and business value of their tasks, followed by writing tests and finally implementing the logic. This ensures that features are developed with a clear business focus from start to finish.

## Conclusion

With **Canine Source Repository**, plumbing code is a thing of the past. From automatic database management and transaction handling to CI/CD pipelines and versioned APIs, developers are free to focus exclusively on what matters—delivering business value. By enforcing best practices like vertical slicing, functional programming, and behavior-driven development, it ensures that every feature is clean, maintainable, and focused on the real-world business outcomes.
