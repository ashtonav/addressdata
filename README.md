# AddressData

![banner](docs/logo.png)

[![codecov](https://codecov.io/gh/ashtonav/addressdata/graph/badge.svg?token=ZD0L2LC2U0)](https://codecov.io/gh/ashtonav/addressdata)
[![.NET](https://github.com/ashtonav/addressdata/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ashtonav/addressdata/actions/workflows/dotnet.yml)
[![license](https://img.shields.io/github/license/ashtonav/addressdata.svg)](LICENSE)

AddressData is a tool that collects addresses from real cities around the world, along with their latitude and longitude coordinates.

After addresses are collected, they are used by [github.com/ashtonav/addressdata.net](https://github.com/ashtonav/addressdata.net) to power [AddressData.net](https://AddressData.net), a site that displays all known addresses by city, alongside an interactive map.

Currently, [AddressData.net](https://AddressData.net) contains millions of addresses for over 1,500 cities around the world.

## Table of Contents

- [Installation](#installation)
    - [Using Visual Studio](#using-visual-studio)
        - [Requirements](#requirements)
        - [How to Run](#how-to-run)
        - [How to Test](#how-to-test)
- [Usage](#usage)
- [Acknowledgments](#acknowledgments)
- [Contributing](#contributing)
- [License](#license)

## Installation

### Using Visual Studio

#### Requirements
- Visual Studio 2022
    - With ASP.NET and web development installed from the Visual Studio Installer
- .NET 8 SDK
- Any Operating System

#### How to Run
1. Open the solution in Visual Studio 2022.
2. Build and launch the AddressData.WebApi project.
3. The API can be accessed at [https://localhost:5280](https://localhost:5280).

#### How to Test
1. Open the solution in Visual Studio 2022.
2. Run the tests in Test Explorer.

## Usage

1. **Start the Web API**: Run the `AddressData.WebApi` project.
2. **Seed the Data**:
    - Navigate to [https://localhost:5280](https://localhost:5280). 
    - Make a `POST` request to the `/documents/seed` endpoint.
3. **Monitor Output**:
    - After a while, you’ll see a newly created folder structure (in your solution’s output directory) containing country and city CSV files with addresses.
    - Note that the *full* seeding process can take up to **1 day** to complete, depending on how many records you are collecting.
    - If you’re in a hurry, you can provide a **limit** parameter in your POST request to `/documents/seed` to only generate a smaller subset of data.

## Acknowledgments

- **Overpass Turbo API**:  
  Addresses are retrieved from [Overpass Turbo](https://overpass-turbo.eu/)—a web-based data mining tool for OpenStreetMap. It provides a powerful query language for extracting location-based data (like street addresses) from the OpenStreetMap database.

## Contributing

Pull requests are accepted.

## License

[MIT](https://choosealicense.com/licenses/mit/)
