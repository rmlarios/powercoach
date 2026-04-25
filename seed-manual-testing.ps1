# PowerCoach - Seed data for manual testing
# This script populates the database with realistic data through the API
# Safe to re-run: skips entities that already exist (409 Conflict)

$ErrorActionPreference = "Continue"

$base = "http://localhost:5000/api"
$coachId = "00000000-0000-0000-0000-000000000001"
$headers = @{ "X-Coach-Id" = $coachId }
$today = Get-Date -Format "yyyy-MM-dd"

function Post-Api($url, $body = $null) {
    try {
        if ($null -eq $body) {
            Invoke-RestMethod -Uri $url -Method POST -Headers $headers -ErrorAction Stop
        } else {
            Invoke-RestMethod -Uri $url -Method POST -Headers $headers -Body ($body | ConvertTo-Json -Depth 10 -Compress) -ContentType "application/json" -ErrorAction Stop
        }
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -eq 409) {
            Write-Host "    (already exists, skipping)" -ForegroundColor DarkGray
            return $null
        }
        Write-Host "    ERROR ($status): $_" -ForegroundColor Red
        return $null
    }
}

function Put-Api($url, $body) {
    try {
        Invoke-WebRequest -Uri $url -Method PUT -Headers $headers -Body ($body | ConvertTo-Json -Depth 10 -Compress) -ContentType "application/json" -UseBasicParsing -ErrorAction Stop | Out-Null
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        Write-Host "    PUT warning ($status): $_" -ForegroundColor Yellow
    }
}

function Get-Api($url) {
    Invoke-RestMethod -Uri $url -Method GET -Headers $headers
}

function Get-Id($result) {
    if ($null -eq $result) { return $null }
    if ($result -is [string]) { return $result }
    if ($result.PSObject.Properties['id']) { return [string]$result.id }
    if ($result.PSObject.Properties['Id']) { return [string]$result.Id }
    return [string]$result
}

Write-Host "`n=== SEEDING EXERCISES ===" -ForegroundColor Cyan

$exercises = @(
    @{ name="Sentadilla Barra Baja"; category="Squat"; primaryMuscleGroup="Quadriceps"; description="Sentadilla de powerlifting con barra baja en espalda"; equipment="Barra, Rack de sentadilla"; isCompound=$true },
    @{ name="Sentadilla Barra Alta"; category="Squat"; primaryMuscleGroup="Quadriceps"; description="Sentadilla olimpica con barra alta"; equipment="Barra, Rack de sentadilla"; isCompound=$true },
    @{ name="Sentadilla Frontal"; category="Squat"; primaryMuscleGroup="Quadriceps"; description="Front squat con barra en posicion frontal"; equipment="Barra, Rack de sentadilla"; isCompound=$true },
    @{ name="Press Banca Competicion"; category="Bench"; primaryMuscleGroup="Chest"; description="Press de banca con pausa en pecho para competencia"; equipment="Barra, Banco plano"; isCompound=$true },
    @{ name="Press Banca Inclinado"; category="Bench"; primaryMuscleGroup="Chest"; description="Press de banca en banco inclinado 30 grados"; equipment="Barra, Banco inclinado"; isCompound=$true },
    @{ name="Press Banca Cerrado"; category="Bench"; primaryMuscleGroup="Triceps"; description="Press de banca con agarre estrecho para triceps"; equipment="Barra, Banco plano"; isCompound=$true },
    @{ name="Peso Muerto Convencional"; category="Deadlift"; primaryMuscleGroup="Back"; description="Peso muerto con postura convencional"; equipment="Barra, Plataforma"; isCompound=$true },
    @{ name="Peso Muerto Sumo"; category="Deadlift"; primaryMuscleGroup="Back"; description="Peso muerto con postura sumo abierta"; equipment="Barra, Plataforma"; isCompound=$true },
    @{ name="Peso Muerto Rumano"; category="Deadlift"; primaryMuscleGroup="Hamstrings"; description="RDL para cadena posterior"; equipment="Barra"; isCompound=$true },
    @{ name="Press Militar"; category="Accessory"; primaryMuscleGroup="Shoulders"; description="Press de hombro de pie con barra"; equipment="Barra"; isCompound=$true },
    @{ name="Remo con Barra"; category="Accessory"; primaryMuscleGroup="Back"; description="Remo Pendlay para espalda"; equipment="Barra"; isCompound=$true },
    @{ name="Dominadas"; category="Accessory"; primaryMuscleGroup="Back"; description="Pull-ups con peso corporal o lastre"; equipment="Barra de dominadas"; isCompound=$true },
    @{ name="Curl de Biceps"; category="Accessory"; primaryMuscleGroup="Biceps"; description="Curl con mancuernas"; equipment="Mancuernas"; isCompound=$false },
    @{ name="Extension de Triceps"; category="Accessory"; primaryMuscleGroup="Triceps"; description="Extension de triceps en polea"; equipment="Polea"; isCompound=$false },
    @{ name="Prensa de Piernas"; category="Accessory"; primaryMuscleGroup="Quadriceps"; description="Leg press a 45 grados"; equipment="Prensa"; isCompound=$true },
    @{ name="Hip Thrust"; category="Accessory"; primaryMuscleGroup="Glutes"; description="Hip thrust con barra para gluteo"; equipment="Barra, Banco"; isCompound=$true }
)

