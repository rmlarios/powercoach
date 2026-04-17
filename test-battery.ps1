# PowerCoach MVP - Integration Test Battery

$ErrorActionPreference = "Stop"

$base = "http://localhost:5000/api"
$coachId = "00000000-0000-0000-0000-000000000001"
$headers = @{ "X-Coach-Id" = $coachId }
$runId = Get-Date -Format "yyyyMMddHHmmss"
$today = Get-Date -Format "yyyy-MM-dd"

$pass = 0
$fail = 0
$total = 0

function Write-Section($title) {
    Write-Host ""
    Write-Host "===========================================" -ForegroundColor Cyan
    Write-Host " $title" -ForegroundColor Cyan
    Write-Host "===========================================" -ForegroundColor Cyan
}

function Test-Step($name, $block) {
    $script:total++

    try {
        $result = & $block
        Write-Host "  [PASS] $name" -ForegroundColor Green
        $script:pass++
        return $result
    }
    catch {
        Write-Host "  [FAIL] $name - $($_.Exception.Message)" -ForegroundColor Red
        $script:fail++
        return $null
    }
}

function To-JsonBody($body) {
    return ($body | ConvertTo-Json -Depth 10 -Compress)
}

function Get-Api($url) {
    Invoke-RestMethod -Uri $url -Method GET -Headers $headers
}

function Post-Api($url, $body = $null) {
    if ($null -eq $body) {
        Invoke-RestMethod -Uri $url -Method POST -Headers $headers
    }
    else {
        Invoke-RestMethod -Uri $url -Method POST -Headers $headers -Body (To-JsonBody $body) -ContentType "application/json"
    }
}

function Put-Api($url, $body) {
    Invoke-WebRequest -Uri $url -Method PUT -Headers $headers -Body (To-JsonBody $body) -ContentType "application/json" -UseBasicParsing | Out-Null
}

function Delete-Api($url) {
    Invoke-WebRequest -Uri $url -Method DELETE -Headers $headers -UseBasicParsing | Out-Null
}

function Get-IdValue($value) {
    if ($null -eq $value) { return $null }

    if ($value -is [string]) { return $value }

    if ($value.PSObject.Properties['id']) { return [string]$value.id }
    if ($value.PSObject.Properties['Id']) { return [string]$value.Id }

    return [string]$value
}

function Ensure-ApiIsUp() {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method GET
    if ($health -ne "Healthy") {
        throw "API health check failed: $health"
    }
}

Ensure-ApiIsUp

$squatName = "Low Bar Squat $runId"
$benchName = "Competition Bench $runId"
$deadliftName = "Conventional Deadlift $runId"
$curlName = "Bicep Curl $runId"
$planName = "Plan Competicion $runId"
$programName = "Fuerza 5x5 $runId"
$juanEmail = "juan.$runId@test.com"
$mariaEmail = "maria.$runId@test.com"

Write-Section "MODULE 1: EXERCISES"

$squatIdRaw = Test-Step "1.1 Create squat exercise" {
    Post-Api "$base/coaches/$coachId/exercises" @{
        name = $squatName
        category = "Squat"
        primaryMuscleGroup = "Quadriceps"
        description = "Powerlifting squat"
        equipment = "Barbell, Squat Rack"
        isCompound = $true
    }
}
$squatId = Get-IdValue $squatIdRaw

$benchIdRaw = Test-Step "1.2 Create bench exercise" {
    Post-Api "$base/coaches/$coachId/exercises" @{
        name = $benchName
        category = "Bench"
        primaryMuscleGroup = "Chest"
        equipment = "Barbell, Bench"
        isCompound = $true
    }
}
$benchId = Get-IdValue $benchIdRaw

$deadliftIdRaw = Test-Step "1.3 Create deadlift exercise" {
    Post-Api "$base/coaches/$coachId/exercises" @{
        name = $deadliftName
        category = "Deadlift"
        primaryMuscleGroup = "Back"
        equipment = "Barbell"
        isCompound = $true
    }
}
$deadliftId = Get-IdValue $deadliftIdRaw

$curlIdRaw = Test-Step "1.4 Create accessory exercise" {
    Post-Api "$base/coaches/$coachId/exercises" @{
        name = $curlName
        category = "Accessory"
        primaryMuscleGroup = "Biceps"
        isCompound = $false
    }
}
$curlId = Get-IdValue $curlIdRaw

