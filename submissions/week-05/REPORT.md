# Week 05 - TaskFlow

## 1. Introducere

In aceasta saptamana am continuat lucrul la proiectul TaskFlow. Am lucrat cu Entity Framework Core si SQL Server si am modificat mai multe parti ale aplicatiei pentru ca datele sa fie gestionate direct din baza de date.

Am lucrat la filtrarea sarcinilor, taguri, validari, statistici si la optimizarea interogarilor.

## 2. Filtrarea sarcinilor

Am creat clasa TaskFilter, prin care pot filtra sarcinile dupa status, prioritate, proiect sau text.

Am adaugat metoda SearchAsync in ITaskRepository si am implementat-o in repository-urile EF Core, InMemory si JSON.

Am modificat si TaskService, astfel incat metoda GetAllAsync sa foloseasca noua metoda de cautare.

Am testat filtrarea cu urmatoarea cerere:

GET /api/tasks?status=Todo&priority=High&search=Test

Cererea a returnat 200 OK si doua sarcini care corespundeau conditiilor.


## 3. Lucrul cu tagurile

Am creat EfTagRepository si am adaugat metode pentru obtinerea tagurilor, cautarea dupa ID sau nume, obtinerea mai multor taguri dupa ID si adaugarea unor taguri noi.

Am creat si clasa TagSummary, care contine ID-ul, numele tagului si numarul de sarcini asociate.

Am modificat TagDto pentru a include TaskCount si am inregistrat noul repository in Program.cs.

Am testat GET /api/tags si am primit lista tagurilor impreuna cu numarul de sarcini pentru fiecare.

La testare am obtinut urmatoarele rezultate:

```json
[
  {
    "id": 1,
    "name": "backend",
    "taskCount": 2
  },
  {
    "id": 2,
    "name": "frontend",
    "taskCount": 1
  },
  {
    "id": 1001,
    "name": "testing",
    "taskCount": 1
  },
  {
    "id": 3,
    "name": "urgent",
    "taskCount": 1
  }
]
```
## 4. Inlocuirea tagurilor unei sarcini

Am adaugat endpoint-ul PUT /api/tasks/{id}/tags, care permite schimbarea tagurilor unei sarcini.

In repository am incarcat sarcina impreuna cu tagurile existente, apoi am inlocuit lista veche cu cea noua si am salvat modificarile.


## 5. Verificarea proiectelor

Am adaugat verificari pentru ProjectId atunci cand se creeaza sau se actualizeaza o sarcina.

Daca este trimis un ID de proiect care nu exista, aplicatia returneaza 400 Bad Request si explica problema de validare.

Am verificat atat cererea POST, cat si PUT, iar ambele au returnat rezultatul asteptat.

## 6. Statistici pentru sarcini

Am adaugat endpoint-ul GET /api/tasks/stats. Acesta afiseaza numarul total de sarcini, numarul lor in functie de status si prioritate, precum si numarul sarcinilor restante.

Pentru gruparea sarcinilor am folosit GroupBy, iar pentru numararea celor restante am folosit CountAsync.

La testare am primit 200 OK. Rezultatul a fost urmatorul:

```json
{
  "totalTasks": 5,
  "tasksByStatus": [
    { "status": "Done", "count": 1 },
    { "status": "InProgress", "count": 1 },
    { "status": "Todo", "count": 3 }
  ],
  "tasksByPriority": [
    { "priority": "Critical", "count": 1 },
    { "priority": "High", "count": 1 },
    { "priority": "Low", "count": 1 },
    { "priority": "Medium", "count": 2 }
  ],
  "overdueTasks": 1
}
```

Am folosit ToQueryString() pentru a vedea interogarile SQL generate de EF Core.

Pentru gruparea dupa status:

```sql
SELECT [t].[Status], COUNT(*) AS [Count]
FROM [Tasks] AS [t]
GROUP BY [t].[Status];
```

Pentru gruparea dupa prioritate:

```sql
SELECT [t].[Priority], COUNT(*) AS [Count]
FROM [Tasks] AS [t]
GROUP BY [t].[Priority];
```

Pentru numarul sarcinilor restante:

```sql
SELECT COUNT(*)
FROM [Tasks] AS [t]
WHERE [t].[Status] <> N'Done'
    AND [t].[DueDate] IS NOT NULL
    AND CONVERT(date, [t].[DueDate]) < @now_Date;
```

In total, endpoint-ul a executat 3 comenzi SQL. In logul API timpul de raspuns a fost de aproximativ 96 ms.

## 7. Problema N+1

Am lucrat si la exercitiul despre problema N+1. Pentru test am creat temporar 10 proiecte, iar dupa terminarea verificarilor le-am sters.

Problema N+1 apare atunci cand aplicatia executa o interogare pentru obtinerea datelor principale si apoi alte interogari pentru fiecare element.

Nu am pastrat valorile exacte pentru numarul de interogari si timpul de raspuns inainte si dupa optimizare, asa ca nu am trecut cifre pe care nu le pot confirma.

## 8. Indexul compus

Am adaugat un index compus pentru Status si DueDate in TaskItemConfiguration.

```csharp
builder.HasIndex(t => new { t.Status, t.DueDate })
    .HasDatabaseName("IX_Tasks_Status_DueDate");

builder.HasIndex(t => t.DueDate);
```
