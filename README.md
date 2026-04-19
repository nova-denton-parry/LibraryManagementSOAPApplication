# LibraryManagementSOAPApplication

WCF-based library management system with a console client, built in C# for Windows.

## Features

- Add, update, and delete books from the library
- Check out and return books with availability tracking
- Search by ISBN, title, author, or availability status
- ISBN-13 format validation with automatic formatting (strips dashes and spaces)
- Detailed validation error messaging via typed WCF FaultExceptions
- Interactive console UI with y/n prompts and confirmation steps

## Requirements
- .NET Framework 4.8 (Windows only)
- Visual Studio 2022 or later

## Setup
1. Clone the repository
2. Open `LibraryManagementService.sln` in Visual Studio, make sure the project root is selected, and run it - this starts the WCF service (you'll know you were successful if a web browser opens; if a popup window opens, you have the wrong file selected in the solution explorer)
3. Open `LibraryManagementClient.sln` in a separate Visual Studio instance and run it - this launches the console client

> ⚠️ The service **must** be running before the client

## How It Works
```mermaid
flowchart TD
    A([Start]) --> B(Launch WCF Service)
    B --> C(Launch Console Client)
    C --> D(Main Menu)
    D --> E{Selection}
    E --> I([Exit])
    E --> F(View All Books)
    E --> G(Add New Book)
    E --> H(Search)
    H --> J{Search By}
    J --> K(ISBN)
    J --> L(Title)
    J --> M(Author)
    J --> N(Availability)
    K & L & M & N --> O(View Book Detail)
    F --> O
    O --> P{Options}
    P --> Q(Check Out)
    P --> R(Return)
    P --> S(View Availability)
    P --> T(Edit Details)
    P --> U(Delete)
    P --> D
    G --> D
```
