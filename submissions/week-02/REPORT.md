# Săptămâna 02 — Tema pentru acasă TaskFlow

## 1. Rezultate

- Build: reușit (`dotnet build TaskFlow.slnx`).
- Teste: 16 reușite, 0 eșuate, 0 omise.
- Funcționalități implementate: raportul termenelor apropiate, filtrarea după stare și prioritate, exportul JSON și teste unitare suplimentare.

## 2. Principiile SOLID

| Principiu | Clasă și fișier din proiect | Explicație |
|---|---|---|
| S — Single Responsibility | `TaskExporter` — `src/TaskFlow.Domain/Services/TaskExporter.cs` | Clasa are responsabilitatea de a transforma sarcinile în randuri de export  |
| O — Open/Closed | `DueSoonReport` — `src/TaskFlow.Domain/Reports/DueSoonReport.cs` | Raportul nou extinde funcționalitatea printr-o clasă nouă |
| L — Liskov Substitution | `InMemoryTaskRepository` — `src/TaskFlow.Domain/Repositories/InMemoryTaskRepository.cs` | Implementarea respectă contractul ITaskRepository  |
| I — Interface Segregation | `INotifier` — `src/TaskFlow.Domain/Abstractions/INotifier.cs` și `ITaskRepository` — `src/TaskFlow.Domain/Abstractions/ITaskRepository.cs` | Responsabilitățile sunt separate in interfeee  |
| D — Dependency Inversion | `TaskService` — `src/TaskFlow.Domain/Services/TaskService.cs` | Serviciul depinde de abstracțiile ITaskRepository și INotifier  |

## 3. Cum schimbăm locul de stocare a sarcinilor?

Pentru a folosi o bază de date în locul stocării actuale, aș crea o implementare nouă, de exemplu `SqlTaskRepository`, care implementează interfața `ITaskRepository`.

Fișiere și linii relevante:

- `src/TaskFlow.Domain/Abstractions/ITaskRepository.cs`, linia 6: interfața care definește contractul repository-ului.
- `src/TaskFlow.Domain/Repositories/InMemoryTaskRepository.cs`, linia 7: implementarea în memorie.
- `src/TaskFlow.Domain/Repositories/JsonFileTaskRepository.cs`, linia 9: implementarea bazată pe fișier JSON.
- `src/TaskFlow.ConsoleApp/Program.cs`, liniile 18–20: alegerea implementării repository-ului.
- `src/TaskFlow.ConsoleApp/Program.cs`, linia 28: crearea serviciului cu repository-ul selectat.
- `src/TaskFlow.Domain/Services/TaskService.cs`, linia 8: serviciul implementează `ITaskService` și utilizează abstracțiile domeniului.

Aș adăuga implementarea nouă și aș modifica selecția repository-ului din `Program.cs` pentru a folosi baza de date. Dacă noua implementare respectă contractul existent, logica principală din `TaskService` nu ar trebui să fie rescrisă.

Numerele de linie sunt cele verificate în versiunea curentă a proiectului și se pot schimba după modificări ulterioare.

## 4. Timpul petrecut și probleme întâlnite

Completează duratele cu timpul efectiv lucrat.

| Sarcină | Timp petrecut |
|---|---|
| 1. Raportul `DueSoonReport` | 1h |
| 2. Filtrarea după stare și prioritate | 1.20h |
| 3. Exportul JSON | 30 min |
| 4. Testele unitare | 20 min |
| 5. Raportul și verificarea finală |5 min |

Probleme întâlnite:
- Verificarea comportamentului la limitele intervalului de 7 zile.
- Testarea comportamentului pentru date fixe, fără folosirea `DateTime.Now` în teste.
- Verificarea compatibilității noilor funcționalități cu interfețele existente.
- Pregătirea capturilor de ecran și documentarea rezultatelor.

Păstrează doar problemele pe care le-ai întâlnit efectiv și adaptează lista dacă este necesar.

