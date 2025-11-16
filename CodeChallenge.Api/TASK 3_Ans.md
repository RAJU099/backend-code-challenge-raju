# CodeChallenge Assignment – Task 3: Unit Testing

## Overview

For this part of the assignment, I added automated tests targeting the `MessageLogic` component. The intention was to verify that the business rules behave correctly in a variety of situations—including both typical operations and edge cases—without relying on any external systems.

## Task 3: Testing Approach

### Question 5: Describe your testing strategy and the tools you selected.

### Testing Approach

My goal was to thoroughly validate how the business logic responds to different inputs and scenarios. The tests focus solely on `MessageLogic`, ensuring that its rules and validations behave consistently. I covered cases such as:

* Creating a valid message successfully
* Detecting duplicate titles within the same organization
* Rejecting content that falls outside the allowed length
* Returning *NotFound* when attempting to update a message that doesn’t exist
* Preventing updates to messages that are no longer active
* Ensuring deletes fail if the message cannot be found

These tests validate the expected outcomes for both standard workflows and exceptional conditions, giving me confidence that this layer behaves predictably.


### Tools Used

1. xUnit
A lightweight and modern testing framework for .NET. It offers strong extensibility, clean test organization, and fast execution, making it a natural fit for this project.

2. Moq
Used for mocking `IMessageRepository`. Mocking ensures that the tests isolate the business logic by simulating repository behaviors rather than interacting with live data or storage.

3. FluentAssertions
This library improves readability by allowing expressive assertions. It also makes test failures easier to understand thanks to clear, detailed messages.


### Question 6: Additional Scenarios for Real-World Testing

If this were a production-ready system, more extensive testing would be necessary. Some areas I would focus on include:

1. Security & Access Control
Confirm that only authorized users can read or modify specific messages.

2. Concurrency Handling
Test how simultaneous operations behave—for example, two users attempting to update the same message.

3. Performance With High Data Volume
Evaluate behavior when dealing with thousands of messages or large payloads.

4. Broader Input Validation
Test extreme or malicious inputs, such as oversized fields, empty values, or attempts at injection attacks.

5. Resilience to Infrastructure Failures
Simulate issues like repository or database downtime to ensure the logic handles failures gracefully.

6. Cache-Related Behavior
If caching were introduced, verify that stale data is not returned after updates or deletes.

7. Logging Verification
Ensure that meaningful logs are created for actions like creation, updates, and deletions for traceability.


## How to Run the Tests

To execute the tests locally:

1. Install the **.NET 8 SDK**.
2. Open the repository and navigate to the project folder.
3. Restore dependencies:

   dotnet restore

4. Build the solution:

   dotnet build

5. Run the full test suite:

   dotnet test


## Conclusion

This task highlights how automated testing can validate core business logic independently of external systems. Through mocking, clear assertions, and targeted scenarios, the tests help ensure that the application behaves consistently across both expected and edge-case situations.
