# Raport – Saptamana 04

**Proiect:** TaskFlow  
**Tema:** SQL Server si T-SQL  
**Data:** 16.11.2026

## 1. Ce am realizat

Am lucrat cu baza de date TaskFlowLab si am realizat urmatoarele:

- Am creat tabelele si relatiile dintre ele.
- Am verificat cheile primare, cheile externe si constrangerile.
- Am normalizat datele pana la forma 3NF.
- Am realizat interogarile SQL si vederea vw_TaskDetails.
- Am creat indecsi si am analizat planurile de executie.
- Am creat proceduri stocate si am testat tranzactiile.
- Am adaugat tabelele TimeEntries, Activities si TimeEntryActivities.
- Am creat 5000 de inregistrari de timp si 10000 de legaturi cu activitati.
- Am realizat cele 8 interogari suplimentare, vederea vw_UserWorkload si procedurile noi.

## 2. Rezultate

In urma testelor pentru tema suplimentara:

| Test | Rezultat |
|---|---:|
| TimeEntries generate | 5000 |
| TimeEntryActivities generate | 10000 |
| Sarcini mutate | 2000 |
| Sarcini inainte de rollback | 3001 |
| Sarcini dupa rollback | 3001 |
| Eroare acces direct nepermis | 229 |
| ID returnat de usp_LogTime | 5001 |

Testele procedurilor si ale permisiunilor au functionat. Tranzactia a fost anulata corect atunci cand am folosit un autor inexistent.

## 3. Indecsi

Am creat indecsi pentru cautarea inregistrarilor dupa sarcina, utilizator, data si activitate.

Am verificat statisticile I/O si planurile de executie inainte si dupa creare. Capturile si valorile exacte ale logical reads vor fi adaugate dupa verificarea planurilor.

## 4. Probleme intalnite

Am avut nevoie sa corectez unele interogari pentru a folosi denumirile corecte ale coloanelor. Am verificat si ordinea crearii tabelelor, relatiile dintre ele si permisiunile utilizatorului TaskFlowApp.

Dupa corectii, scriptul 06-homework.sql s-a executat fara erori.

## 5. Timpul de lucru

| Activitate | Timp estimat |
|---|---:|
| Schema si normalizare | 1 h |
| Date si interogari | 2 h |
| Indecsi si proceduri | 2 h |
| Tema suplimentara | 2 h |
| ERD si raport | 30 min |
| **Total estimat** | **7 h 30 min** |

