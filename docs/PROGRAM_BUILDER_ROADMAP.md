# Program Builder - Roadmap de Mejoras

> Plan de implementación progresiva para transformar el Program Builder en una herramienta superior al Excel.
> **Objetivo**: Crear la mejor experiencia de programación de entrenamiento para coaches de fuerza.

---

## 📊 Resumen del Roadmap

| Métrica | Valor |
|---------|-------|
| **Fases Totales** | 5 |
| **Features Totales** | 11 |
| **Tasks Estimadas** | ~85 |
| **Tiempo Estimado Total** | 8-10 semanas |
| **✅ Completadas** | 10 (PB-001 → PB-010, todas subtasks) |
| **🟡 En Progreso** | 0 |

**Última actualización**: 2026-03-20

---

## 🎯 Visión del Producto

### Problema Actual
El coach actualmente usa Excel para programar entrenamientos porque ofrece:
- Vista por días (hojas separadas)
- Flexibilidad en notación (`1x1 3x4`, `Single @9`, etc.)
- Cálculo automático de pesos basado en %RM
- Formato familiar y exportable

### Objetivo
Crear un Program Builder que:
1. ✅ Replique la experiencia intuitiva del Excel
2. ✅ Agregue capacidades imposibles en Excel (historial, 1RM automático, visualizaciones)
3. ✅ Genere entregables profesionales (PDF/Excel)
4. ✅ Escale a múltiples atletas sin duplicación manual

---

## 📅 Fases de Implementación

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  FASE 1: Paridad Excel        │  FASE 2: Cálculos      │  FASE 3: Export   │
│  ─────────────────────        │  ──────────────        │  ─────────────    │
│  • Day-Centric View           │  • Auto-Weight con 1RM │  • Export PDF     │
│  • Smart Set Notation         │  • Quick Fill Actions  │  • Export Excel   │
│  • Exercise Type Support      │                        │                   │
│                               │                        │                   │
│  [2-3 semanas]               │  [1-2 semanas]         │  [1-2 semanas]    │
├─────────────────────────────────────────────────────────────────────────────┤
│  FASE 4: Diferenciadores      │  FASE 5: Innovación                        │
│  ────────────────────         │  ─────────────────                         │
│  • Progression Visualizer     │  • AI Suggestions                          │
│  • Exercise Library History   │  • Template Marketplace                    │
│  • Calendar Integration       │  • Athlete Preview Mode                    │
│                               │                        │                   │
│  [2 semanas]                  │  [2+ semanas]                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔴 FASE 1: Paridad con Excel
**Prioridad**: Crítica | **Estimación**: 2-3 semanas | **Sprint**: 1-2

> Objetivo: Que el coach pueda hacer TODO lo que hace en Excel, con la misma velocidad.

---

### PB-001: Day-Centric View (Vista Centrada en Días)
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Estimación**: 5-7 días

