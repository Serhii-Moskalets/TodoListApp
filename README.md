# TodoListApp

A full-stack Todo List web application with a **React** frontend and **ASP.NET Core Web API** backend, using **SQL Server** and **Entity Framework Core**.

## Features

- User authentication and profile management using ASP.NET Core Identity
- Create, update, delete, and manage tasks
- Add comments to tasks
- Tag tasks for better organization
- Filter, search, and sort tasks
- Assign tasks to users and manage access

## Technologies Used

- **Frontend:** React, JavaScript, CSS/SCSS
- **Backend:** ASP.NET Core Web API (latest version), C#
- **Database:** SQL Server (or PostgreSQL if hosting restrictions)
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Version Control:** Git, GitHub

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/)
- SQL Server or PostgreSQL database

### Setup
1. Clone the repository:
    ```bash
    git clone https://github.com/your-username/TodoListApp.git
    ```
2. Navigate to the backend folder and install dependencies:
    ```bash
    cd TodoListApp/Backend
    dotnet restore
    ```
3. Navigate to the frontend folder and install dependencies:
    ```bash
    cd TodoListApp/Frontend
    npm install
    ```
4. Configure the connection string in `appsettings.json`.
5. Run migrations and start the backend:
    ```bash
    dotnet ef database update
    dotnet run
    ```
6. Start the frontend:
    ```bash
    npm start
    ```

## Contributing

Contributions are welcome! Please fork the repository and create a pull request.

## License

This project is licensed under the MIT License.
