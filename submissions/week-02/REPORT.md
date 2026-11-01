1. Rezultate

- Build: reușit (`dotnet build TaskFlow.slnx`).
- Teste: 16 reușite, 0 eșuate, 0 omise.

2. Principiile SOLID

| Principiu | Clasă și fișier din proiect | Explicație |
|---|---|---|
| S — Single Responsibility | `TaskExporter` — `src/TaskFlow.Domain/Services/TaskExporter.cs` | Clasa are responsabilitatea de a transforma sarcinile în randuri de export  |
| O — Open/Closed | `DueSoonReport` — `src/TaskFlow.Domain/Reports/DueSoonReport.cs` | Raportul nou extinde funcționalitatea printr-o clasă nouă |
| L — Liskov Substitution | `InMemoryTaskRepository` — `src/TaskFlow.Domain/Repositories/InMemoryTaskRepository.cs` | Implementarea respectă contractul ITaskRepository  |
| I — Interface Segregation | `INotifier` — `src/TaskFlow.Domain/Abstractions/INotifier.cs` și `ITaskRepository` — `src/TaskFlow.Domain/Abstractions/ITaskRepository.cs` | Responsabilitățile sunt separate in interfeee  |
| D — Dependency Inversion | `TaskService` — `src/TaskFlow.Domain/Services/TaskService.cs` | Serviciul depinde de abstracțiile ITaskRepository și INotifier  |

Aș adăuga implementarea nouă și aș modifica selecția repository-ului din `Program.cs` pentru a folosi baza de date. Dacă noua implementare respectă contractul existent, logica principală din `TaskService` nu ar trebui să fie rescrisă.

Numerele de linie sunt cele verificate în versiunea curentă a proiectului și se pot schimba după modificări ulterioare.

Timpul petrecut și probleme întâlnite
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

