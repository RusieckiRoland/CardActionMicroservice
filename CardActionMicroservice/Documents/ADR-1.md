# ADR-1: Selection of HTTP POST Method for Retrieving Card Data

- **Date**: 2024-06-17  
- **Status**: Accepted  
- **Author**: Roland R
- **Context**: Card Action Microservice

---

## **Decision**  
The HTTP **POST** method has been chosen for retrieving user card data instead of GET. The primary reason for this decision is **data security**, particularly protecting sensitive card numbers from accidental exposure.

---

## **Rationale**

1. **Protection of Sensitive Data**  
   User card numbers are considered sensitive information. Using the **GET** method transmits parameters (including card numbers) in the URL, which can lead to:  
   - Storage in **browser history**.  
   - Logging in server-side HTTP logs.  
   - Exposure through referer headers sent to other services or servers.  

   The **POST** method transmits data in the request body, which reduces the risk of accidental exposure in these contexts.

2. **Security as a Priority**  
   Although GET is **idempotent** and generally preferred for data retrieval, **data security** takes precedence over idempotency in this scenario.

3. **Future Extensibility**  
   Using POST allows easier extension of the input payload structure, such as adding filters or additional parameters in the future.

---

## **Alternatives Considered**

1. **GET**  
   While GET is the natural choice for retrieving resources, it does not provide sufficient protection for sensitive card data.

2. **POST with Additional Encryption**  
   Encrypting payload data was considered, but it was deemed that POST, combined with existing HTTPS mechanisms, provides an adequate level of security.

---

## **Consequences of the Decision**

- **Advantages**:  
   - Improved security for sensitive card numbers.  
   - Data is not exposed in logs, browser history, or referer headers.  

- **Disadvantages**:  
   - POST is
