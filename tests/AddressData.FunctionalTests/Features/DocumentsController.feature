Feature: Documents
  In order to manage address documents
  As an API client
  I want to be able to add and retrieve city documents via the DocumentsController

  @AddCityError
  Scenario: Add city with invalid areaId
    Given I have an areaId <InvalidAreaId>
    When I call the InsertDocument endpoint of the DocumentsController
    Then I expect to receive an error response with status code <ExpectedStatusCode>
    And the response content should contain "<ExpectedErrorMessage>"

    Examples:
      | InvalidAreaId | ExpectedStatusCode | ExpectedErrorMessage                                                                     |
      | 0             | 500                | "Please provide a valid AreaId. It must be a positive number. AreaId provided: 0."         |
      | -1            | 500                | "Please provide a valid AreaId. It must be a positive number. AreaId provided: -1."        |

  @RunSeeding
  Scenario: Run seeding should return cities that have been added
    When I call the SeedDocuments endpoint with limit = <limit>
    Then I expect a 200 status code
    And I expect the response to contain the list of seeded cities matching the limit

    Examples:
      | limit |
      | 5     |

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