$exerciseIds = @{}
foreach ($ex in $exercises) {
    $result = Post-Api "$base/coaches/$coachId/exercises" $ex
    $id = Get-Id $result
    if ($id) {
        $exerciseIds[$ex.name] = $id
        Write-Host "  + $($ex.name) -> $id" -ForegroundColor Green
    }
}

# Recover exercise IDs if some already existed
if ($exerciseIds.Count -lt $exercises.Count) {
    Write-Host "  Recovering existing exercise IDs..." -ForegroundColor DarkGray
    $existing = Get-Api "$base/coaches/$coachId/exercises"
    $exList = if ($existing.PSObject.Properties['items']) { $existing.items } else { $existing }
    foreach ($e in $exList) {
        $exerciseIds[$e.name] = [string]$e.id
    }
    Write-Host "  Recovered $($exerciseIds.Count) exercise IDs" -ForegroundColor DarkGray
}

Write-Host "`n=== SEEDING APPLICATIONS ===" -ForegroundColor Cyan

$apps = @(
    @{ coachId=$coachId; firstName="Juan Carlos"; lastName="Garcia Lopez"; email="juancarlos@gmail.com"; phone="+34611222333"; country="Espana"; age=31; gender="Male"; trainingExperience="3 anos entrenando fuerza por mi cuenta. Familiarizado con los basicos."; currentSquat=140; currentBench=100; currentDeadlift=170; motivation="Competir en mi primera competencia de powerlifting en 6 meses"; goals="Clasificar a nivel regional"; message="Recomendado por un amigo que ya entrena contigo"; referralSource="Amigo" },
    @{ coachId=$coachId; firstName="Maria"; lastName="Fernandez Ruiz"; email="maria.fernandez@outlook.com"; phone="+34622333444"; country="Espana"; age=34; gender="Female"; trainingExperience="5 anos, compito en IPF hace 2 anos. Total actual 342.5kg en 63kg."; currentSquat=120; currentBench=72.5; currentDeadlift=150; motivation="Clasificar al campeonato nacional"; goals="Mejorar peso muerto y total"; message="Busco un coach con experiencia en peaking para competencia"; referralSource="Instagram" },
    @{ coachId=$coachId; firstName="Pedro"; lastName="Martinez Sanz"; email="pedro.martinez@gmail.com"; phone="+34633444555"; country="Mexico"; age=28; gender="Male"; trainingExperience="6 meses en CrossFit. Nuevo en entrenamiento de fuerza puro."; currentSquat=80; currentBench=60; currentDeadlift=100; motivation="Aprender tecnica correcta y ganar fuerza base"; goals="Perder grasa y ganar fuerza"; message="Hernia discal L4-L5 hace 3 anos, ya recuperado. Quiero entrenar con supervision."; referralSource="Google" },
    @{ coachId=$coachId; firstName="Ana"; lastName="Rodriguez Torres"; email="ana.rodriguez@hotmail.com"; phone="+34644555666"; country="Argentina"; age=29; gender="Female"; trainingExperience="2 anos entrenando sola con programas de internet"; currentSquat=90; currentBench=50; currentDeadlift=110; motivation="Subir total en los 3 basicos"; goals="Competir eventualmente"; message="Tendinitis rotuliana leve en rodilla izquierda"; referralSource="YouTube" },
    @{ coachId=$coachId; firstName="Carlos"; lastName="Hernandez Diaz"; email="carlos.hdiaz@gmail.com"; phone="+34655666777"; country="Chile"; age=25; gender="Male"; trainingExperience="1 ano en CrossFit, quiere especializarse en fuerza"; currentSquat=100; currentBench=75; currentDeadlift=130; motivation="Especializarme en powerlifting"; goals="Aprender tecnica correcta de los basicos"; message="Vengo de CrossFit y quiero un enfoque mas estructurado"; referralSource="Amigo" }
)

