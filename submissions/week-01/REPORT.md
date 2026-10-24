X# Săptămâna 01: raport

## Ce s-a făcut
- (enumerați pașii laboratorului și punctele temei pentru acasă)
Am configurat solutia TaskFlow pentru .NET 10
Am creat proiectele TaskFlow.Domain si TaskFlow.ConsoleApp
Am definit enumerarile TaskPriority si TaskItemStatus
Am implementat entitatile TaskItem si Project
Am adaugat validarea datelor si metodele pentru gestionarea starii sarcinilor
Am implementat citirea si validarea datelor introduse in consola prin ConsoleInput
Am creat meniul pentru adaugarea, afisarea, pornirea si finalizarea sarcinilor
Am implementat afisarea sarcinilor restante
Am compilat si testat aplicatia cu dotnet build si dotnet run
Am pregatit documentatia proiectului si raportul laboratorului
## Timpul consumat
| Partea | Ore |
|---|---|
| Teorie | 3h|
| Laborator | 1.20h|
| Temă pentru acasă | 1h|

## Probleme și cum le-am rezolvat
-La integrarea clasei ConsoleInput cu aplicatia de consola au aparut erori de compilare.Am corectat codul pana când sa complicat cu success
-Menu nu afire cum trebuie , am schimbat fisierul .cs
## Răspunsuri la întrebările de autoverificare
1.n C# var permite compilatorului sa deduca tipul variabilei la compilare iar tipul ramane fix In JavaScript let declara o variabila al carei tip poate fi diferit in timpul executiei
2.Pentru lista se va afisa 1 deoarece a si b refera aceeasi lista Pentru int valoarea lui a ramane 0 deoarece b primeste o copie a valorii lui a iar b++ modifica doar copia
3.string` este imuabil si orice modificare creeaza un sir nou
4.Campul stocheaza valoarea iar proprietatea permite validarea ei
5.`Status` are `private set` pentru a preveni schimbarea directa a starii
6. Membrii clasei sunt implicit `private` iar clasele de nivel superior `internal`
7.`string?` permite `null` iar `DateTime?` este un tip valoare nullable
8.`(TaskPriority)42` se compileaza dar verificam valoarea cu `Enum.IsDefined`
9.`ArgumentException` indica un argument invalid iar `InvalidOperationException` o operatie interzisa
10.`TryParse` verifica datele fara exceptii pentru valori invalide
11`Complete` si `IsOverdue` primesc `now` pentru testare si folosirea aceluiasi moment
12.`TaskFlow.Domain` nu depinde de `ConsoleApp` pentru a putea fi reutilizat