Nueva vista que replica el modelo mental del Excel: días como tabs, ejercicios como filas, semanas como columnas.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ 📅 [Día 1] [Día 2] [Día 3] [Día 4] [Apoyo]                    [+ Add Day]  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Exercise              │ S1        │ S2        │ S3        │ S4        │   │
│  ──────────────────────┼───────────┼───────────┼───────────┼───────────┼   │
│  🏋️ Bench Press        │ 2x8  60%  │ 2x8  62%  │ 3x6  65%  │ 3x5  70%  │   │
│     └─ Peso sugerido   │ 190kg     │ 196kg     │ 206kg     │ 221kg     │   │
│                        │           │           │           │           │   │
│  🏋️ Low Bar Squat      │ 3x6  65%  │ 1x1+3x4   │ 1x1+3x4   │ Single @9 │   │
│                        │ 315kg     │ 84%+74%   │ 87%+77%   │ 385kg     │   │
│                        │           │           │           │           │   │
│  🏋️ Prensa TEMPO 3:0:0 │ 2x12      │ 2x12  @9  │ 2x10  @10 │ 2x10      │   │
│                        │ 450kg     │           │           │           │   │
│                        │           │           │           │           │   │
│  [+ Agregar ejercicio]                                                     │
└─────────────────────────────────────────────────────────────────────────────┘
```

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-001-01 | Crear tipo `BuilderViewMode = 'weekly' \| 'progression' \| 'day-centric'` | ✅ | 0.5h | types/builder.ts |
| PB-001-02 | Actualizar `ViewModeSelector` con tercera opción | ✅ | 1h | Icono + tooltip |
| PB-001-03 | Crear componente `DayCentricView` (contenedor principal) | ✅ | 2h | Layout con tabs |
| PB-001-04 | Crear componente `DayTab` (tab individual de día) | ✅ | 2h | Con rename, delete, reorder |
| PB-001-05 | Crear componente `DayExerciseGrid` (tabla de ejercicios) | ✅ | 4h | Columnas = semanas |
| PB-001-06 | Crear componente `WeekCell` (celda editable) | ✅ | 4h | Con parsing de notación |
| PB-001-07 | Implementar reordenamiento de ejercicios con DnD | ✅ | 3h | @dnd-kit |
| PB-001-08 | Implementar agregar/eliminar días | ✅ | 2h | Con confirmación |
| PB-001-09 | Implementar agregar/eliminar ejercicios | ✅ | 2h | Con autocomplete |
| PB-001-10 | Sincronizar cambios con el modelo BuilderState existente | ✅ | 4h | Bidireccional |
| PB-001-11 | Agregar scroll horizontal suave para muchas semanas | ✅ | 1h | Sticky first column |
| PB-001-12 | Implementar keyboard navigation (Tab, Enter, Arrow keys) | ✅ | 3h | UX crítica |
| PB-001-13 | Tests unitarios para transformación de datos | ✅ | 2h | Jest — day-centric-utils.test.ts (19 tests) |

**Criterios de Aceptación**:
- [x] La vista muestra días como tabs horizontales
- [x] Los ejercicios se muestran como filas, semanas como columnas
- [x] Se puede editar cualquier celda inline
- [x] Los cambios se reflejan en las otras vistas (Weekly, Progression)
- [x] Keyboard navigation funciona fluido (Tab entre celdas)
- [x] Se pueden agregar/eliminar días y ejercicios

**Dependencias**: Ninguna (puede empezar inmediatamente)

---

### PB-002: Smart Set Notation (Notación Flexible de Series)
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Estimación**: 3-4 días

Parser y renderer inteligente que soporta todas las notaciones usadas en el Excel del coach.

**Notaciones soportadas**:
```
Básicas:
  "3x8"       → 3 series de 8 reps
  "4x8-12"    → 4 series de 8-12 reps (rango)
  "3x10 @8"   → 3 series de 10 reps a RPE 8

Compuestas:
  "1x1 3x4"   → 1 single + 3 series de 4 (backoff)
  "1x1 3x4 84% 74%"  → Con porcentajes por grupo

Especiales:
  "Single @9"     → 1 rep a RPE 9
  "AMRAP"         → As Many Reps As Possible
  "2x10 EMOM 6"   → EMOM de 6 minutos

Con tempo:
  "3x8 TEMPO 3:1:0" → Con tempo especificado
  
Con peso directo:
  "3x5 @225"   → 3 series de 5 a 225kg/lb
```

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-002-01 | Diseñar estructura de datos `SetNotation` | ✅ | 2h | Tipo flexible |
| PB-002-02 | Crear `parseSetNotation(input: string): SetNotation` | ✅ | 4h | Regex + parser |
| PB-002-03 | Crear `formatSetNotation(notation: SetNotation): string` | ✅ | 2h | Inverso del parser |
| PB-002-04 | Crear `SetNotationInput` (input con autocompletado) | ✅ | 4h | Sugerencias mientras escribe |
| PB-002-05 | Crear `SetNotationDisplay` (render bonito de la notación) | ✅ | 2h | Con badges de color |
| PB-002-06 | Implementar validación de notaciones | ✅ | 2h | Mensajes de error claros |
| PB-002-07 | Agregar tooltips explicativos al hover | ✅ | 1h | "1 single + 3x4 backoff" |
| PB-002-08 | Tests unitarios para parser (casos edge) | ✅ | 3h | Jest |
| PB-002-09 | Documentar notaciones soportadas (help modal) | ✅ | 1h | Para el usuario |
| PB-002-10 | Mostrar notación compuesta completa en Day/Weekly View | ✅ | 2h | "1x1 3x4" muestra todos los grupos |

**Tipo SetNotation propuesto**:
```typescript
interface SetGroup {
  sets: number;
  repsMin: number;
  repsMax?: number;        // Para rangos como 8-12
  percentage?: number;     // %RM
  rpe?: number;
  weight?: number;         // Peso directo
  tempo?: string;          // "3:1:0"
  isAMRAP?: boolean;
}

