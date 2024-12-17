# ADR-2: Use of Strategy Pattern and Configuration Management for AllowedActionsService

- **Date**: 2024-06-17  
- **Status**: Accepted  
- **Author**: Roland R
- **Context**: Card Action Microservice  

---

## **Decision**  
The **Strategy Pattern** has been selected as the design approach for implementing the `AllowedActionsService`. Additionally, the configuration of allowable actions is stored in a file (`config\ActionsConfiguration.json`) and an embedded JSON configuration in code is used for unit testing. The `InMemoryDataProvider` is utilized to simulate data retrieval, balancing simplicity and test accuracy.

---

## **Rationale**

1. **Use of the Strategy Pattern**  
   - The Strategy Pattern allows flexible and extensible implementation of `AllowedActionsService`, which evaluates allowed actions based on the type and status of the card.  
   - Adding new strategies (e.g., based on future business rules) is straightforward without altering the existing codebase.  
   - Promotes compliance with the **Open-Closed Principle** (SOLID), as new rules can be introduced without modifying the service itself.

2. **Configuration Management**  
   - Allowable actions are stored in an external configuration file `config\ActionsConfiguration.json`.  
   - This approach supports:
     - Easy updates to business rules without recompiling the application.
     - Dynamic changes to configuration during deployment or runtime.  

3. **Embedded JSON Configuration for Unit Tests**  
   - For unit tests, a copy of JSON configuration with additional verison nb. 
   - This ensures that unit tests remain **deterministic** and independent of external file changes.  
   - New test cases can be easily added to validate extended or modified configurations.

   - **Benefit**: Changes to the `config\ActionsConfiguration.json` file do not impact unit test execution, ensuring tests remain valid even as business rules evolve.

4. **Use of InMemoryDataProvider**  
   - The `InMemoryDataProvider` is implemented to simulate asynchronous data retrieval using `await Task.Delay(100)` to mimic a real-world database call.  
   - While this may exceed the strict speed requirements of unit tests, the current approach prioritizes **simplicity** over absolute performance.  
   - In the future, a `FakeDataProvider` implementation (based on `ICardProvider`) can replace the in-memory provider for faster unit tests.

---

## **Consequences**

- **Advantages**:  
   - The Strategy Pattern enables scalable and maintainable logic for determining allowed actions.  
   - Business rules can be adjusted dynamically via the configuration file without code changes.  
   - Unit tests remain robust, independent of configuration file changes, and can easily accommodate new test scenarios.  
   - Asynchronous simulation (`Task.Delay`) simplifies mocking real-world data retrieval for testing purposes.

- **Disadvantages**:  
   - Unit tests may be slower due to the `await Task.Delay(100)` in `InMemoryDataProvider`.  
   - The embedded JSON in unit tests requires manual updates when configuration logic changes.  

---

## **Future Considerations**

1. Replace the `InMemoryDataProvider` with a **FakeDataProvider** implementation that adheres to the `ICardProvider` interface to eliminate unnecessary delays in unit tests.
2. Explore dynamic configuration reloading during runtime for enhanced flexibility.
3. Automate validation of configuration file updates to ensure compatibility with existing test scenarios.

---

## **Conclusion**

The **Strategy Pattern** has been adopted for its extensibility, and the configuration of allowable actions is managed via JSON files for flexibility. Embedded JSON is used in unit tests to ensure consistency, while `InMemoryDataProvider` strikes a balance between simplicity and realism. Future improvements can optimize test speed and configuration validation.

---

## **Meta**

- **Date**: 2024-06-17  
- **Status**: Accepted  
- **Author**: Roland R  
- **Version**: 1.1  