$appIds = @()
foreach ($app in $apps) {
    $result = Post-Api "$base/applications" $app
    $id = Get-Id $result
    if ($id) {
        $appIds += $id
        Write-Host "  + Aplicacion $($app.firstName) $($app.lastName) -> $id" -ForegroundColor Green
    }
}

# Recover application IDs if some already existed
if ($appIds.Count -lt $apps.Count) {
    Write-Host "  Recovering existing application IDs..." -ForegroundColor DarkGray
    $existingApps = Get-Api "$base/applications?coachId=$coachId&pageSize=50"
    $appList = if ($existingApps.PSObject.Properties['items']) { $existingApps.items } else { $existingApps }
    $appIds = @()
    foreach ($a in $apps) {
        $match = $appList | Where-Object { $_.email -eq $a.email -or $_.fullName -eq "$($a.firstName) $($a.lastName)" } | Select-Object -First 1
        if ($match) { $appIds += [string]$match.id }
    }
    Write-Host "  Recovered $($appIds.Count) application IDs" -ForegroundColor DarkGray
}

# Approve first 3, reject 4th, leave 5th pending
Write-Host "`n=== PROCESSING APPLICATIONS ===" -ForegroundColor Cyan

$athleteIds = @()
for ($i = 0; $i -lt 3; $i++) {
    $result = Post-Api "$base/applications/$($appIds[$i])/approve?coachId=$coachId"
    $athId = if ($result -and $result.athleteId) { [string]$result.athleteId } elseif ($result) { Get-Id $result } else { $null }
    if ($athId) {
        $athleteIds += $athId
        Write-Host "  + Aprobada: $($apps[$i].firstName) -> Atleta $athId" -ForegroundColor Green
    }
}

if ($appIds.Count -ge 4) {
    Post-Api "$base/applications/$($appIds[3])/reject?coachId=$coachId" @{ reason = "No tenemos cupo disponible en este momento. Te contactaremos cuando haya plazas." } | Out-Null
    Write-Host "  - Rechazada: $($apps[3].firstName)" -ForegroundColor Yellow
}
if ($appIds.Count -ge 5) {
    Write-Host "  ~ Pendiente: $($apps[4].firstName)" -ForegroundColor DarkYellow
}

# Recover athlete IDs if approvals were already done
if ($athleteIds.Count -lt 3) {
    Write-Host "  Recovering existing athlete IDs..." -ForegroundColor DarkGray
    $existingAthletes = Get-Api "$base/athletes?coachId=$coachId&pageSize=50"
    $athList = if ($existingAthletes.PSObject.Properties['items']) { $existingAthletes.items } else { $existingAthletes }
    $athleteIds = @()
    foreach ($a in $athList) { $athleteIds += [string]$a.id }
    Write-Host "  Recovered $($athleteIds.Count) athlete IDs" -ForegroundColor DarkGray
}

if ($athleteIds.Count -lt 3) {
    Write-Host "  WARNING: Need at least 3 athletes. Only found $($athleteIds.Count). Some steps may be skipped." -ForegroundColor Yellow
}

# Update athletes with more info
Write-Host "`n=== UPDATING ATHLETE PROFILES ===" -ForegroundColor Cyan

$athleteUpdates = @(
    @{ country="Espana"; height=178; weight=83; experienceLevel="Intermediate" },
    @{ country="Espana"; height=165; weight=63; experienceLevel="Advanced" },
    @{ country="Mexico"; height=175; weight=95; experienceLevel="Beginner" }
)

