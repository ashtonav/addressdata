Feature: Documents
  In order to manage address documents
  As an API client
  I want to be able to add and retrieve city documents via the DocumentsController

  @AddCityError
  Scenario: Add city should return an error for invalid or unresolvable input
    Given I have an areaId <AreaId>
    When I call the InsertDocument endpoint of the DocumentsController
    Then I expect to receive an error response with status code <ExpectedStatusCode>
    And the response content should contain <ExpectedErrorMessage>

    Examples:
      | AreaId     | ExpectedStatusCode | ExpectedErrorMessage                                                                |
      | 0          | 500                | "Please provide a valid AreaId. It must be a positive number. AreaId provided: 0."  |
      | -1         | 500                | "Please provide a valid AreaId. It must be a positive number. AreaId provided: -1." |
      | 3699999999 | 500                | "not found in Overpass Turbo"                                                       |
      | 3600181430 | 500                | "is small"                                                                          |

  @RunSeeding
  Scenario: Run seeding should return cities that have been added
    When I call the SeedDocuments endpoint with limit = <limit>
    Then I expect a 200 status code
    And I expect the response to contain the list of seeded cities matching the limit

    Examples:
      | limit |
      | 1     |

  @InsertCity
  Scenario: Insert city should return created result
    Given I have an areaId <AreaId>
    When I call the InsertDocument endpoint of the DocumentsController
    Then I expect a 201 status code
    And I expect the response to contain the newly created city's details

    Examples:
      | AreaId     |
      | 3614369889 |
      | 3602168517 |

  @InsertThenGet
  Scenario: Insert then GET should return the correct city
    Given I have an areaId <AreaIdToGet>
    When I call the InsertDocument endpoint to add the city
    And I immediately call GetDocument endpoint with the same areaId
    Then I expect a 200 status code
    And I expect the city data to match what was inserted

    Examples:
      | AreaIdToGet |
      | 3600181417  |
      | 798351215   |

  @GetAllDocuments
  Scenario: Get all documents should include a previously inserted city
    Given I have an areaId <AreaId>
    When I call the InsertDocument endpoint to add the city
    And I call the GetAllDocuments endpoint
    Then I expect a 200 status code
    And I expect the list of documents to include the newly created city

    Examples:
      | AreaId     |
      | 3602168517 |

  @GetDocumentNotFound
  Scenario: Get document should return not found for an areaId with no matching location
    Given I have an areaId <UnknownAreaId>
    When I call the GetDocument endpoint of the DocumentsController
    Then I expect a 404 status code
    And the response content should contain "Location not found"

    Examples:
      | UnknownAreaId |
      | 3699999999    |

  @GetDocumentNotFound
  Scenario: Get document should return not found for a valid location with no document yet
    Given I have an areaId <AreaId>
    When I call the GetDocument endpoint of the DocumentsController
    Then I expect a 404 status code
    And the response content should contain "Document not found"

    Examples:
      | AreaId     |
      | 3600181430 |
