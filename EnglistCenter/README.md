EnglistCenter - Razor Pages frontend UI for EnglishCenter API

This project is a thin UI layer (Razor Pages) that calls the EnglishCenter API for business logic.

Configuration:
- appsettings.json: set Api:BaseUrl to the API base address (including /api/). Example: https://localhost:5001/api/

Pages:
- /Account/Login - login page that calls POST api/auth/login and stores tokens in session.

Next steps:
- Add authorization middleware to forward access token to API calls.
- Add more pages and role-specific UI.
