# URL Shortener API

A robust, high-performance URL shortening service built with ASP.NET Core 8.0. This API provides user management, role-based access control, custom aliases, and background-processed analytics.

## Features

### 🔐 Authentication & Identity
- **Identity Management:** Full user registration and login system using ASP.NET Core Identity.
- **JWT Authentication:** Secure stateless authentication with Access and Refresh tokens.
- **Security Questions:** Enhanced account recovery with hashed security answers.
- **Pro Plan:** Subscription-based features (Free vs. Pro).

### 🔗 URL Shortening
- **Shorten URLs:** Generate short, unique codes for long URLs.
- **Custom Aliases:** Pro users can define their own custom short codes.
- **Redirection:** Fast redirection from short codes to original destination URLs.
- **Link Management:** Users can view and delete their own generated links.

### 📊 Analytics & Performance
- **Background Processing:** Click logging is handled asynchronously using `System.Threading.Channels` and a `BackgroundService` to ensure high redirection performance without database bottlenecks.
- **Admin Dashboard:** Admins can view site-wide analytics, monitor all users, and deactivate any malicious links.

## Tech Stack

- **Framework:** ASP.NET Core 8.0 (Web API)
- **Database:** SQLite
- **ORM:** Entity Framework Core
- **Security:** JWT (JSON Web Tokens)
- **Documentation:** Swagger (OpenAPI)

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Entity Framework Core Tools

### Installation

1.  **Clone the repository:**
    ```bash
    git clone <your-repo-url>
    cd URL_Shortener
    ```

2.  **Configure Environment:**
    - Open `appsettings.json`.
    - Set a strong key for `Jwt:Key`.
    - The database is configured to use `UrlShortener.db` by default.

3.  **Apply Migrations:**
    ```bash
    dotnet ef database update
    ```

4.  **Run the Application:**
    ```bash
    dotnet run
    ```
    The API will be available at `https://localhost:7193` (or your configured port).

## API Usage & Authentication

### Custom Authorization Header
**Important:** This API uses a custom header for JWT authentication. Instead of the standard `Authorization: Bearer <token>`, you must send your token in the `Authorization_Header`:

```http
GET /api/urls
Authorization_Header: <your_jwt_token>
```

### Key Endpoints

| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Register a new user | No |
| `POST` | `/api/auth/login` | Login and receive tokens | No |
| `POST` | `/api/urls` | Create a shortened URL | Yes |
| `GET` | `/r/{code}` | Redirect to original URL | No |
| `GET` | `/api/admin/urls` | View all system URLs | Yes (Admin) |

## Default Admin Account
Upon first run, the database seeder creates a default admin account:
- **Email:** `admin@urlshortener.com`
- **Password:** `Admin@1234`

*Please change these credentials immediately after deployment.*

## Project Structure

- `Controllers/`: API endpoints for Auth, URLs, Redirection, and Admin.
- `DTOs/`: Data Transfer Objects for clean API contracts.
- `Models/`: Entity definitions and Enums.
- `Services/`: Business logic including Token generation and ShortCode generation.
- `Services/ClickLogProcessorService.cs`: Background worker for processing analytics queue.

## License
This project is licensed under the MIT License.

---

*Developed with ❤️ by [Your Name/Handle]*