interface SetNotation {
  groups: SetGroup[];      // Permite "1x1 3x4" como 2 grupos
  emomMinutes?: number;    // Para EMOM
  notes?: string;
  raw: string;             // String original
}
```

**Criterios de Aceptación**:
- [x] Parsea correctamente todas las notaciones del Excel del coach
- [x] El input muestra sugerencias/autocompletado
- [x] Errores de parsing muestran mensaje amigable
- [x] Se puede escribir en formato libre y se parsea
- [x] El display es legible y con colores distintivos

**Dependencias**: Ninguna

---

### PB-003: Exercise Type Support (Soporte para Tipos de Ejercicio)
**Estado**: ✅ Completado | **Prioridad**: 🟡 Alta | **Estimación**: 2-3 días

Soporte para ejercicios especiales: EMOM, ejercicios con tempo, supersets, etc.

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-003-01 | Crear enum `ExerciseType` (Standard, EMOM, Tempo, Superset, Circuit) | ✅ | 0.5h | `src/types/builder.ts` |
| PB-003-02 | Agregar campo `exerciseType` al modelo | ✅ | 1h | BuilderExercise, DaySlotExercise |
| PB-003-03 | Crear UI para seleccionar tipo de ejercicio | ✅ | 2h | `exercise-type-selector.tsx` |
| PB-003-04 | Crear `EMOMConfig` (minutos, trabajo/descanso) | ✅ | 2h | Con inputs numéricos |
| PB-003-05 | Crear `TempoConfig` (excéntrico, pausa, concéntrico) | ✅ | 2h | Visual "3:1:0" |
| PB-003-06 | Crear `SupersetConfig` (agrupar ejercicios) | ✅ | 3h | Visual grouping implementado |
| PB-003-07 | Adaptar visualización según tipo | ✅ | 2h | ExerciseTypeBadge integrado |
| PB-003-08 | Tests para cada tipo de ejercicio | ✅ | 2h | Unit tests ExerciseType y SupersetConfig |

**Criterios de Aceptación**:
- [x] Se puede marcar un ejercicio como EMOM y configurar minutos
- [x] El tempo se muestra claramente (ej: "TEMPO 3:1:0")
- [x] Los supersets se agrupan visualmente (left-border color + grouping en DayCentricView)
- [x] La exportación respeta estos tipos (PDF badge + Excel name suffix)

**Dependencias**: PB-002 (notación)

---

## 🟠 FASE 2: Cálculos Automáticos
**Prioridad**: Alta | **Estimación**: 1-2 semanas | **Sprint**: 2-3

> Objetivo: Automatizar los cálculos que el coach hace manualmente en Excel.

---

### PB-004: Auto-Weight Calculator con 1RM
**Estado**: ✅ Completado | **Prioridad**: 🔴 Crítica | **Estimación**: 4-5 días

Cálculo automático de pesos basado en el 1RM del atleta por ejercicio.

```
┌─────────────────────────────────────────────────────────────────┐
│  Bench Press                     S1          S2          S3    │
│  ────────────────────────────────────────────────────────────── │
│  Prescription:                   3x8 @65%    3x6 @70%    3x5   │
│  ─────────────────────────────────────────────────               │
│  1RM: 315kg                      ↓           ↓           ↓      │
│  ────────────────────────────────────────────────────────────── │
│  Suggested Weight:               205kg       220kg       —      │
│  (rounded to 2.5kg)                                             │
└─────────────────────────────────────────────────────────────────┘
```

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-004-01 | Crear endpoint API `GET /athletes/{id}/max-lifts` | ✅ | 2h | Backend + AthleteMaxLift entity |
| PB-004-02 | Crear hook `useAthleteMaxLifts` | ✅ | 1h | Frontend hooks + API |
| PB-004-03 | Implementar función `calculateWorkingWeight` | ✅ | 1h | `weight-calculator.ts` |
| PB-004-04 | Crear configuración de redondeo por coach (2.5kg, 5kg, etc) | ✅ | 2h | Select en Builder toolbar |
| PB-004-05 | Mostrar peso sugerido debajo de cada celda con % | ✅ | 3h | WeightSuggestion component |
| PB-004-06 | Implementar edición manual del peso (override) | ✅ | 2h | Click para editar + ícono ✎ |
| PB-004-07 | Indicador visual cuando no hay 1RM disponible | ✅ | 1h | "No 1RM" en amber |
| PB-004-08 | Crear modal para registrar 1RM rápido desde el builder | ✅ | 3h | Click en "No 1RM" abre modal |
| PB-004-09 | Tests de cálculo de pesos | ✅ | 2h | 35 tests en weight-calculator.test.ts |
| PB-004-10 | Selector "Preview como atleta" en Builder | ✅ | 2h | Select en toolbar |

**Criterios de Aceptación**:
- [x] Los pesos se calculan automáticamente cuando hay % y 1RM
- [x] El redondeo es configurable por el coach (1kg, 2.5kg, 5kg)
- [x] Se puede hacer override manual del peso calculado
- [x] Muestra indicador claro cuando falta 1RM
- [x] Click en "No 1RM" abre modal para registrar 1RM rápido

**Dependencias**: PB-002 (para extraer % del notation)

---

### PB-005: Quick Fill Actions
**Estado**: ✅ Completado | **Prioridad**: 🟡 Alta | **Estimación**: 3-4 días

Acciones rápidas para llenar múltiples celdas de una vez.

**Acciones disponibles**:
```
┌─────────────────────────────────────────────────────────────────┐
│  Quick Actions (click derecho o botón ⚡)                       │
│  ─────────────────────────────────────────                      │
│  📋 Copy S1 to all weeks                                        │
│  📈 Apply +2% increment per week                                │
│  📉 Apply +5% for 3 weeks, then deload                          │
│  🔄 Repeat pattern (S1-S4) for remaining weeks                  │
│  🎯 Insert deload week at S4                                    │
│  📊 Apply progression template...                               │
│  👤 Copy from another athlete...                                │
└─────────────────────────────────────────────────────────────────┘
```

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-005-01 | Crear menu contextual (right-click) en celdas | ✅ | 2h | @radix-ui/context-menu |
| PB-005-02 | Implementar "Copy to all weeks" | ✅ | 1h | |
| PB-005-03 | Implementar "Apply increment per week" | ✅ | 2h | Modal con config |
| PB-005-04 | Implementar "Insert deload week" | ✅ | 2h | Reduce % y volumen |
| PB-005-05 | Implementar "Repeat pattern" | ✅ | 2h | Detectar patrón |
| PB-005-06 | Implementar "Copy from athlete" | ⏸️ | 3h | Selector de atleta (deferred) |
| PB-005-07 | Agregar preview antes de aplicar acción | ✅ | 2h | UX safety |
| PB-005-08 | Implementar Undo para acciones masivas | ✅ | 3h | Ctrl+Z |
| PB-005-09 | Agregar keyboard shortcuts (Ctrl+Z undo) | ✅ | 2h | |

**Criterios de Aceptación**:
- [x] El menú contextual aparece con click derecho
- [x] Cada acción muestra preview antes de aplicar
- [x] Se puede deshacer (Undo) cualquier acción masiva
- [x] Las acciones son rápidas y responsivas

**Dependencias**: PB-001 (Day-Centric View)

---

## 🟡 FASE 3: Exportación
**Prioridad**: Alta | **Estimación**: 1-2 semanas | **Sprint**: 3

> Objetivo: Generar entregables profesionales para el atleta.

---

### PB-006: Export to PDF
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Estimación**: 4-5 días

Generación de PDF profesional con el programa del atleta.

```
┌─────────────────────────────────────────────────────────────────┐
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  [LOGO]  Programa de Entrenamiento                        │  │
│  │          Atleta: Juan Pérez                               │  │
│  │          Coach: Carlos García                             │  │
│  │          Duración: 8 semanas                              │  │
│  │          Fecha inicio: 15 Marzo 2026                      │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ═══════════════════════════════════════════════════════════    │
│  DÍA 1 - PUSH                                                   │
│  ═══════════════════════════════════════════════════════════    │
│                                                                 │
│  ┌────────────────┬────────┬────────┬────────┬────────┐        │
│  │ Exercise       │ S1     │ S2     │ S3     │ S4     │        │
│  ├────────────────┼────────┼────────┼────────┼────────┤        │
│  │ Bench Press    │ 3x8    │ 3x6    │ 3x5    │ 3x3    │        │
│  │                │ 205kg  │ 220kg  │ 235kg  │ 250kg  │        │
│  ├────────────────┼────────┼────────┼────────┼────────┤        │
│  │ Incline DB     │ 3x10   │ 3x10   │ 3x8    │ 3x8    │        │
│  │                │ @8     │ @8     │ @9     │ @9     │        │
│  └────────────────┴────────┴────────┴────────┴────────┘        │
│                                                                 │
│  [Notas del día...]                                             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-006-01 | Evaluar librerías (react-pdf, jsPDF, puppeteer) | ✅ | 2h | Elegimos @react-pdf/renderer |
| PB-006-02 | Crear template PDF base | ✅ | 4h | program-pdf-document.tsx con Header, DaySection, Footer |
| PB-006-03 | Implementar renderizado de tablas de ejercicios | ✅ | 4h | ExerciseTable component con columnas por semana |
| PB-006-04 | Agregar logo del coach (configurable) | ✅ | 2h | Upload en Settings, base64 en localStorage, renderiza en PDF header |
| PB-006-05 | Implementar paginación automática | ✅ | 2h | Page breaks automáticos por @react-pdf |
| PB-006-06 | Crear opciones de personalización | ✅ | 2h | includeWeights, includeNotes, weightRoundTo |
| PB-006-07 | Preview del PDF antes de descargar | ✅ | 3h | ExportPDFDialog con iframe preview |
| PB-006-08 | Agregar notas del coach por semana/día | ✅ | 1h | Se renderizan si existen |
| PB-006-09 | Tests de generación PDF | ✅ | 2h | Pure utils extraídos en pdf-utils.ts, 18 tests en pdf-utils.test.ts |

