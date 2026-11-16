# Normalizarea bazei de date TaskFlowLab

## 1. Analiza tabelului inițial

Tabelul primit conține informații despre timpul petrecut de angajați pentru realizarea diferitelor sarcini. Acesta include data lucrului, datele angajatului, informațiile despre sarcină și proiect, numărul de ore și activitățile efectuate.

La prima vedere, tabelul este ușor de înțeles, dar are câteva probleme. Unele informații se repetă, de exemplu numele și emailul angajaților sau denumirile proiectelor. De asemenea, coloana `Activities` poate conține mai multe activități în aceeași celulă.

Pentru a evita repetarea inutilă a datelor și pentru a organiza mai bine baza de date, tabelul va fi normalizat în trei etape.

## 2. Prima formă normală (1NF)

Prima formă normală presupune ca fiecare celulă să conțină o singură valoare, fără liste sau grupuri de valori.

În tabelul inițial, coloana `Activities` nu respectă această regulă. De exemplu, pentru sarcina cu ID-ul 3, angajatul Ion Rusu a înregistrat activitățile „coding, review” în aceeași celulă.

Pentru a rezolva problema, activitățile au fost separate și trecute pe rânduri diferite.

Exemplu:

| EntryDate | UserEmail | TaskId | Hours | Activity |
|---|---|---:|---:|---|
| 2026-10-12 | ion.rusu@taskflow.local | 3 | 2.5 | coding |
| 2026-10-12 | ion.rusu@taskflow.local | 3 | 2.5 | review |

După această modificare, fiecare câmp conține o singură valoare.

Pentru tabelul rezultat, cheia candidat poate fi considerată combinația:

`(EntryDate, UserEmail, TaskId, Activity)`

Aceasta identifică fiecare activitate înregistrată pentru un angajat, la o anumită sarcină și dată.

## 3. A doua formă normală (2NF)

A doua formă normală presupune ca tabelul să respecte prima formă normală și fiecare atribut care nu face parte din cheie să depindă de întreaga cheie primară, nu doar de o parte a acesteia.

În tabelul obținut după prima etapă există mai multe dependențe funcționale:

- `UserEmail → UserName`
- `TaskId → TaskTitle, ProjectName`
- `ProjectName → ProjectOwnerEmail`
- `(EntryDate, UserEmail, TaskId) → Hours`

De exemplu, numele angajatului depinde doar de emailul acestuia. La fel, titlul sarcinii și proiectul din care face parte depind de `TaskId`, nu de întreaga cheie compusă.

Pentru a elimina aceste dependențe parțiale, informațiile au fost împărțite în mai multe tabele:

**Users**

| UserEmail (PK) | UserName |
|---|---|
| ion.rusu@taskflow.local | Ion Rusu |
| maria.ceban@taskflow.local | Maria Ceban |

**Tasks**

| TaskId (PK) | TaskTitle | ProjectName | ProjectOwnerEmail |
|---|---|---|---|
| 3 | Deploy build #3 | Mobile App | stefan.turcanu@taskflow.local |
| 8 | Fix bug #8 | Website | ana.popescu@taskflow.local |

**TimeEntries**

| EntryDate | UserEmail | TaskId | Hours |
|---|---|---:|---:|
| 2026-10-12 | ion.rusu@taskflow.local | 3 | 2.5 |
| 2026-10-12 | ion.rusu@taskflow.local | 8 | 1.0 |
| 2026-10-13 | maria.ceban@taskflow.local | 3 | 4.0 |

**Activities**

| ActivityId (PK) | ActivityName |
|---|---|
| 1 | coding |
| 2 | review |
| 3 | testing |

**TimeEntryActivities**

Acest tabel păstrează legătura dintre înregistrările de timp și activitățile efectuate. El conține cheia înregistrării de timp și ID-ul activității.

În urma acestei etape, informațiile despre angajați, sarcini și activități nu mai trebuie repetate în fiecare înregistrare de timp.

## 4. A treia formă normală (3NF)

A treia formă normală presupune ca tabelul să fie în forma 2NF și să nu existe dependențe tranzitive între atributele care nu fac parte din cheie.

În etapa anterioară am observat că în tabelul `Tasks` există următoarea dependență:

`TaskId → ProjectName → ProjectOwnerEmail`

Cu alte cuvinte, ID-ul sarcinii determină proiectul, iar proiectul determină emailul proprietarului său. Astfel, informația despre proprietarul proiectului nu trebuie păstrată în tabelul sarcinilor.

Pentru a elimina această dependență, am separat datele proiectelor într-un tabel propriu, numit `Projects`.

Structura devine:

**Projects**

| ProjectId (PK) | ProjectName | OwnerId (FK) |
|---|---|---|
| 1 | Mobile App | ID-ul proprietarului |
| 2 | Website | ID-ul proprietarului |

**Tasks**

| TaskId (PK) | TaskTitle | ProjectId (FK) |
|---|---|---:|
| 3 | Deploy build #3 | 1 |
| 8 | Fix bug #8 | 2 |

În baza de date finală, proprietarul proiectului este identificat prin `OwnerId`, care face legătura cu tabelul `Users`.

Astfel, informațiile despre proiecte sunt păstrate într-un singur loc, iar modificarea proprietarului unui proiect nu necesită modificarea tuturor sarcinilor asociate.

## 5. Structura finală a bazei de date

După aplicarea celor trei forme normale, evidența timpului este organizată folosind tabelele existente din baza de date TaskFlowLab și trei tabele noi.

Tabelele folosite sunt:

| Tabel | Rol |
|---|---|
| Users | Păstrează datele angajaților |
| Projects | Păstrează informațiile despre proiecte și proprietari |
| Tasks | Păstrează sarcinile și proiectele din care fac parte |
| TimeEntries | Înregistrează timpul lucrat de fiecare angajat |
| Activities | Păstrează lista activităților disponibile |
| TimeEntryActivities | Face legătura dintre timpul înregistrat și activități |

### Chei și relații

- `Users.Id` – cheie primară pentru angajați.
- `Projects.Id` – cheie primară pentru proiecte.
- `Projects.OwnerId` – cheie externă către `Users.Id`.
- `Tasks.Id` – cheie primară pentru sarcini.
- `Tasks.ProjectId` – cheie externă către `Projects.Id`.
- `TimeEntries.Id` – cheie primară pentru înregistrările de timp.
- `TimeEntries.TaskId` – cheie externă către `Tasks.Id`.
- `TimeEntries.UserId` – cheie externă către `Users.Id`.
- `Activities.Id` – cheie primară pentru activități.
- `TimeEntryActivities` – are cheia primară compusă din `TimeEntryId` și `ActivityId`, ambele fiind chei externe.

