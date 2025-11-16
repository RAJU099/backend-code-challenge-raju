# Task 1 – Messages REST API

## How I Approached the Solution

For this task, I built a REST API that allows clients to create, read, update, and delete messages. All interactions with the data store go through the `IMessageRepository`, which helps keep the controller focused on request handling rather than data logic.

The controller exposes endpoints to:

* Fetch all messages associated with an organization
* Retrieve a specific message using its ID
* Add a new message
* Modify an existing message
* Remove a message

All operations are asynchronous to ensure the API stays responsive under load.

Design choices made:

* The controller remains minimal and avoids mixing in any business rules.
* Organization identifiers are checked to prevent cross-tenant data issues.
* Each endpoint returns meaningful HTTP status codes for clarity.
* The priority was to build something clean, easy to understand, and functionally correct.


## Things I Would Enhance With More Time

With additional time, I would strengthen the solution by:

* Introducing a dedicated service layer to keep business logic separate from HTTP concerns.
* Replacing the temporary in-memory mechanism with a proper database.
* Adding richer validation for inputs (e.g., title restrictions, content limits, organization-specific uniqueness).
* Implementing unified error-handling and structured error responses.
* Adding extensive logging to simplify troubleshooting and monitoring.
