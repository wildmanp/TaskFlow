
# TaskFlowLab — Entity Relationship Diagram

Diagrama prezintă structura bazei de date TaskFlowLab, inclusiv tabelele pentru gestionarea proiectelor, sarcinilor, comentariilor, etichetelor și evidenței timpului de lucru.

```mermaid
erDiagram
    Users ||--o{ Projects : "owns"
    Users |o--o{ Tasks : "is assigned"
    Projects |o--o{ Tasks : "contains"
    Tasks ||--o{ TaskTags : "has"
    Tags ||--o{ TaskTags : "marks"
    Tasks ||--o{ TaskComments : "has"
    Users ||--o{ TaskComments : "writes"

    Users ||--o{ TimeEntries : "logs"
    Tasks ||--o{ TimeEntries : "tracks"
    TimeEntries ||--o{ TimeEntryActivities : "includes"
    Activities ||--o{ TimeEntryActivities : "categorizes"

    Users {
        int Id PK
        nvarchar Email UK
        nvarchar DisplayName
        bit IsActive
        datetime2 CreatedAt
    }

    Projects {
        int Id PK
        nvarchar Name UK
        nvarchar Description
        int OwnerId FK
        datetime2 CreatedAt
    }

    Tasks {
        int Id PK
        nvarchar Title
        nvarchar Description
        tinyint Priority
        tinyint Status
        datetime2 DueDate
        datetime2 CompletedAt
        int ProjectId FK
        int AssigneeId FK
        datetime2 CreatedAt
    }

    Tags {
        int Id PK
        nvarchar Name UK
    }

    TaskTags {
        int TaskId PK, FK
        int TagId PK, FK
    }

    TaskComments {
        int Id PK
        int TaskId FK
        int AuthorId FK
        nvarchar Body
        datetime2 CreatedAt
    }

    TimeEntries {
        int Id PK
        int TaskId FK
        int UserId FK
        date WorkDate
        decimal Hours
        nvarchar Note
        datetime2 CreatedAt
    }

    Activities {
        int Id PK
        nvarchar Name UK
    }

    TimeEntryActivities {
        int TimeEntryId PK, FK
        int ActivityId PK, FK
    }
```

## Relațiile dintre entități

### Relații 1:N

- Users → Projects: un utilizator poate deține mai multe proiecte.
- Projects → Tasks: un proiect poate conține mai multe sarcini.
- Users → Tasks: un utilizator poate avea mai multe sarcini atribuite.
- Tasks → TaskComments: o sarcină poate avea mai multe comentarii.
- Users → TaskComments: un utilizator poate scrie mai multe comentarii.
- Tasks → TimeEntries: o sarcină poate avea mai multe înregistrări de timp.
- Users → TimeEntries: un utilizator poate înregistra mai multe perioade de lucru.
- TimeEntries → TimeEntryActivities: o înregistrare de timp poate avea mai multe activități.
- Activities → TimeEntryActivities: o activitate poate apărea în mai multe înregistrări.

### Relații N:M

- Tasks ↔ Tags: implementată prin tabelul intermediar `TaskTags`.
- TimeEntries ↔ Activities: implementată prin tabelul intermediar `TimeEntryActivities`.

### Constrângeri importante

- `TimeEntries.Id` este cheia primară.
- `TimeEntries.TaskId` referă `Tasks.Id` cu ON DELETE CASCADE.
- `TimeEntries.UserId` referă `Users.Id` cu ON DELETE NO ACTION.
- Combinația `(TaskId, UserId, WorkDate)` este unică.
- `TimeEntries.Hours` trebuie să fie mai mare decât 0 și cel mult 24.
- `Activities.Name` este unic.
- `TimeEntryActivities` utilizează cheia primară compusă `(TimeEntryId, ActivityId)`.
