# Saptamana 01 raport

## Ce s-a facut
- Am configurat solutia TaskFlow pentru .NET 10
- Am creat proiectele `TaskFlow.Domain` si `TaskFlow.ConsoleApp`
- Am definit enumerarile `TaskPriority` si `TaskItemStatus`
- Am implementat entitatile `TaskItem` si `Project`
- Am adaugat validarea datelor si metodele pentru gestionarea starii sarcinilor
- Am implementat citirea si validarea datelor prin `ConsoleInput`
- Am creat meniul de consola pentru adaugarea afisarea pornirea si finalizarea sarcinilor
- Am implementat afisarea sarcinilor restante
- Am compilat si testat aplicatia cu `dotnet build` si `dotnet run`
- Am creat documentatia si raportul laboratorului

## Timpul consumat

| Partea | Ore |
|---|---:|
| Teorie | 3 |
| Laborator | 1.2 |
| Tema pentru acasa | 1 |

## Probleme si cum le-am rezolvat
- La integrarea clasei `ConsoleInput` au aparut erori de compilare si le-am rezolvat corectand codul
- Meniul nu se afisa corect si am corectat fisierul `Program.cs`

## Raspunsuri la intrebarile de autoverificare
1. In C# var deduce tipul la compilare iar in JavaScript let permite schimbarea tipului in timpul executiei
2. Lista afiseaza 1 deoarece a si b refera aceeasi lista iar pentru int valoarea lui a ramane 0
3. string este un tip referinta imuabil iar o modificare creeaza un sir nou
4. Campul stocheaza valoarea iar proprietatea permite validarea ei prin `Title`
5. Status are private set pentru a preveni schimbarea directa a starii
6. Membrii unei clase sunt implicit `private` iar clasele de nivel superior sunt implicit `internal`
7. `string?` permite valoarea `null` iar `DateTime?` este un tip valoare nullable
8. `(TaskPriority)42` se compileaza deoarece conversia este permisa iar `Enum.IsDefined` verifica valoarea
9. `ArgumentException` indica un argument invalid iar `InvalidOperationException` indica o operatie interzisa in starea curenta
10. `TryParse` verifica valorile invalide fara sa arunce exceptii pentru conversia nereusita
11. `Complete` si `IsOverdue` primesc `now` pentru a putea folosi acelasi moment si pentru a facilita testarea
12. `TaskFlow.Domain` nu depinde de `ConsoleApp` pentru ca logica de domeniu sa poata fi reutilizata