**Archivos creados**:
- `src/components/pdf/program-pdf-styles.ts` - Estilos StyleSheet
- `src/components/pdf/program-pdf-document.tsx` - Componente principal del PDF
- `src/components/pdf/use-program-pdf.tsx` - Hook para generar/descargar
- `src/components/pdf/export-pdf-dialog.tsx` - Diálogo con preview y opciones
- `src/components/pdf/index.ts` - Barrel export

**Criterios de Aceptación**:
- [x] El PDF se genera en menos de 3 segundos
- [x] Incluye logo del coach (si está configurado) — Upload en Settings → logoUrl en Coach → PDF header
- [x] Las tablas son legibles y bien formateadas
- [x] Se puede elegir qué información incluir
- [x] Funciona para programas de cualquier duración

**Dependencias**: Ninguna

---

### PB-007: Export to Excel
**Estado**: ✅ Completada | **Prioridad**: 🟡 Alta | **Estimación**: 3-4 días

Exportación a Excel compatible con el formato actual del coach.

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-007-01 | Integrar librería xlsx (SheetJS) | ✅ | 1h | npm install xlsx |
| PB-007-02 | Crear estructura de workbook (1 sheet por día) | ✅ | 2h | Overview + Day sheets |
| PB-007-03 | Implementar formateo de celdas (negrita, colores) | ✅ | 2h | Column widths configurados |
| PB-007-04 | Agregar fórmulas de cálculo de peso | ⏸️ | 3h | Pesos calculados inline (no fórmulas) |
| PB-007-05 | Congelar primera columna y fila | ✅ | 1h | !freeze configurado |
| PB-007-06 | Agregar sheet de resumen/overview | ✅ | 2h | Con info de programa y 1RMs |
| PB-007-07 | Preview y descarga | ✅ | 1h | ExportExcelDialog con opciones |
| PB-007-08 | Tests de estructura del Excel | ✅ | 2h | Pure utils extraídos en excel-utils.ts, 38 tests en excel-utils.test.ts |

