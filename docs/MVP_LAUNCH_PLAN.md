# PowerCoach MVP Launch Plan

## Resumen del Estado Actual
**Backend:** Estructuralmente completo al 90% (Auth, Programas, Workouts, Suscripciones, Check-ins).
**Frontend (`coach-dashboard`):** Base sólida pero depende de `Mock Data` para flujos clave como el Dashboard, registro de entrenamientos y creador de programas.

Para alcanzar un **Minimum Viable Product (MVP)** publicable, debemos conectar las vistas restantes del frontend a la API real, construir las interfaces orientadas al Atleta, e implementar las UI de Check-in y Suscripciones.

---

## Fases de Implementación

### Fase 1: "Des-Mockear" el Coach Dashboard (Integración) - ✅ COMPLETADO
1. **Conectar `/dashboard`** a la API real `DashboardController` para métricas (Eliminado `USE_MOCK_DATA`, añadido token de autenticación). ✅
2. **Conectar `/programs/[id]/builder`** a la API real de Atletas para obtener los Max Lifts (PRs) verdaderos habilitando cálculos precisos basados en %. ✅
3. **Conectar `/workout`** a los endpoints reales de lectura/escritura del `WorkoutsController` (Estado y mocks removidos, auto-save conectado, lectura de URL reemplazada por Contexto de Autenticación). ✅

### Fase 2: Interfaz de Check-ins y Progreso (Coach) - ✅ COMPLETADO
1. Construir la pantalla de listado y detalle de **Check-ins** consumiendo `CheckInsController` (pendientes de revisión). ✅
2. Desarrollar gráficas de **Lift History** y seguimiento de PRs en la vista de perfil de cada Atleta. ✅

### Fase 3: Portal del Atleta (Web progresiva enfocada a móvil) - ✅ COMPLETADO
1. Implementar enrutamiento basado en roles en el frontend (Admin vs Coach vs Atleta). ✅
2. Construir el **Dashboard del Atleta**: "Entrenamiento de Hoy", calendario de la semana y programa activo. ✅
3. Construir la **UI de Ejecución de Entrenamiento** (optimizada para móviles): registrar series, repeticiones, peso real, RPE y subir videos. ✅ (reutiliza `/workout` existente)
4. Construir el formulario de **Check-in para Atletas**: peso corporal, nivel de fatiga, notas, etc. ✅

### Fase 4: Facturación, Solicitudes (Onboarding) y Lanzamiento - ✅ COMPLETADO
1. Construir el formulario público de **Applications (Solicitudes)** para nuevos prospectos. ✅
2. Construir la UI de **Planes y Suscripciones** para que el Coach gestione facturación. ✅
3. Pulir la UI, diseño responsivo (especialmente portal de atleta) y configuración de Docker para producción. ✅

---

## Archivos Relevantes (Objetivos Inmediatos)
- `coach-dashboard/src/app/(dashboard)/dashboard/page.tsx` — Remover `USE_MOCK_DATA`.
- `coach-dashboard/src/app/(dashboard)/programs/[id]/builder/page.tsx` — Conectar PRs reales.
- `coach-dashboard/src/app/(dashboard)/workout/page.tsx` — Integración final con API Workouts.
- `coach-dashboard/src/middleware.ts` — Lógica para redireccionamiento dual: Coach vs Athlete.

---

## Criterios de Verificación MVP
1. **Flujo E2E Coach:** Crear un programa, definir bloques de %, asignarlo a un atleta activo.
2. **Flujo E2E Atleta:** Iniciar sesión en móvil, cargar rutina de hoy, registrar 3 series, enviar check-in.
3. **Datos en Vivo:** El Dashboard del Coach agrega datos reales (atletas activos, check-ins pendientes) sin usar información estática.

---

## Decisiones Técnicas Clave
- El MVP utilizará **un único frontend Next.js** (`coach-dashboard`) con enrutamiento basado en roles (Vista Coach vs Vista Atleta) en lugar de dós repositorios separados para minimizar la sobrecarga de mantenimiento.
- Los **pagos** se iniciarán con un registro manual (usando el `PaymentsController` actual) antes de introducir la complejidad técnica de la integración con pasarelas como Stripe/PayPal en la v1.1.