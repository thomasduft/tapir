# Todo

Request
- Headers
- QueryParameters
- Content
- Method
- Endpoint

=> HttpRequestMessageBuilder
  - Create(instructions)
  - Build(): HttpRequestMessage

Response
- StatusCode
- ReasonPhrase
- Content
- Headers

=> HttpResponseMessageValidator
  - Create(instructions)
  - WithStatusCode(response.StatusCode)
  - WithReasonPhrase(response.ReasonPhrase)
  - WithContent(response.Content)
  - WithHeaders(response.Headers)
  - Validate(): IEnumerable<ValidationResult>

- IRequestCreator
  - public
- IResponseHandler/Verifier/Checker
  - public

- Actions
  - Area: Request
    - AddHeader: Name, Value
    - AddQueryParam: Name, Value
  - Area: Execution
    - AddBody: File, Value
    - Send: Method, Endpoint
  - Area: Response
    - CheckStatusCode: Value
    - CheckStatusMessage: Value
    - CheckHeader: Name, Value
    - CheckBody: File, Value
    - StoreVariable: Name, Path


- [ ] Provide executable API with some dummy endpoints to test `tapir`
  - users
    - get
    - post
    - put
    - delete
  - documents
    - get
    - post
    - put
    - delete