**Archivos creados**:
- `src/components/excel/program-excel-generator.ts` - Lógica de generación
- `src/components/excel/export-excel-dialog.tsx` - Diálogo con opciones
- `src/components/excel/index.ts` - Barrel export

**Criterios de Aceptación**:
- [x] El Excel tiene la misma estructura que el del coach (días como hojas)
- [x] Los formatos se mantienen (ancho de columnas)
- [x] Se puede abrir en Excel, Google Sheets, etc.
- [ ] Las fórmulas funcionan (decidido: pesos inline mejor UX)

**Dependencias**: Ninguna

---

## 🟢 FASE 4: Diferenciadores
**Prioridad**: Media | **Estimación**: 2 semanas | **Sprint**: 4

> Objetivo: Features que hacen la app superior al Excel.

---

### PB-008: Progression Visualizer (Gráficos)
**Estado**: ✅ Completado | **Prioridad**: 🟡 Alta | **Estimación**: 4-5 días

Visualización gráfica de la progresión del ejercicio a través de las semanas.

```
┌─────────────────────────────────────────────────────────────────┐
│  📊 Bench Press - Progression                                   │
│  ─────────────────────────────────────────────────────────────  │
│                                                                 │
│  100% ┤                                           ╭─╮           │
│   95% ┤                                      ╭────╯ │           │
│   90% ┤                                 ╭────╯      │           │
│   85% ┤                            ╭────╯          │           │
│   80% ┤                       ╭────╯               │  Deload   │
│   75% ┤                  ╭────╯                    ╰───╮       │
│   70% ┤             ╭────╯                              │       │
│   65% ┤        ╭────╯                                   │       │
│   60% ┼────────╯                                        │       │
│       └────┬────┬────┬────┬────┬────┬────┬────┬────┬────┘       │
│            S1   S2   S3   S4   S5   S6   S7   S8   S9           │
│                                                                 │
│  Legend: ── %RM    ─· RPE    ▓ Volume                          │
└─────────────────────────────────────────────────────────────────┘
```

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-008-01 | Integrar librería de gráficos (Recharts) | ✅ | 1h | npm install recharts |
| PB-008-02 | Crear componente `ProgressionChart` | ✅ | 4h | Line chart con ComposedChart |
| PB-008-03 | Mostrar múltiples métricas (%RM, RPE, Volume) | ✅ | 2h | Toggle switches |
| PB-008-04 | Agregar tooltips interactivos | ✅ | 2h | CustomTooltip component |
| PB-008-05 | Detectar y marcar bloques (Hypertrophy, Strength, Peak) | ✅ | 3h | detectBlocks() con legend |
| PB-008-06 | Comparar múltiples ejercicios en mismo gráfico | ✅ | 2h | MultiExerciseChart + compare mode en ExerciseList |
| PB-008-07 | Integrar en Exercise Progression View | ✅ | 2h | Toggle "Ver gráfico" |
| PB-008-08 | Tests de renderizado | ✅ | 1h | progression-utils.test.ts (35 tests) |

