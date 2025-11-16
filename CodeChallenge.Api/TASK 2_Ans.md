
# Task 2 – MessageLogic & Validation

## My Thought Process Behind the Validation

Instead of handling validation inside the controller, I placed all business rules in a dedicated `MessageLogic` component. The idea was to keep request handling lightweight and move decision-making into a layer that can be reused, tested independently, and modified without touching the API surface.

The logic enforces several constraints, such as:

* Only active messages are allowed to be modified or removed.
* Every update triggers an automatic refresh of the `UpdatedAt` timestamp.
* Message titles must be unique within the same organization.
* Titles must fall within a reasonable length range (3–200 characters).
* Content must also adhere to defined size boundaries (10–1000 characters).

By grouping these rules together, the system avoids scattered validations and helps maintain a clean separation between business concerns and HTTP behavior.


## How I Would Evolve This for Production

If this were being prepared for a real-world deployment, I would reshape the solution in several ways:

* Replace the in-memory provider with a persistent database.
* Introduce structured error-handling and clear error models.
* Implement authentication and authorization to restrict who can manage messages.
* Add monitoring and logging to track usage and issues.
* Include pagination or caching strategies for scenarios involving large message sets.