for ($i = 0; $i -lt 3; $i++) {
    Put-Api "$base/athletes/$($athleteIds[$i])" $athleteUpdates[$i]
    Write-Host "  + Actualizado atleta $($athleteIds[$i]): $($athleteUpdates[$i].country), $($athleteUpdates[$i].experienceLevel)" -ForegroundColor Green
}

# Create plans
Write-Host "`n=== SEEDING PLANS ===" -ForegroundColor Cyan

$plans = @(
    @{ coachId=$coachId; name="Plan Basico"; description="Plan de entrenamiento basico con programacion mensual y seguimiento semanal"; price=80; currency="EUR"; durationInMonths=1 },
    @{ coachId=$coachId; name="Plan Premium"; description="Plan completo con programacion personalizada, video analisis y soporte diario"; price=150; currency="EUR"; durationInMonths=1 },
    @{ coachId=$coachId; name="Plan Competicion"; description="Plan especializado para preparacion de competencias de powerlifting"; price=200; currency="EUR"; durationInMonths=3 }
)

$planIds = @()
foreach ($plan in $plans) {
    $result = Post-Api "$base/plans" $plan
    $id = Get-Id $result
    if ($id) {
        $planIds += $id
        Write-Host "  + $($plan.name) ($($plan.price) $($plan.currency)) -> $id" -ForegroundColor Green
    }
}

# Recover plan IDs if some already existed
if ($planIds.Count -lt $plans.Count) {
    Write-Host "  Recovering existing plan IDs..." -ForegroundColor DarkGray
    $existingPlans = Get-Api "$base/plans?coachId=$coachId"
    $pList = if ($existingPlans -is [array]) { $existingPlans } else { @($existingPlans) }
    $planIds = @()
    foreach ($p in $pList) { $planIds += [string]$p.id }
    Write-Host "  Recovered $($planIds.Count) plan IDs" -ForegroundColor DarkGray
}

# Create subscriptions
Write-Host "`n=== SEEDING SUBSCRIPTIONS ===" -ForegroundColor Cyan

$subIds = @()
if ($athleteIds.Count -ge 3 -and $planIds.Count -ge 3) {
    $subData = @(
        @{ athleteId=$athleteIds[0]; planId=$planIds[1]; startDate=$today },
        @{ athleteId=$athleteIds[1]; planId=$planIds[2]; startDate=$today },
        @{ athleteId=$athleteIds[2]; planId=$planIds[0]; startDate=$today }
    )

    foreach ($sub in $subData) {
        $result = Post-Api "$base/subscriptions" $sub
        $id = Get-Id $result
        if ($id) {
            $subIds += $id
            Write-Host "  + Suscripcion atleta $($sub.athleteId) -> $id" -ForegroundColor Green
        } else {
            $subIds += "unknown"
        }
    }
} else {
    Write-Host "  Skipped (need 3 athletes and 3 plans)" -ForegroundColor Yellow
}

# Create program templates
Write-Host "`n=== SEEDING PROGRAM TEMPLATES ===" -ForegroundColor Cyan

# Program 1: Fuerza 5x5
$prog1 = Post-Api "$base/programs" @{
    coachId = $coachId
    name = "Fuerza 5x5 Lineal"
    description = "Programa de fuerza lineal clasico con progresion semanal en sentadilla, press banca y peso muerto"
    durationWeeks = 4
}
$prog1Id = Get-Id $prog1
Write-Host "  + Fuerza 5x5 -> $prog1Id" -ForegroundColor Green

# Add weeks and days for program 1
$w1 = Get-Id (Post-Api "$base/programs/$prog1Id/weeks?coachId=$coachId" @{ weekNumber=1; notes="Semana de acumulacion 1" })
$d1 = Get-Id (Post-Api "$base/programs/$prog1Id/weeks/$w1/days?coachId=$coachId" @{ dayNumber=1; name="Dia A - Sentadilla"; focus="Push"; notes="Foco en sentadilla y empuje" })
$d2 = Get-Id (Post-Api "$base/programs/$prog1Id/weeks/$w1/days?coachId=$coachId" @{ dayNumber=2; name="Dia B - Press Banca"; focus="Pull"; notes="Foco en press banca y tiron" })
$d3 = Get-Id (Post-Api "$base/programs/$prog1Id/weeks/$w1/days?coachId=$coachId" @{ dayNumber=3; name="Dia C - Peso Muerto"; focus="Legs"; notes="Foco en peso muerto" })