**Archivos creados**:
- `src/components/charts/progression-chart.tsx` - Componente principal con Recharts
- `src/components/charts/index.ts` - Barrel export

**Criterios de Aceptación**:
- [x] El gráfico muestra la progresión de forma clara
- [x] Se pueden ver diferentes métricas (%RM, RPE, Volume)
- [x] Los bloques de entrenamiento se detectan visualmente
- [x] Es interactivo (hover muestra detalles)

**Dependencias**: Ninguna

---

### PB-009: Exercise Library con Historial
**Estado**: ✅ Completado | **Prioridad**: 🟡 Alta | **Estimación**: 3-4 días

Al seleccionar un ejercicio, mostrar contexto del atleta.

```
┌─────────────────────────────────────────────────────────────────┐
│  🔍 Search exercise...                                          │
│  ─────────────────────────────────────────────────────────────  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ 🏋️ Bench Press                                            │  │
│  │    ─────────────────────────────────────────────────────  │  │
│  │    📊 Athlete Stats:                                      │  │
│  │       • Current 1RM: 315kg (updated 2 weeks ago)         │  │
│  │       • Last programmed: 3x5 @275kg (S6 - last program)  │  │
│  │       • PR: 320kg (Competition - Jan 2026)               │  │
│  │       • Trend: ↗️ +5kg over last 3 months                 │  │
│  │                                                           │  │
│  │    💡 Suggestion: Start at 65% (~205kg) based on history │  │
│  │                                                           │  │
│  │    [Select]                                               │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ 🏋️ Squat (Low Bar)                                        │  │
│  │    📊 No history for this athlete                         │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-009-01 | Crear endpoint `GET /athletes/{id}/exercise-history/{exerciseId}` | ✅ | 3h | Backend con GetExerciseHistoryQuery |
| PB-009-02 | Crear hook `useExerciseHistory(athleteId, exerciseId)` | ✅ | 1h | React Query con staleTime 5min |
| PB-009-03 | Rediseñar exercise picker con panel de historial | ✅ | 3h | ExercisePickerWithHistory component |
| PB-009-04 | Mostrar 1RM, último peso, PR, tendencia | ✅ | 2h | Panel lateral con badges |
| PB-009-05 | Agregar sugerencia automática de % inicial | ✅ | 2h | 65% del 1RM calculado en backend |
| PB-009-06 | Indicador visual de ejercicios sin historial | ✅ | 1h | Badge/icono por ejercicio |
| PB-009-07 | Tests | ✅ | 1h | Cubierto en progression-utils (getTrendDirection, calculateSuggestedWeight) |

**Archivos creados**:
- `src/CoachPlatform.Application/Features/Athletes/Queries/GetExerciseHistory/` - Query y Handler backend
- `src/components/builder/exercise-picker-with-history.tsx` - Componente picker con historial
- Actualizado `use-athletes.ts` con hook `useExerciseHistory`
- Actualizados `types/athlete.ts` con ExerciseHistory, MaxLiftSummary, etc.

**Criterios de Aceptación**:
- [x] Al buscar ejercicio se muestra el historial del atleta
- [x] La sugerencia de % inicial es útil y precisa (65% del 1RM)
- [x] Es rápido (no bloquea el UX) - React Query con cache
- [x] Integrado en DayCentricView y ExerciseTable

**Dependencias**: API de historial (completada)

---

### PB-010: Calendar Integration
**Estado**: ✅ Completada | **Prioridad**: 🟢 Media | **Estimación**: 3-4 días

Vista del programa en formato calendario con drag-to-reschedule.

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-010-01 | Crear componente `CalendarView` | ✅ | 4h | calendar-view.tsx con date-fns |
| PB-010-02 | Implementar asignación de fechas a semanas | ✅ | 2h | mapProgramToCalendar() + selector de fecha |
| PB-010-03 | Mostrar días de entrenamiento con resumen | ✅ | 2h | Popover con detalles, ejercicios, focus |
| PB-010-04 | Integrar con días de descanso y competencias | ✅ | 2h | Soporte para deload y competencias |
| PB-010-05 | Drag to reschedule | ✅ | 3h | @dnd-kit useDraggable/useDroppable, cross-week moves, undo |
| PB-010-06 | Tests | ✅ | 1h | 32 tests (getCalendarDays, mapProgramToCalendar, getTrainingDayOffset) |

**Archivos creados**:
- `src/components/builder/calendar-view.tsx` - Componente principal del calendario con DnD
- `src/components/builder/calendar-utils.ts` - Funciones puras extraídas (getCalendarDays, mapProgramToCalendar, getTrainingDayOffset)
- `src/__tests__/components/calendar-view.test.ts` - 32 tests unitarios
- Actualizado `view-mode-selector.tsx` con opción de calendario
- Actualizado `types/builder.ts` con 'calendar' en BuilderViewMode
- Actualizado `builder/page.tsx` con handleCalendarDayMove (same-week reorder + cross-week move)

**Criterios de Aceptación**:
- [x] Vista calendario mensual con navegación por mes
- [x] Días de entrenamiento coloreados por tipo (training, deload, competition)
- [x] Popover con detalles al click (ejercicios, focus, notas)
- [x] Drag & drop para mover training days entre fechas
- [x] Soporte cross-week moves con undo
- [x] Leyenda visual con tipos de día y hint de drag
- [x] 32 tests unitarios pasando

**Dependencias**: Ninguna

---

## 🔵 FASE 5: Innovación
**Prioridad**: Baja | **Estimación**: 2+ semanas | **Sprint**: 5+

> Objetivo: Features de siguiente nivel que ningún competidor tiene.

---

### PB-011: AI Progression Suggestions
**Estado**: ⬜ Pendiente | **Prioridad**: 🟢 Media | **Estimación**: 5+ días

Sugerencias inteligentes basadas en historial y ciencia del entrenamiento.

| ID | Task | Estado | Estimación | Notas |
|----|------|--------|------------|-------|
| PB-011-01 | Definir reglas de progresión basadas en literatura | ⬜ | 4h | Research |
| PB-011-02 | Crear motor de sugerencias (rule-based inicialmente) | ⬜ | 8h | |
| PB-011-03 | Analizar patrones de fatiga del atleta | ⬜ | 4h | |
| PB-011-04 | Generar alertas proactivas | ⬜ | 4h | "Consider deload" |
| PB-011-05 | Integrar con historial de rendimiento | ⬜ | 4h | |
| PB-011-06 | UI para sugerencias (no intrusiva) | ⬜ | 3h | |

**Dependencias**: PB-009 (historial)

---

### PB-012: Template Marketplace (Futuro)
**Estado**: ⬜ Pendiente | **Prioridad**: 🟢 Baja | **Estimación**: 1+ semana

Guardar y compartir templates de programas.

*Detalle a definir cuando se priorice.*

---

### PB-013: Athlete Preview Mode (Futuro)
**Estado**: ⬜ Pendiente | **Prioridad**: 🟢 Baja | **Estimación**: 3-4 días

Ver exactamente lo que verá el atleta en su app móvil.

*Detalle a definir cuando se priorice.*

---

## 📈 Métricas de Éxito

| Métrica | Actual | Objetivo |
|---------|--------|----------|
| Tiempo para crear programa 8 semanas | ~2 horas (Excel) | <30 minutos |
| Errores de cálculo de peso | Ocasionales | 0 |
| Tiempo para modificar ejercicio | ~5 min (todas las semanas) | <30 segundos |
| Satisfacción del coach con UX | N/A | >4.5/5 |

---

## 🔗 Dependencias Entre Features

```
                    ┌─────────────┐
                    │   PB-001    │ Day-Centric View
                    │  (Base UI)  │
                    └──────┬──────┘
                           │
              ┌────────────┼────────────┐
              │            │            │
              ▼            ▼            ▼
        ┌─────────┐  ┌─────────┐  ┌─────────┐
        │ PB-002  │  │ PB-005  │  │ PB-006  │
        │Notation │  │QuickFill│  │  PDF    │
        └────┬────┘  └─────────┘  └─────────┘
             │
             ▼
        ┌─────────┐
        │ PB-004  │ Auto-Weight
        │  1RM    │
        └────┬────┘
             │
             ▼
        ┌─────────┐
        │ PB-009  │ Exercise History
        │ Library │
        └────┬────┘
             │
             ▼
        ┌─────────┐
        │ PB-011  │ AI Suggestions
        └─────────┘
