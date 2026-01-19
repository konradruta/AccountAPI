# API zarządzania kontami użytkowników

REST API stworzone w ASP.NET Core, umożliwiające rejestrację, logowanie
oraz zarządzanie kontem użytkownika.

## Funkcjonalności
- Rejestracja użytkownika
- Logowanie użytkownika (JWT)
- Zarządzanie kontem użytkownika
- Hashowanie haseł
- Walidacja danych wejściowych (FluentValidation)
- Globalna obsługa wyjątków (middleware)
- Swagger / OpenAPI

## Architektura
- Controllers – obsługa endpointów API
- Services – logika biznesowa
- Entities – encje domenowe i kontekst bazy danych
- Middleware – globalna obsługa błędów
- Exceptions – własne wyjątki aplikacyjne
- Migrations – migracje bazy danych (EF Core)

## Technologie
- ASP.NET Core
- C#
- Entity Framework Core
- JWT Authentication
- FluentValidation
- Swagger

## Uruchomienie projektu
1. Sklonuj repozytorium
2. Skonfiguruj connection string w `appsettings.json`
3. Wykonaj migracje bazy danych
4. Uruchom aplikację
5. Otwórz Swagger:
https://localhost:7290/swagger


## Cel projektu
Projekt stworzony jako część portfolio backendowego,
w celu nauki oraz prezentacji umiejętności w ASP.NET Core.