Test-Step "1.5 List coach exercises" {
    $r = Get-Api "$base/coaches/$coachId/exercises"
    if ($r.Count -lt 4) { throw "Expected at least 4 exercises, got $($r.Count)" }
}

Test-Step "1.6 Filter by category" {
    $r = Get-Api "$base/coaches/$coachId/exercises?category=Squat"
    if ($r.Count -lt 1) { throw "Expected at least 1 squat exercise" }
}

Test-Step "1.7 Get exercise detail" {
    $r = Get-Api "$base/coaches/$coachId/exercises/$squatId"
    if ($r.name -ne $squatName) { throw "Unexpected exercise name: $($r.name)" }
}

Test-Step "1.8 Update exercise" {
    Put-Api "$base/coaches/$coachId/exercises/$squatId" @{
        name = "High Bar Squat $runId"
        category = "Squat"
        primaryMuscleGroup = "Quadriceps"
        description = "Olympic style"
        equipment = "Barbell, Squat Rack"
        isCompound = $true
    }
}

Test-Step "1.9 Verify update" {
    $r = Get-Api "$base/coaches/$coachId/exercises/$squatId"
    if ($r.name -ne "High Bar Squat $runId") { throw "Exercise update not persisted" }
}

Test-Step "1.10 Delete exercise (soft delete)" {
    Delete-Api "$base/coaches/$coachId/exercises/$curlId"
}

Test-Step "1.11 Filter active exercises" {
    $r = Get-Api "$base/coaches/$coachId/exercises?isActive=true"
    if ($r.Count -lt 3) { throw "Expected at least 3 active exercises, got $($r.Count)" }
}

Write-Section "MODULE 2: APPLICATIONS"

$app1IdRaw = Test-Step "2.1 Create application Juan" {
    Post-Api "$base/applications" @{
        coachId = $coachId
        firstName = "Juan"
        lastName = "Perez"
        email = $juanEmail
        age = 28
        gender = "Male"
        country = "Argentina"
        trainingExperience = "5 years"
        currentSquat = 180
        currentBench = 120
        currentDeadlift = 220
        motivation = "Competir en powerlifting"
        goals = "Total 600kg"
    }
}
$app1Id = Get-IdValue $app1IdRaw

$app2IdRaw = Test-Step "2.2 Create application Maria" {
    Post-Api "$base/applications" @{
        coachId = $coachId
        firstName = "Maria"
        lastName = "Garcia"
        email = $mariaEmail
        age = 25
        trainingExperience = "2 years"
        currentSquat = 80
        currentBench = 50
        currentDeadlift = 100
    }
}
$app2Id = Get-IdValue $app2IdRaw

Test-Step "2.3 List applications" {
    $r = Get-Api "$base/applications?coachId=$coachId"
    if ($r.items.Count -lt 2) { throw "Expected at least 2 applications, got $($r.items.Count)" }
}

Test-Step "2.4 Get application detail" {
    $r = Get-Api "$base/applications/$app1Id"
    if ($r.firstName -ne "Juan") { throw "Unexpected application firstName: $($r.firstName)" }
}

$approveResult = Test-Step "2.5 Approve application and create athlete" {
    Post-Api "$base/applications/$app1Id/approve" @{ notes = "Excelente candidato" }
}

$athleteId = $approveResult.athleteId

Test-Step "2.6 Verify application accepted" {
    $r = Get-Api "$base/applications/$app1Id"
    if ($r.status -ne "Accepted") { throw "Unexpected status: $($r.status)" }
}

Test-Step "2.7 Reject second application" {
    Post-Api "$base/applications/$app2Id/reject" @{ reason = "No cumple requisitos minimos" }
}

Test-Step "2.8 Verify application rejected" {
    $r = Get-Api "$base/applications/$app2Id"
    if ($r.status -ne "Rejected") { throw "Unexpected status: $($r.status)" }
}

Write-Section "MODULE 3: ATHLETES"

Test-Step "3.1 List athletes" {
    $r = Get-Api "$base/athletes?coachId=$coachId"
    if ($r.items.Count -lt 1) { throw "Expected at least 1 athlete, got $($r.items.Count)" }
}

Test-Step "3.2 Get athlete detail" {
    $r = Get-Api "$base/athletes/$athleteId"
    if ($r.firstName -ne "Juan") { throw "Unexpected athlete firstName: $($r.firstName)" }
}

Test-Step "3.3 Update athlete" {
    Put-Api "$base/athletes/$athleteId" @{
        country = "Argentina"
        height = 178
        weight = 93
        experienceLevel = "Intermediate"
    }
}

