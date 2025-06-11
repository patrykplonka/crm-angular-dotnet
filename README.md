System CRM (Angular + .NET Core)
Prosty system CRM do zarządzania kursami elearning.
Wymagania wstępne
Przed uruchomieniem aplikacji upewnij się, że masz zainstalowane:

.NET 8 SDK
Node.js 18+ (zawiera npm)
Angular CLI (npm install -g @angular/cli)
SQL Server lub inna wspierana baza danych
Visual Studio 2022 lub VS Code (opcjonalnie)


Uruchamianie aplikacji
1. Sklonuj repozytorium
git clone https://github.com/patrykplonka/crm-angular-dotnet.git
cd crm-angular-dotnet

2. Konfiguracja backendu

Przejdź do katalogu projektu backendowego (np. Backend lub CRM.WebApi).
Przywróć pakiety NuGet:dotnet restore


Skonfiguruj ciąg połączenia z bazą danych w pliku appsettings.json:"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CRMDb;Trusted_Connection=True;"
}


Zastosuj migracje bazy danych, aby stworzyć bazę:dotnet ef database update --project CRM.Infrastructure --startup-project CRM.WebApi


Uruchom API backendu:dotnet run --project CRM.WebApi

API będzie dostępne pod adresem https://localhost:5001 (sprawdź port w launchSettings.json). Dokumentacja Swagger: https://localhost:5001/swagger.

3. Konfiguracja frontendu

Przejdź do katalogu projektu frontendowego (np. ClientApp):cd ClientApp


Zainstaluj zależności npm:npm install


Zaktualizuj adres API w pliku environment.ts, jeśli backend działa na innym porcie:export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api'
};


Uruchom aplikację Angular:ng serve

Frontend będzie dostępny pod adresem http://localhost:4200.

4. Uruchamianie testów w Visual Studio
Testy backendu

Otwórz rozwiązanie (crm-angular-dotnet.sln) w Visual Studio 2022.
Upewnij się, że projekt testowy CRM.Tests ma poprawnie skonfigurowane pakiety (np. xUnit, Moq).
Przejdź do Test > Eksplorator testów (lub użyj skrótu Ctrl+E, T).
W Eksploratorze testów zobaczysz listę testów z projektu CRM.Tests.
Kliknij Uruchom wszystkie testy lub wybierz konkretne testy i kliknij Uruchom wybrane testy.
Wyniki testów pojawią się w Eksploratorze testów.

Testy frontendu

Upewnij się, że zależności npm są zainstalowane w folderze ClientApp (patrz sekcja "Konfiguracja frontendu").
W Visual Studio otwórz terminal (Narzędzia > Wiersz polecenia > Terminal dewelopera) i przejdź do folderu ClientApp:cd ClientApp


Uruchom testy Angular:ng test

Testy uruchomią się w przeglądarce (Karma/Jasmine). Wyniki pojawią się w przeglądarce i konsoli Visual Studio.
Aby uruchomić testy jednorazowo (bez automatycznego odświeżania):ng test --watch=false