# Day A exercises
$null = Post-Api "$base/programs/$prog1Id/weeks/$w1/days/$d1/exercises?coachId=$coachId" @{ exerciseId=$exerciseIds["Sentadilla Barra Baja"]; sets=5; reps="5"; targetRpe=7.5; restSeconds=180; notes="Progresion: +2.5kg/semana"; order=1 }
$null = Post-Api "$base/programs/$prog1Id/weeks/$w1/days/$d1/exercises?coachId=$coachId" @{ exerciseId=$exerciseIds["Press Militar"]; sets=3; reps="8"; targetRpe=7; restSeconds=120; notes="Accesorio de hombro"; order=2 }
$null = Post-Api "$base/programs/$prog1Id/weeks/$w1/days/$d1/exercises?coachId=$coachId" @{ exerciseId=$exerciseIds["Prensa de Piernas"]; sets=3; reps="10"; targetRpe=7; restSeconds=90; notes="Accesorio de cuadriceps"; order=3 }

# Day B exercises  
$null = Post-Api "$base/programs/$prog1Id/weeks/$w1/days/$d2/exercises?coachId=$coachId" @{ exerciseId=$exerciseIds["Press Banca Competicion"]; sets=5; reps="5"; targetRpe=7.5; restSeconds=180; notes="Progresion: +2.5kg/semana"; order=1 }
$null = Post-Api "$base/programs/$prog1Id/weeks/$w1/days/$d2/exercises?coachId=$coachId" @{ exerciseId=$exerciseIds["Remo con Barra"]; sets=4; reps="8"; targetRpe=7; restSeconds=120; notes="Balance de empuje/tiron"; order=2 }
$null = Post-Api "$base/programs/$prog1Id/weeks/$w1/days/$d2/exercises?coachId=$coachId" @{ exerciseId=$exerciseIds["Curl de Biceps"]; sets=3; reps="12"; targetRpe=6; restSeconds=60; notes="Accesorio"; order=3 }

# Day C exercises
$null = Post-Api "$base/programs/$prog1Id/weeks/$w1/days/$d3/exercises?coachId=$coachId" @{ exerciseId=$exerciseIds["Peso Muerto Convencional"]; sets=5; reps="5"; targetRpe=7.5; restSeconds=240; notes="Progresion: +2.5kg/semana"; order=1 }
$null = Post-Api "$base/programs/$prog1Id/weeks/$w1/days/$d3/exercises?coachId=$coachId" @{ exerciseId=$exerciseIds["Sentadilla Frontal"]; sets=3; reps="6"; targetRpe=7; restSeconds=150; notes="Variante para cuadriceps"; order=2 }
$null = Post-Api "$base/programs/$prog1Id/weeks/$w1/days/$d3/exercises?coachId=$coachId" @{ exerciseId=$exerciseIds["Hip Thrust"]; sets=3; reps="10"; targetRpe=7; restSeconds=90; notes="Accesorio de gluteo"; order=3 }

# Program 2: Peaking para competencia (empty, for builder testing)
$prog2 = Post-Api "$base/programs" @{
    coachId = $coachId
    name = "Peaking Competencia IPF"
    description = "Programa de 8 semanas de peaking para competencia de powerlifting IPF. Incluye reduccion de volumen y aumento de intensidad progresiva."
    durationWeeks = 8
}
$prog2Id = Get-Id $prog2
Write-Host "  + Peaking Competencia -> $prog2Id" -ForegroundColor Green

# Program 3: Principiante
$prog3 = Post-Api "$base/programs" @{
    coachId = $coachId
    name = "Introduccion Fuerza"
    description = "Programa para principiantes enfocado en aprender la tecnica de los movimientos basicos con cargas ligeras"
    durationWeeks = 6
}
$prog3Id = Get-Id $prog3
Write-Host "  + Introduccion Fuerza -> $prog3Id" -ForegroundColor Green

