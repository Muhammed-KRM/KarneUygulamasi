$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:5070/api"
$ErrorLog = "test_errors.log"

# Setup Log
"Test Execution Log`n==================" | Out-File $ErrorLog -Encoding UTF8

function Log-Error {
    param ($Step, $Message, $Exception)
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    # Try to read response body if it exists
    $ResponseBody = ""
    if ($Exception -and $Exception.Response) {
        $Stream = $Exception.Response.GetResponseStream()
        if ($Stream) {
            $Reader = New-Object System.IO.StreamReader($Stream)
            $ResponseBody = $Reader.ReadToEnd()
            $Reader.Close()
            $Stream.Close()
        }
    }

    $LogMsg = "[$Timestamp] FAIL at $Step`: $Message`n$ResponseBody"
    Add-Content $ErrorLog $LogMsg
    Add-Content $ErrorLog ("-" * 50)
    Write-Host "❌ FAIL: $Step - Check $ErrorLog" -ForegroundColor Red
}

function Log-Success {
    param ($Step)
    Write-Host "✅ PASS: $Step" -ForegroundColor Green
}

# 1. AUTHENTICATION
Write-Host "`n--- Testing Authentication ---" -ForegroundColor Cyan
$ManagerEmail = "manager_$(Get-Date -UFormat %s)@test.com"
$ManagerData = @{
    email    = $ManagerEmail
    password = "Password123!"
    fullName = "Test Manager"
    phone    = "5551234567"
}

try {
    $Res = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method Post -Body ($ManagerData | ConvertTo-Json) -ContentType "application/json"
    Log-Success "Register User"
}
catch {
    Log-Error "Register User" $_.Exception.Message $_.Exception
    exit
}

$LoginData = @{
    email    = $ManagerEmail
    password = "Password123!"
}

try {
    $Res = Invoke-RestMethod -Uri "$BaseUrl/auth/login" -Method Post -Body ($LoginData | ConvertTo-Json) -ContentType "application/json"
    $Token = $Res.data.token
    if ($Token) {
        Log-Success "Login Manager"
    }
    else {
        throw "No token received"
    }
}
catch {
    Log-Error "Login Manager" $_.Exception.Message
    exit
}

$Headers = @{ Authorization = "Bearer $Token" }

# 2. CLASSROOM
Write-Host "`n--- Testing Classroom ---" -ForegroundColor Cyan

# Approval Hack: Since Manager just registered, they might not have an approved institution.
# But for now, let's try creating classroom directly. If fails, it's a valid test failure.
$ClassData = @{
    institutionId = 1 # Warning: This assumes ID 1 exists and is approved!
    name          = "Test Class $(Get-Date -UFormat %s)"
    grade         = 10
}

try {
    $Res = Invoke-RestMethod -Uri "$BaseUrl/classroom" -Method Post -Body ($ClassData | ConvertTo-Json) -ContentType "application/json" -Headers $Headers
    $ClassId = $Res.data
    Log-Success "Create Classroom (ID: $ClassId)"
}
catch {
    Log-Error "Create Classroom" $_.Exception.Message
    # Don't exit, maybe we can test messaging differently
}

# 3. MESSAGE
Write-Host "`n--- Testing Message ---" -ForegroundColor Cyan

try {
    # Get Conversations to find a target
    $Res = Invoke-RestMethod -Uri "$BaseUrl/message/conversations" -Method Get -Headers $Headers
    $Convs = $Res.data
    
    if ($Convs.Count -gt 0) {
        $TargetConv = $Convs[0].id
        
        $MsgData = @{
            conversationId = $TargetConv
            content        = "Automated Test Message $(Get-Date)"
            type           = 0
        }
        
        $ResMsg = Invoke-RestMethod -Uri "$BaseUrl/message/send" -Method Post -Body ($MsgData | ConvertTo-Json) -ContentType "application/json" -Headers $Headers
        Log-Success "Send Message (SignalR Trigger)"
    }
    else {
        # If we just created a class, we might need to refresh or fetch specific class details
        Log-Error "Get Conversations" "No conversations found for user."
    }
}
catch {
    Log-Error "Message Flow" $_.Exception.Message
}

Write-Host "`nTests Completed. Check $ErrorLog for failures." -ForegroundColor Yellow
