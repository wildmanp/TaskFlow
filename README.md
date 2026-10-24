# TaskFlow

Manager de sarcini didactic (cursul C#/.NET junior).

## Cerinte
- .NET SDK 10.0.x

## Compilare si rulare
```bash
dotnet build TaskFlow.slnx
dotnet run --project src/TaskFlow.ConsoleApp
```

## Structura
- `src/TaskFlow.Domain`: modelul de domeniu (`TaskItem`, `Project`, enumerari)
- `src/TaskFlow.ConsoleApp`: meniu de consola peste lista de sarcini din memorie
- `submissions/week-01/REPORT.md`: raportul laboratorului din saptamana 01