Test-Step "3.4 Verify athlete update" {
    $r = Get-Api "$base/athletes/$athleteId"
    if ([decimal]$r.weight -ne 93) { throw "Unexpected athlete weight: $($r.weight)" }
}

Write-Section "MODULE 4: PLANS AND SUBSCRIPTIONS"

$planIdRaw = Test-Step "4.1 Create plan" {
    Post-Api "$base/plans" @{
        coachId = $coachId
        name = $planName
        description = "Plan premium para competidores"
        price = 150
        currency = "USD"
        durationInMonths = 1
    }
}
$planId = Get-IdValue $planIdRaw

Test-Step "4.2 List plans" {
    $r = Get-Api "$base/plans?coachId=$coachId"
    if ($r.Count -lt 1) { throw "Expected at least 1 plan" }
}

$subIdRaw = Test-Step "4.3 Create subscription" {
    Post-Api "$base/subscriptions" @{
        athleteId = $athleteId
        planId = $planId
        startDate = $today
    }
}
$subId = Get-IdValue $subIdRaw

Test-Step "4.4 List athlete subscriptions" {
    $r = Get-Api "$base/athletes/$athleteId/subscriptions"
    if ($r.Count -lt 1) { throw "Expected at least 1 subscription" }
}

Write-Section "MODULE 5: PROGRAM TEMPLATES"

$progIdRaw = Test-Step "5.1 Create training program" {
    Post-Api "$base/programs" @{
        coachId = $coachId
        name = $programName
        description = "Programa de fuerza lineal"
        durationWeeks = 4
    }
}
$progId = Get-IdValue $progIdRaw

$week1IdRaw = Test-Step "5.2 Add week 1" {
    Post-Api "$base/programs/$progId/weeks?coachId=$coachId" @{
        weekNumber = 1
        name = "Semana 1"
    }
}
$week1Id = Get-IdValue $week1IdRaw

$day1IdRaw = Test-Step "5.3 Add day 1" {
    Post-Api "$base/programs/$progId/weeks/$week1Id/days?coachId=$coachId" @{
        dayNumber = 1
        name = "Dia 1"
        focus = "Push"
    }
}
$day1Id = Get-IdValue $day1IdRaw

$null = Test-Step "5.4 Add squat to program day" {
    Post-Api "$base/programs/$progId/weeks/$week1Id/days/$day1Id/exercises?coachId=$coachId" @{
        exerciseId = $squatId
        sets = 5
        reps = "5"
        targetRpe = 8
        restSeconds = 180
        notes = "Foco en profundidad"
    }
}

$null = Test-Step "5.5 Add bench to program day" {
    Post-Api "$base/programs/$progId/weeks/$week1Id/days/$day1Id/exercises?coachId=$coachId" @{
        exerciseId = $benchId
        sets = 5
        reps = "5"
        targetRpe = 8
        restSeconds = 180
    }
}

Test-Step "5.6 Verify program detail" {
    $detailUrl = "$base/programs/${progId}?coachId=$coachId"
    $r = Get-Api $detailUrl
    if ($r.weeks.Count -lt 1) { throw "Expected at least 1 week in program detail" }
}

Write-Section "MODULE 6: PROGRAM ASSIGNMENT"

$athProgIdRaw = Test-Step "6.1 Assign program to athlete" {
    Post-Api "$base/athletes/$athleteId/assign-program?coachId=$coachId" @{
        programTemplateId = $progId
        startDate = $today
        notes = "Inicio de ciclo"
    }
}
$athProgId = Get-IdValue $athProgIdRaw

$currentProgram = Test-Step "6.2 Get athlete current program" {
    $r = Get-Api "$base/athletes/$athleteId/program"
    if (-not $r.programName) { throw "No current program returned" }
    $r
}

$weekWorkouts = Test-Step "6.3 Get athlete weekly workouts" {
    Get-Api "$base/athletes/$athleteId/workouts/week"
}

$workoutId = $null
if ($currentProgram -and $currentProgram.weekSchedule -and $currentProgram.weekSchedule.Count -gt 0) {
    foreach ($w in $currentProgram.weekSchedule) {
        if ($w.days -and $w.days.Count -gt 0) {
            foreach ($d in $w.days) {
                if ($d.workoutId) { $workoutId = $d.workoutId; break }
            }
            if ($workoutId) { break }
        }
    }
}
if (-not $workoutId -and $weekWorkouts -and $weekWorkouts.Count -gt 0) {
    $workoutId = $weekWorkouts[0].id
}
if (-not $workoutId -and $weekWorkouts -and $weekWorkouts.workouts -and $weekWorkouts.workouts.Count -gt 0) {
    $workoutId = $weekWorkouts.workouts[0].id
}

