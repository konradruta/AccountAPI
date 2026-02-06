# API zarządzania kontami użytkowników

REST API stworzone w ASP.NET Core, umożliwiające rejestrację, logowanie
oraz zarządzanie kontem użytkownika.

## Funkcjonalności
- Rejestracja użytkownika
- Logowanie użytkownika (JWT)
- Zarządzanie kontem użytkownika
- Zmiana hasła użytkownika
- Wymuszona zmiana hasła przy użyciu hasła tymczasowego
- Funkcja "Forgot password" z wysyłką hasła tymczasowego e-mailem
- Reset licznika błędnych logowań po poprawnej autoryzacji
- Hashowanie haseł (PasswordHasher)
- Walidacja danych wejściowych (FluentValidation)
- Globalna obsługa wyjątków (middleware)
- Swagger / OpenAPI

## Role i autoryzacja
- Role użytkowników przechowywane w bazie danych (`User`, `Admin`)
- Role przekazywane jako **claims w tokenie JWT**
- Autoryzacja oparta o **Role-Based Authorization**
- Ograniczenie dostępu do wybranych endpointów przy użyciu atrybutu:
  ```csharp
  [Authorize(Roles = "Admin")]
-Kontrola dostępu do zasobów w zależności od roli użytkownika

## Bezpieczeństwo
- Hasła przechowywane wyłącznie w postaci hashy
- Weryfikacja starego hasła przed zmianą
- Obsługa błędnych prób logowania
- Hasła tymczasowe z wymuszoną zmianą przy logowaniu
- Autoryzacja oparta o JWT i claims
- Ochrona wrażliwych endpointów przed dostępem nieautoryzowanym

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

## Testy
Projekt zawiera testy jednostkowe logiki biznesowej (warstwa Services),
napisane przy użyciu:

- **xUnit** – framework testowy
- **Moq** – mockowanie zależności
- **Entity Framework Core InMemory** – testowa baza danych

Testy obejmują m.in.:
- rejestrację użytkownika
- poprawne logowanie użytkownika
- logowanie z błędnym hasłem
- próbę logowania nieistniejącego użytkownika

## Uruchomienie projektu
1. Sklonuj repozytorium
2. Skonfiguruj connection string w `appsettings.json`
3. Wykonaj migracje bazy danych
4. Uruchom aplikację
5. Otwórz Swagger:
https://localhost:7290/swagger


## Cel projektu
Projekt stworzony jako część portfolio backendowego,
prezentujący implementację bezpiecznego REST API w ASP.NET Core
z autoryzacją JWT, rolami użytkowników oraz testami jednostkowymi.
