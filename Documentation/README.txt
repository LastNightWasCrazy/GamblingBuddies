System zarządzania kasynem online: GamblingBuddies
==================================================
Twórcy
==================================================
1. Bartosz Śniadkowski (lider)
2. Krzysztof Pych
3. Wojciech Korenkiewicz

==================================================
Założenia systemu
==================================================
GamblingBuddies to aplikacja webowa stworzona w technologii ASP.NET Core MVC, wspierająca zarządzanie działalnością kasyna online. System umożliwia obsługę procesów związanych z organizacją gier, zarządzaniem personelem, rezerwacjami miejsc przy stołach, obsługą płatności oraz generowaniem raportów.

Aplikacja została zaprojektowana zgodnie z architekturą warstwową, wykorzystując Entity Framework Core do komunikacji z relacyjną bazą danych SQL Server.

==================================================
Główne funkcjonalności
==================================================
1. Zarządzanie strukturą kasyna
	zarządzanie salami gry,
	zarządzanie stołami do gier,
	przypisywanie gier do stołów,
	konfiguracja parametrów gier oraz ich wariantów.

2. Zarządzanie sesjami gier
	planowanie sesji gier w określonych terminach,
	określanie czasu rozpoczęcia i zakończenia sesji,
	monitorowanie aktywnych i zakończonych sesji.

3. Rezerwacje
	tworzenie rezerwacji miejsc przy stołach,
	przypisywanie miejsc do uczestników,
	zarządzanie statusem rezerwacji,
	przegląd historii rezerwacji.

4. Obsługa płatności
	rejestrowanie płatności za rezerwacje,
	integracja z systemem płatności PayU,
	monitorowanie statusów płatności,
	historia transakcji płatniczych.

5. Zarządzanie personelem
	ewidencja pracowników,
	zarządzanie stanowiskami i statusami pracowników,
	planowanie zmian roboczych,
	przypisywanie pracowników do konkretnych sesji gier.

6. Raporty i archiwizacja
	generowanie raportów płatności,
	archiwizacja wygenerowanych raportów,
	przechowywanie załączników PDF,
	przegląd historii raportów.

7. Bezpieczeństwo i uprawnienia
	logowanie użytkowników do systemu,
	obsługa ról i uprawnień,
	kontrola dostępu do funkcjonalności administracyjnych,
	rejestrowanie wybranych działań użytkowników.

8. Interfejs API
	System udostępnia również podstawowe endpointy API umożliwiające pobieranie danych dotyczących:
		gier,
		sesji gier,
		sal,
		stołów,
		rezerwacji,
		płatności,
		graczy.

==================================================
Struktura użytkowników systemu
==================================================
1. Administrator (login: admin, hasło: admin123) - posiada pełny dostęp do systemu.
2. Menadżer (login: manager, hasło: manager123) - zarządza personelem, sesjami i raportami.
3. Pracownik (login: employee, hasło: employee123) - widzi przydzielone obowiązki i harmonogram.

==================================================
Architektura systemu
==================================================
1. Warstwa prezentacji
	ASP.NET Core MVC,
	Razor Views,
	HTML,
	CSS,
	JavaScript.

2. Warstwa logiki biznesowej
	obsługa procesów biznesowych,
	walidacja danych,
	zarządzanie harmonogramami,
	obsługa płatności.

3. Warstwa dostępu do danych
	Entity Framework Core,
	migracje bazy danych.

4. Warstwa bazy danych
	Microsoft SQL Server.

==================================================
Wykorzystane technologie
==================================================
.NET 9.0
ASP.NET Core MVC
C#
Entity Framework Core
SQL Server
Razor Pages / Razor Views
HTML5
CSS
JavaScript
PayU

==================================================
Wymagania systemowe
==================================================
.NET SDK 9.0+
SQL Server
SQL Server Management Studio
Visual Studio 2022+

==================================================
Instalacja i uruchomienie
==================================================
1. Klonowanie repozytorium
	git clone https://github.com/LastNightWasCrazy/GamblingBuddies
	cd GamblingBuddies

2. Konfiguracja bazy danych w pliku appsettings.json
	{
  		"ConnectionStrings":
		{
    			"DefaultConnection": "Server=localhost;Database=GamblingBuddies;Trusted_Connection=True;TrustServerCertificate=True;"
  		}
	}

3. Utworzenie bazy danych w konsoli Menadżera Pakietów:
dotnet ef database update

4. Uruchomienie aplikacji w konsoli Menadżera Pakietów (dotnet run) lub poprzez uruchomienie projektu w Visual Studio.

5. Dostęp do swaggera:
dodanie /swagger do linku głównej strony.

==================================================
Szczegółowa dokumentacja
==================================================
Dokumentacja techniczna projektu została wygenerowana za pomocą narzędzia Doxygen.