# Assign program 1 to Juan Carlos
Write-Host "`n=== ASSIGNING PROGRAMS ===" -ForegroundColor Cyan
$athProg1 = Post-Api "$base/athletes/$($athleteIds[0])/assign-program?coachId=$coachId" @{
    programTemplateId = $prog1Id
    startDate = $today
    notes = "Inicio de ciclo de fuerza 5x5"
}
Write-Host "  + Fuerza 5x5 -> Juan Carlos" -ForegroundColor Green

# Register check-ins
Write-Host "`n=== SEEDING CHECK-INS ===" -ForegroundColor Cyan

if ($athleteIds.Count -ge 3) {
    $checkIns = @(
        @{ athleteId=$athleteIds[0]; weight=83.2; fatigueLevel=5; sleepQuality=8; motivationLevel=9; notes="Primer dia, me siento con energia" },
        @{ athleteId=$athleteIds[0]; weight=83.0; fatigueLevel=6; sleepQuality=7; motivationLevel=8; notes="Dormi un poco menos, pero con ganas de entrenar" },
        @{ athleteId=$athleteIds[1]; weight=63.1; fatigueLevel=4; sleepQuality=9; motivationLevel=10; notes="Excelente recuperacion, lista para entrenar fuerte" },
        @{ athleteId=$athleteIds[2]; weight=94.8; fatigueLevel=3; sleepQuality=8; motivationLevel=7; notes="Empezando a acostumbrarme a la rutina" }
    )

    foreach ($ci in $checkIns) {
        $null = Post-Api "$base/check-ins" $ci
        Write-Host "  + Check-in para atleta $($ci.athleteId)" -ForegroundColor Green
    }
} else {
    Write-Host "  Skipped (need 3 athletes)" -ForegroundColor Yellow
}

# Register max lifts
Write-Host "`n=== SEEDING MAX LIFTS ===" -ForegroundColor Cyan

if ($athleteIds.Count -ge 3) {
    $maxLifts = @(
        @{ athleteId=$athleteIds[0]; exerciseId=$exerciseIds["Sentadilla Barra Baja"]; weight=160; isTested=$true; notes="PR en entrenamiento" },
        @{ athleteId=$athleteIds[0]; exerciseId=$exerciseIds["Press Banca Competicion"]; weight=110; isTested=$true; notes="Estimado por triple a RPE 8" },
        @{ athleteId=$athleteIds[0]; exerciseId=$exerciseIds["Peso Muerto Convencional"]; weight=190; isTested=$true; notes="PR en competencia local" },
        @{ athleteId=$athleteIds[1]; exerciseId=$exerciseIds["Sentadilla Barra Baja"]; weight=120; isTested=$true; notes="PR en campeonato regional" },
        @{ athleteId=$athleteIds[1]; exerciseId=$exerciseIds["Press Banca Competicion"]; weight=72.5; isTested=$true; notes="PR en competencia" },
        @{ athleteId=$athleteIds[1]; exerciseId=$exerciseIds["Peso Muerto Sumo"]; weight=150; isTested=$true; notes="PR en entrenamiento" },
        @{ athleteId=$athleteIds[2]; exerciseId=$exerciseIds["Sentadilla Barra Baja"]; weight=80; isTested=$false; notes="Estimado, principiante" },
        @{ athleteId=$athleteIds[2]; exerciseId=$exerciseIds["Press Banca Competicion"]; weight=60; isTested=$false; notes="Estimado" }
    )

    foreach ($ml in $maxLifts) {
        $null = Post-Api "$base/athletes/$($ml.athleteId)/max-lifts" $ml
        Write-Host "  + Max lift $($ml.weight)kg para atleta $($ml.athleteId)" -ForegroundColor Green
    }
} else {
    Write-Host "  Skipped (need 3 athletes)" -ForegroundColor Yellow
}

# Register payments
Write-Host "`n=== SEEDING PAYMENTS ===" -ForegroundColor Cyan

