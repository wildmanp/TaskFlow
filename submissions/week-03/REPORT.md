1. PUT inlocuieste resursa complet, iar PATCH modifica doar o parte. PUT este idempotent.

2. 400 inseamna ca cererea contine date gresite. 409 inseamna ca operatia intra in conflict cu starea actuala a resursei.

3. In timpul await, firul poate fi folosit pentru alte cereri. .Result blocheaza firul si poate incetini aplicatia.

4. CancellationToken vine din HttpContext.RequestAborted si se anuleaza cand clientul inchide conexiunea.

5. id vine din ruta, request din corpul JSON, iar cancellationToken din HttpContext.RequestAborted.

6. Nu returnam direct TaskItem pentru a evita modificarea datelor interne, problemele de serializare si schimbarea contractului API.

7. Enum-ul primeste automat valoarea Todo daca proprietatea lipseste. Folosim TaskItemStatus? pentru a permite null.

8. Singleton este o instanta pentru toata aplicatia, Scoped este o instanta pentru fiecare cerere, iar Transient creeaza o instanta noua la fiecare injectare. Repository-ul este Singleton pentru a pastra datele, iar TaskService este Scoped.

9. Captive dependency apare cand un Singleton pastreaza o dependinta Scoped. In Development verificarile pot detecta aceasta problema la pornire.

10. Valorile vin mai intai din appsettings.json, apoi pot fi suprascrise de appsettings.Development.json si de variabilele de mediu.

11. Middleware-ul de timp trebuie sa fie inainte de UseExceptionHandler pentru a inregistra codul final al raspunsului, inclusiv 500.

12. Clientul primeste status 500 si ProblemDetails cu traceId. Detaliile exceptiei se gasesc in logurile serverului.