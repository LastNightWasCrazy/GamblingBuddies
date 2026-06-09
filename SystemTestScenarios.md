# Koñcowe testy systemu GamblingBuddies

## Scenariusz 1: Logowanie administratora

**Cel:** Sprawdzenie, czy administrator mo¿e zalogowaæ siê do systemu.

**Kroki:**
1. Uruchom aplikacjê.
2. WejdŸ w `Logowanie`.
3. Wpisz dane konta administratora.
4. Kliknij `Zaloguj`.

**Oczekiwany wynik:**
- u¿ytkownik zostaje zalogowany,
- w navbarze pojawia siê `Panel admina`,
- widoczne s¹ modu³y: Sto³y, Gry, Rezerwacje.

---

## Scenariusz 2: Utworzenie rezerwacji

**Cel:** Sprawdzenie, czy u¿ytkownik mo¿e utworzyæ rezerwacjê.

**Kroki:**
1. Zaloguj siê do systemu.
2. WejdŸ w `Rezerwacja`.
3. Wybierz salê.
4. Wybierz grê.
5. Wybierz stó³.
6. Wybierz datê i godzinê z przysz³oœci.
7. Podaj liczbê miejsc.
8. Kliknij `Zarezerwuj`.

**Oczekiwany wynik:**
- system tworzy rezerwacjê,
- rezerwacja otrzymuje status `Pending`,
- pojawia siê komunikat sukcesu.

---

## Scenariusz 3: Blokada rezerwacji w przesz³oœci

**Cel:** Sprawdzenie walidacji b³êdnej daty.

**Kroki:**
1. WejdŸ w formularz rezerwacji.
2. Spróbuj wybraæ datê/godzinê z przesz³oœci.
3. Kliknij `Zarezerwuj`.

**Oczekiwany wynik:**
- system nie tworzy rezerwacji,
- pojawia siê komunikat b³êdu.

---

## Scenariusz 4: Konflikt rezerwacji

**Cel:** Sprawdzenie, czy system blokuje rezerwacjê tego samego sto³u w tym samym czasie.

**Kroki:**
1. Utwórz rezerwacjê dla wybranego sto³u i godziny.
2. Spróbuj utworzyæ drug¹ rezerwacjê dla tego samego sto³u w tym samym przedziale czasowym.

**Oczekiwany wynik:**
- druga rezerwacja zostaje odrzucona,
- pojawia siê komunikat o konflikcie terminu.

---

## Scenariusz 5: Zatwierdzenie rezerwacji przez administratora

**Cel:** Sprawdzenie dzia³ania panelu administracyjnego.

**Kroki:**
1. Zaloguj siê jako administrator.
2. WejdŸ w `Rezerwacje`.
3. Wybierz rezerwacjê ze statusem `Pending`.
4. Kliknij `ZatwierdŸ`.

**Oczekiwany wynik:**
- status rezerwacji zmienia siê na `Confirmed`,
- pojawia siê komunikat sukcesu.

---

## Scenariusz 6: Anulowanie rezerwacji przez administratora

**Cel:** Sprawdzenie anulowania rezerwacji.

**Kroki:**
1. Zaloguj siê jako administrator.
2. WejdŸ w `Rezerwacje`.
3. Kliknij `Anuluj` przy wybranej rezerwacji.

**Oczekiwany wynik:**
- status rezerwacji zmienia siê na `Cancelled`,
- rezerwacja pozostaje widoczna w historii.

---

## Scenariusz 7: Podgl¹d szczegó³ów sto³u

**Cel:** Sprawdzenie danych sto³u i sesji.

**Kroki:**
1. Zaloguj siê jako administrator.
2. WejdŸ w `Sto³y`.
3. Kliknij `Szczegó³y` przy wybranym stole.

**Oczekiwany wynik:**
- system pokazuje salê, min/max graczy, liczbê miejsc,
- system pokazuje sesje przy stole,
- system pokazuje liczbê zarezerwowanych i wolnych miejsc.

---

## Scenariusz 8: Edycja sto³u

**Cel:** Sprawdzenie modyfikacji danych sto³u.

**Kroki:**
1. Zaloguj siê jako administrator.
2. WejdŸ w `Sto³y`.
3. Kliknij `Edytuj`.
4. Zmieñ numer sto³u albo limit graczy.
5. Kliknij `Zapisz`.

**Oczekiwany wynik:**
- dane sto³u zostaj¹ zapisane,
- zmiany s¹ widoczne na liœcie sto³ów.

---

## Scenariusz 9: Edycja gry

**Cel:** Sprawdzenie modyfikacji gry.

**Kroki:**
1. Zaloguj siê jako administrator.
2. WejdŸ w `Gry`.
3. Kliknij `Edytuj`.
4. Zmieñ opis lub status gry.
5. Kliknij `Zapisz`.

**Oczekiwany wynik:**
- dane gry zostaj¹ zapisane,
- zmiany s¹ widoczne na liœcie gier.

---

## Scenariusz 10: Dostêp bez zalogowania

**Cel:** Sprawdzenie autoryzacji.

**Kroki:**
1. Wyloguj siê.
2. Spróbuj wejœæ bezpoœrednio w `/AdminPanel`.
3. Spróbuj wejœæ w `/Reservations`.

**Oczekiwany wynik:**
- system przekierowuje do logowania,
- u¿ytkownik niezalogowany nie ma dostêpu do panelu administracyjnego.