Write-Section "MODULE 7: WORKOUT TRACKING"

if ($workoutId) {
    Test-Step "7.1 Start workout" {
        Post-Api "$base/athletes/$athleteId/workouts/$workoutId/start"
    }

    Test-Step "7.2 Save set 1" {
        Put-Api "$base/athletes/$athleteId/workouts/$workoutId/sets" @{
            exerciseLogId = $null
            exerciseId = $squatId
            setNumber = 1
            reps = 5
            weight = 140
            rpe = 7
            targetReps = 5
            targetWeight = 140
            isCompleted = $true
            notes = "Set 1"
        }
    }

    Test-Step "7.3 Save set 2" {
        Put-Api "$base/athletes/$athleteId/workouts/$workoutId/sets" @{
            exerciseLogId = $null
            exerciseId = $squatId
            setNumber = 2
            reps = 5
            weight = 145
            rpe = 8
            targetReps = 5
            targetWeight = 145
            isCompleted = $true
            notes = "Set 2"
        }
    }

    Test-Step "7.4 Complete workout" {
        Post-Api "$base/athletes/$athleteId/workouts/$workoutId/complete" @{
            durationMinutes = 75
            fatigueRating = 7
            notes = "Buena sesion"
        }
    }

    Test-Step "7.5 Verify completed workout" {
        $r = Get-Api "$base/athletes/$athleteId/workouts/$workoutId"
        if ($r.status -ne "Completed" -and $r.statusName -ne "Completed") { throw "Workout status is '$($r.status)', expected 'Completed'" }
    }
}
else {
    Write-Host "  [WARN] Skipping workout tracking tests - no workoutId found" -ForegroundColor Yellow
}

Write-Section "MODULE 8: CHECK-INS AND PAYMENTS"

Test-Step "8.1 Create check-in" {
    Post-Api "$base/check-ins" @{
        athleteId = $athleteId
        weight = 93.5
        fatigueLevel = 6
        sleepQuality = 8
        motivationLevel = 9
        notes = "Me siento bien"
    }
}

Test-Step "8.2 List athlete check-ins" {
    $r = Get-Api "$base/athletes/$athleteId/checkins"
    if ($r.Count -lt 1) { throw "Expected at least 1 check-in" }
}

Test-Step "8.3 Register payment" {
    Post-Api "$base/payments" @{
        athleteId = $athleteId
        subscriptionId = $subId
        amount = 150
        currency = "USD"
        paymentDate = $today
        paymentMethod = "bank_transfer"
        notes = "TRF-001"
    }
}

Test-Step "8.4 List athlete payments" {
    $r = Get-Api "$base/athletes/$athleteId/payments"
    if ($r.Count -lt 1) { throw "Expected at least 1 payment" }
}

Write-Section "MODULE 9: MAX LIFTS AND DASHBOARD"

Test-Step "9.1 Register squat max lift" {
    Post-Api "$base/athletes/$athleteId/max-lifts" @{
        exerciseId = $squatId
        weight = 180
        isTested = $true
        notes = "PR en competencia"
    }
}

Test-Step "9.2 Register bench max lift" {
    Post-Api "$base/athletes/$athleteId/max-lifts" @{
        exerciseId = $benchId
        weight = 120
        isTested = $true
    }
}

Test-Step "9.3 Get max lifts" {
    $r = Get-Api "$base/athletes/$athleteId/max-lifts"
    if (-not $r) { throw "No max lifts response returned" }
}

Test-Step "9.4 Get coach dashboard" {
    $r = Get-Api "$base/dashboard/coach?coachId=$coachId"
    if (-not $r) { throw "No dashboard response returned" }
}

Write-Host ""
Write-Host "===========================================" -ForegroundColor Yellow
Write-Host " RESULTS" -ForegroundColor Yellow
Write-Host "===========================================" -ForegroundColor Yellow
Write-Host "  Total: $total" -ForegroundColor White
Write-Host "  Pass:  $pass" -ForegroundColor Green

$failColor = if ($fail -gt 0) { "Red" } else { "Green" }
Write-Host "  Fail:  $fail" -ForegroundColor $failColor
Write-Host ""

if ($fail -gt 0) {
    exit 1
}

exit 0