```

---

## 🗓️ Timeline Sugerido

```
Semana 1-2:   FASE 1 - Day-Centric View + Smart Notation
Semana 3:     FASE 1 - Exercise Types + FASE 2 inicio
Semana 4:     FASE 2 - Auto-Weight + Quick Fill
Semana 5-6:   FASE 3 - Export PDF + Excel
Semana 7-8:   FASE 4 - Visualizer + History
Semana 9+:    FASE 5 - AI + Templates
```

---

## 📝 Notas de Implementación

### Stack Recomendado
- **Gráficos**: Recharts (ya compatible con React/Next.js)
- **PDF**: @react-pdf/renderer (client-side) o Puppeteer (server-side para calidad)
- **Excel**: SheetJS/xlsx (community edition)
- **DnD**: @dnd-kit (ya implementado)

### Consideraciones Técnicas
1. **Performance**: La Day-Centric View puede tener muchas celdas. Considerar virtualización.
2. **State Management**: Mantener sincronización entre las 3 vistas será crítico.
3. **Backend**: Algunas features requieren nuevos endpoints (1RM, historial).
4. **Testing**: Cada feature debe tener tests antes de siguiente fase.

---

## ✅ Checklist Pre-Implementación

Antes de empezar cada fase:

- [ ] Revisar dependencias de la fase
- [ ] Confirmar que el backend soporta los datos necesarios
- [ ] Crear branch específico para la fase
- [ ] Definir test cases de aceptación
- [ ] Review del diseño con stakeholder (coach)

---

*Documento vivo - Actualizar conforme se avanza en implementación.*
