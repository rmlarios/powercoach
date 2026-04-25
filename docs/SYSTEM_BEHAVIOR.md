# PowerCoach: Funcionamiento del Sistema (Fin de la Fase 1)

Este documento explica cómo está estructurada internamente la aplicación y cómo interactúan las distintas piezas tras finalizar la limpieza de código simulado (Mock Data) de la Fase 1.

---

## 1. Arquitectura General y Flujo de Comunicación

La aplicación funciona usando una arquitectura de capas separadas clásica:
- **Backend (API en .NET 9):** Se ejecuta dentro de un contenedor Docker (`powercoach-api-1`) en el puerto `5000`. Sirve como la fuente de la verdad para la base de datos PostgreSQL, implementando la lógica de negocio y seguridad.
- **Frontend (Next.js 14):** Se ejecuta usando `npm run dev` en el puerto `3003`. Es una Single Page Application (SPA) que se comunica puramente por endpoints REST (`/api/...`) al backend en .NET.

## 2. Flujo de Autenticación (Auth Flow)

Para prevenir errores (como el loop infinito de redirecciones de login solucionado), la asimilación del usuario mediante tokens JWT se administra en sincronía de la siguiente forma:

1. **Login (`/auth/login`):**
   - El usuario introduce sus credenciales (`admin@powercoach.com`).
   - El backend valida y envía un **Access Token** (JWT con vida útil de 15 minutos) y un **Refresh Token** (vida útil de 7 días).
   - **Frontend:** Guarda el Access Token y Refresh Token temporalmente en memoria/`localStorage`. Al mismo tiempo, inyecta una cookie `cp_access_token` para que los `middleware` de Next.js sepan que el usuario tiene acceso.

2. **Refresco de Token automático:**
   - La instancia de `axios` (`apiClient` en el archivo `client.ts`) actúa como interceptor. 
   - Si el backend rechaza una petición (código `401 Unauthorized`), el interceptor captura la caída automáticamente, ejecuta el proceso de uso del *Refresh Token* para obtener un nuevo JWT Access Token, actualiza la cookie `cp_access_token` y luego reintenta la petición original (todo esto de manera invisible al usuario).

3. **Middleware de Rutas Protegidas:**
   - Next.js usa el archivo `middleware.ts` en cada navegación de página (como moverse a `/dashboard` o `/workout`). 
   - El sistema de ruteo simplemente lee la presencia de la cookie `cp_access_token`. Si no está, redirige brutalmente al login (`/login`).

## 3. Estado del Frontend y Peticiones API

PowerCoach utiliza **TanStack Query** (antes React Query) para administrar las peticiones a la API y sincronizarlas con la UI. 

En la Fase 1, se eliminó la constante `USE_MOCK_DATA` que fingía las respuestas en las páginas principales. Ahora todo funciona usando esta cadena interactiva real:

* **Endpoint Real:** Los hooks personalizados (ej: `useTodayWorkout`, `useStartWorkout`, `useExerciseLiftHistory`) en `src/hooks/...` llaman a las rutas del backend expuestas.
* **Caché en Memoria:** TanStack almacena la respuesta en caché. Si el usuario navega a otra ruta y regresa, la información aparece instantáneamente y hace una recarga paralela silenciosa en el fondo.
* **Componentes Visuales Funcionales:** Las páginas simplemente dependen del estado estricto (ej: `isLoading` y `data`).

### ¿Cómo maneja el Contexto al Usuario Logueado (El problema del `athleteId`)?
En lugar de pasar quién está logueado a cada vista usando URLs como `?athleteId=123`, centralizamos la identidad en el `AuthProvider` (`src/providers/auth-provider.tsx`).
1. Todas las páginas llaman al gancho `const { user } = useAuth();`
2. Si es un atleta el que abre la aplicación web, obtenemos su ID del JWT del token directamente con `user?.athleteId`.
3. Esto sirve para inyectar su identificador seguro a las peticiones reales como su registro de Workout (Tarea 1.3).

---

## 4. Cambios Claves Completados en la Fase 1

### A. Dashboard (`DashboardController`)
- **Problema previo:** El Dashboard de coach era un endpoint "público" sin barreras y pedía pertenecer explícitamente a un inquilino/coach aunque fueras Admin, ocasionando error 401. 
- **Solución implementada:** Se colocó explícitamente `[Authorize]` en en el backend (`CoachPlatform.API`). Y se removió la interfaz excesiva de validación en la consulta MediatR `GetCoachDashboardQuery.cs`. Las tarjetas del Dashboard ahora enseñan datos reales de la BD Postgresql.

### B. Builder de Programas (Max Lifts / PRs)
- **Problema Previo:** Cargar datos de ejercicios mezclaba listas locales y provocaba fallos de TypeScript por discrepancias de tipo entre el API de .NET real y los Mock Data locales obsoletos de prueba en Next.js.
- **Solución implementada:** Se removió la carga ficticia `getMockAthleteMaxLifts`. Se configuró la página para que la memoria y las visualizaciones de 1RM dependan nativamente de lo que diga la BD real. Las discrepancias de Typings (Ej: `.primaryMuscleGroup` vs `.muscleGroup`) se mapearon ordenadamente usando `useMemo()`.

### C. Sistema de Seguimiento de Entrenamiento (`/workout`)
- **Problema Previo:** Toda la vista dependía de "estado falso local simulado" (`mockTarget`, `mockWeekWorkouts`).
- **Solución implementada:** 
  1. Se borró toda la rama estática de simulaciones `USE_MOCK_DATA`.
  2. Implementamos **Auto-Save real a la BD**: cuando la persona da "check" o guarda una serie (`handleSaveSet`), dispara una mutación oculta (debriefing y *debounce* de 600ms) que inserta la data de entrenamiento directo a la tabla SQL usando `useSaveSet()` > `PUT /api/athletes/.../sets`.
  3. Manejo de estados de inactividad (`EmptyWorkout` se muestra si el API retorna `Code 204: No Content` de que HOY no toca rutina).

## ¿Qué sigue ahora? (Fase 2)
Una vez establecidas las fundaciones anteriores:
Las tablas de base de datos y la conexión a las UI ya son confiables. El siguiente paso (`Fase 2`) es explotar esos datos:
1. Construir el apartado visual para que el Coach vea reportes (UI de métricas pasadas o Check-ins).
2. Construir la UI de las gráficas de progreso del Atleta. Se usará exactamente la misma disciplina del uso de **TanStack + Contexto JWT** real con la API actual.