if ($athleteIds.Count -ge 3 -and $subIds.Count -ge 3) {
    $payments = @(
        @{ athleteId=$athleteIds[0]; subscriptionId=$subIds[0]; amount=150; currency="EUR"; paymentDate=$today; paymentMethod="bank_transfer"; notes="Pago mensual Plan Premium - Abril" },
        @{ athleteId=$athleteIds[1]; subscriptionId=$subIds[1]; amount=200; currency="EUR"; paymentDate=$today; paymentMethod="bank_transfer"; notes="Pago Plan Competicion - Primer mes" },
        @{ athleteId=$athleteIds[2]; subscriptionId=$subIds[2]; amount=80; currency="EUR"; paymentDate=$today; paymentMethod="paypal"; notes="Pago Plan Basico - Abril" }
    )

    foreach ($pay in $payments) {
        $null = Post-Api "$base/payments" $pay
        Write-Host "  + Pago $($pay.amount) $($pay.currency) de atleta $($pay.athleteId)" -ForegroundColor Green
    }
} else {
    Write-Host "  Skipped (need 3 athletes and 3 subscriptions)" -ForegroundColor Yellow
}
try {
    $loginResult = Invoke-RestMethod "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"email":"admin@powercoach.com","password":"Admin@123456"}' -ErrorAction Stop
    $token = $loginResult.accessToken
    $authHeaders = @{ Authorization = "Bearer $token"; "Content-Type" = "application/json"; "X-Coach-Id" = $coachId }

    # Coach user
    try {
        $coachUser = Invoke-RestMethod "$base/auth/users" -Method POST -Headers $authHeaders -Body "{`"email`":`"coach@powercoach.com`",`"username`":`"demoCoach`",`"password`":`"Coach@123456`",`"role`":`"Coach`",`"coachId`":`"$coachId`"}" -ErrorAction Stop
        Write-Host "  + Coach user: coach@powercoach.com / Coach@123456" -ForegroundColor Green
    } catch {
        $s = $_.Exception.Response.StatusCode.value__
        if ($s -eq 409) { Write-Host "  ~ Coach user already exists" -ForegroundColor DarkGray }
        else { Write-Host "  ! Coach user error ($s): $_" -ForegroundColor Yellow }
    }

    # Athlete user (first athlete = Juan Carlos)
    if ($athleteIds.Count -ge 1) {
        $aid = $athleteIds[0]
        try {
            $athUser = Invoke-RestMethod "$base/auth/users" -Method POST -Headers $authHeaders -Body "{`"email`":`"juancarlos@gmail.com`",`"username`":`"juancarlos`",`"password`":`"Atleta@123456`",`"role`":`"Athlete`",`"athleteId`":`"$aid`"}" -ErrorAction Stop
            Write-Host "  + Athlete user: juancarlos@gmail.com / Atleta@123456" -ForegroundColor Green
        } catch {
            $s = $_.Exception.Response.StatusCode.value__
            if ($s -eq 409) { Write-Host "  ~ Athlete user already exists" -ForegroundColor DarkGray }
            else { Write-Host "  ! Athlete user error ($s): $_" -ForegroundColor Yellow }
        }
    }
} catch {
    Write-Host "  ! Could not login as admin to create users: $_" -ForegroundColor Red
    Write-Host "  Create users manually (see README)" -ForegroundColor Yellow
}

Write-Host "`n===========================================" -ForegroundColor Cyan
Write-Host " SEED COMPLETADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Datos creados:" -ForegroundColor White
Write-Host "  - 16 ejercicios (SBD + accesorios)" -ForegroundColor Gray
Write-Host "  - 5 aplicaciones (3 aprobadas, 1 rechazada, 1 pendiente)" -ForegroundColor Gray
Write-Host "  - 3 atletas con perfiles actualizados" -ForegroundColor Gray
Write-Host "  - 3 planes (Basico, Premium, Competicion)" -ForegroundColor Gray
Write-Host "  - 3 suscripciones" -ForegroundColor Gray
Write-Host "  - 3 programas (1 completo con ejercicios, 2 vacios)" -ForegroundColor Gray
Write-Host "  - 1 programa asignado a Juan Carlos" -ForegroundColor Gray
Write-Host "  - 4 check-ins" -ForegroundColor Gray
Write-Host "  - 8 max lifts" -ForegroundColor Gray
Write-Host "  - 3 pagos registrados" -ForegroundColor Gray
Write-Host "  - Login accounts created:" -ForegroundColor Gray
Write-Host "      Coach: coach@powercoach.com / Coach@123456" -ForegroundColor Gray
Write-Host "      Athlete: juancarlos@gmail.com / Atleta@123456" -ForegroundColor Gray
Write-Host ""
Write-Host "Abre http://localhost:3000 para probar!" -ForegroundColor Yellow
