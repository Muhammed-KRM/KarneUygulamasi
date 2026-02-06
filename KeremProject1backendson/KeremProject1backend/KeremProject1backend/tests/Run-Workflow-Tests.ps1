$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:5070/api"
$DetailedLog = "comprehensive_test_results.txt"

# Initialize detailed log
$LogHeader = @"
================================================================================
                    COMPREHENSIVE TEST RESULTS
================================================================================
Test Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Base URL: $BaseUrl
================================================================================

"@
$LogHeader | Out-File $DetailedLog -Encoding UTF8

# Global test tracking
$script:TestResults = @()
$script:TestCounter = 0

function Write-TestLog {
    param($Message)
    Add-Content $DetailedLog $Message
}

function Log-TestStart {
    param($TestName)
    $script:TestCounter++
    Write-TestLog "`n================================================================================`n  TEST #$script:TestCounter : $TestName`n================================================================================"
    Write-Host "`n--- TEST #$script:TestCounter : $TestName ---" -ForegroundColor Cyan
}

function Log-Request {
    param($Method, $Uri, $Body, $HasToken)
    
    $FullUrl = "$BaseUrl$Uri"
    $TokenInfo = if ($HasToken) { "YES (included in headers)" } else { "NO" }
    
    $RequestLog = @"

[REQUEST]
  Method: $Method
  URL: $FullUrl
  Has Token: $TokenInfo
  Body:
$(if ($Body) { ($Body | ConvertTo-Json -Depth 5) } else { "    (no body)" })

"@
    Write-TestLog $RequestLog
}

function Log-Response {
    param($StatusCode, $ResponseBody, $Success)
    
    $Status = if ($Success) { "✅ SUCCESS" } else { "❌ FAILED" }
    
    $ResponseLog = @"
[RESPONSE]
  Status: $Status
  HTTP Code: $StatusCode
  Body:
$($ResponseBody | ConvertTo-Json -Depth 5)

"@
    Write-TestLog $ResponseLog
}

function Log-TestResult {
    param($TestName, $Endpoint, $Success, $ErrorMessage = "")
    
    $script:TestResults += [PSCustomObject]@{
        TestName = $TestName
        Endpoint = $Endpoint
        Success  = $Success
        Error    = $ErrorMessage
    }
    
    if ($Success) {
        Write-TestLog "[RESULT] ✅ PASS: $TestName"
        Write-Host "✅ PASS: $TestName" -ForegroundColor Green
    }
    else {
        Write-TestLog "[RESULT] ❌ FAIL: $TestName"
        Write-TestLog "[ERROR] $ErrorMessage"
        Write-Host "❌ FAIL: $TestName" -ForegroundColor Red
    }
}

function Api-Post {
    param ($TestName, $Uri, $Body, $Token)
    
    $Headers = @{}
    if ($Token) { $Headers["Authorization"] = "Bearer $Token" }
    
    Log-Request -Method "POST" -Uri $Uri -Body $Body -HasToken ($null -ne $Token)
    
    try {
        if ($Body) {
            $Response = Invoke-RestMethod -Uri "$BaseUrl$Uri" -Method Post -Body ($Body | ConvertTo-Json -Depth 5) -ContentType "application/json" -Headers $Headers
        }
        else {
            $Response = Invoke-RestMethod -Uri "$BaseUrl$Uri" -Method Post -ContentType "application/json" -Headers $Headers
        }
        
        Log-Response -StatusCode 200 -ResponseBody $Response -Success $true
        Log-TestResult -TestName $TestName -Endpoint "POST $Uri" -Success $true
        
        return $Response
    }
    catch {
        $StatusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.Value__ } else { "N/A" }
        $ErrorBody = ""
        
        if ($_.Exception.Response) {
            $Stream = $_.Exception.Response.GetResponseStream()
            if ($Stream) {
                $Reader = New-Object System.IO.StreamReader($Stream)
                $ErrorBody = $Reader.ReadToEnd()
                $Reader.Close()
                $Stream.Close()
            }
        }
        
        Log-Response -StatusCode $StatusCode -ResponseBody $ErrorBody -Success $false
        Log-TestResult -TestName $TestName -Endpoint "POST $Uri" -Success $false -ErrorMessage $_.Exception.Message
        
        throw $_
    }
}

function Api-Get {
    param ($TestName, $Uri, $Token)
    
    $Headers = @{}
    if ($Token) { $Headers["Authorization"] = "Bearer $Token" }
    
    Log-Request -Method "GET" -Uri $Uri -Body $null -HasToken ($null -ne $Token)
    
    try {
        $Response = Invoke-RestMethod -Uri "$BaseUrl$Uri" -Method Get -Headers $Headers
        
        Log-Response -StatusCode 200 -ResponseBody $Response -Success $true
        Log-TestResult -TestName $TestName -Endpoint "GET $Uri" -Success $true
        
        return $Response
    }
    catch {
        $StatusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.Value__ } else { "N/A" }
        $ErrorBody = ""
        
        if ($_.Exception.Response) {
            $Stream = $_.Exception.Response.GetResponseStream()
            if ($Stream) {
                $Reader = New-Object System.IO.StreamReader($Stream)
                $ErrorBody = $Reader.ReadToEnd()
                $Reader.Close()
                $Stream.Close()
            }
        }
        
        Log-Response -StatusCode $StatusCode -ResponseBody $ErrorBody -Success $false
        Log-TestResult -TestName $TestName -Endpoint "GET $Uri" -Success $false -ErrorMessage $_.Exception.Message
        
        throw $_
    }
}

# ============================================================================
# TEST SCENARIOS
# ============================================================================

Log-TestStart "Login System Admin (AdminAdmin)"
try {
    $AdminAuth = Api-Post -TestName "AdminAdmin Login" -Uri "/auth/login" -Body @{ username = "admin"; password = "Admin123!" }
    $AdminToken = $AdminAuth.data.token
}
catch {
    exit
}

Log-TestStart "Create New Admin using AdminAdmin"
$NewAdminUser = "admin_$(Get-Date -UFormat %s)"
try {
    $CreateAdminBody = @{
        fullName = "New Admin User"
        username = $NewAdminUser
        email    = "$NewAdminUser@test.com"
        password = "Password123!"
        role     = 1
    }
    $CreateAdminResult = Api-Post -TestName "Create Admin Account" -Uri "/admin/create-admin" -Body $CreateAdminBody -Token $AdminToken
}
catch {
    # Continue
}

Log-TestStart "Login as New Admin"
try {
    $NewAdminAuth = Api-Post -TestName "New Admin Login" -Uri "/auth/login" -Body @{ username = $NewAdminUser; password = "Password123!" }
    $NewAdminToken = $NewAdminAuth.data.token
}
catch {
    # Continue
}

Log-TestStart "Register Manager User"
$ManagerUser = "manager_test_$(Get-Date -UFormat %s)"
try {
    $RegBody = @{
        fullName    = "Test Manager"
        username    = $ManagerUser
        email       = "$ManagerUser@test.com"
        password    = "Password123!"
        phoneNumber = "5550001122"
    }
    Api-Post -TestName "Register Manager Account" -Uri "/auth/register" -Body $RegBody
}
catch {
    # Continue
}

Log-TestStart "Login Manager User"
try {
    $ManagerAuth = Api-Post -TestName "Manager Login" -Uri "/auth/login" -Body @{ username = $ManagerUser; password = "Password123!" }
    $ManagerToken = $ManagerAuth.data.token
}
catch {
    # Continue
}

Log-TestStart "Apply for Institution"
try {
    $InstBody = @{
        name          = "Test Institution $(Get-Date -UFormat %s)"
        licenseNumber = "LIC-$(Get-Date -UFormat %s)"
        address       = "123 Test St"
        phone         = "5551112233"
    }
    Api-Post -TestName "Institution Application" -Uri "/auth/apply-institution" -Body $InstBody -Token $ManagerToken
}
catch {
    # Continue
}

Log-TestStart "Get Pending Institutions"
try {
    $Pending = Api-Get -TestName "List Pending Institutions" -Uri "/admin/pending-institutions" -Token $AdminToken
    $InstId = 0
    
    if ($Pending.data.Count -gt 0) {
        $InstId = $Pending.data[0].id
        Write-TestLog "`n[INFO] Found Pending Institution ID: $InstId"
        
        Log-TestStart "Approve Institution (AdminAdmin)"
        try {
            Api-Post -TestName "Approve Institution" -Uri "/admin/approve-institution/$InstId" -Body $null -Token $AdminToken
        }
        catch {
            # Continue
        }
    }
}
catch {
    # Continue
}

Log-TestStart "Create Classroom"
try {
    $ManagerAuthRefreshed = Api-Post -TestName "Re-login Manager (to get updated token)" -Uri "/auth/login" -Body @{ username = $ManagerUser; password = "Password123!" }
    $ManagerTokenRefreshed = $ManagerAuthRefreshed.data.token
    
    $MyInsts = $ManagerAuthRefreshed.data.user.institutions
    if ($MyInsts.Count -gt 0) {
        $InstId = $MyInsts[0].id
        Write-TestLog "`n[INFO] Manager has Institution ID: $InstId"
        
        $ClassBody = @{
            institutionId = $InstId
            name          = "Class 10-A"
            grade         = 10
        }
        Api-Post -TestName "Create Classroom" -Uri "/classroom/create" -Body $ClassBody -Token $ManagerTokenRefreshed
    }
    else {
        Write-TestLog "[ERROR] Manager has no institutions!"
        Log-TestResult -TestName "Create Classroom" -Endpoint "POST /classroom/create" -Success $false -ErrorMessage "Manager has no institutions"
    }
}
catch {
    # Continue
}

Log-TestStart "Register & Login Student"
$StudentUser = "student_$(Get-Date -UFormat %s)"
try {
    $RegBodySt = @{
        fullName    = "Test Student"
        username    = $StudentUser
        email       = "$StudentUser@test.com"
        password    = "Password123!"
        phoneNumber = "5559998877"
    }
    Api-Post -TestName "Register Student Account" -Uri "/auth/register" -Body $RegBodySt
    $StAuth = Api-Post -TestName "Student Login" -Uri "/auth/login" -Body @{ username = $StudentUser; password = "Password123!" }
}
catch {
    # Continue
}

# ============================================================================
# TEST SUMMARY
# ============================================================================

Write-TestLog "`n`n"
Write-TestLog "================================================================================"
Write-TestLog "                              TEST SUMMARY"
Write-TestLog "================================================================================"
Write-TestLog ""

$TotalTests = $script:TestResults.Count
$PassedTests = ($script:TestResults | Where-Object { $_.Success }).Count
$FailedTests = $TotalTests - $PassedTests
$SuccessRate = if ($TotalTests -gt 0) { [math]::Round(($PassedTests / $TotalTests) * 100, 2) } else { 0 }

Write-TestLog "Total Tests:   $TotalTests"
Write-TestLog "Passed:        $PassedTests ✅"
Write-TestLog "Failed:        $FailedTests ❌"
Write-TestLog "Success Rate:  $SuccessRate%"
Write-TestLog ""
Write-TestLog "================================================================================"
Write-TestLog "                         ENDPOINT TEST RESULTS"
Write-TestLog "================================================================================"
Write-TestLog ""

foreach ($Result in $script:TestResults) {
    $Status = if ($Result.Success) { "✅" } else { "❌" }
    $EndpointFormatted = $Result.Endpoint.PadRight(50)
    Write-TestLog "$Status $EndpointFormatted | $($Result.TestName)"
}

Write-TestLog ""
Write-TestLog "================================================================================"
Write-TestLog "                              END OF REPORT"
Write-TestLog "================================================================================"

# Console Summary
Write-Host "`n`n" -ForegroundColor Yellow
Write-Host "================================================================================" -ForegroundColor Yellow
Write-Host "                              TEST SUMMARY" -ForegroundColor Yellow
Write-Host "================================================================================" -ForegroundColor Yellow
Write-Host "Total Tests:   $TotalTests" -ForegroundColor White
Write-Host "Passed:        $PassedTests ✅" -ForegroundColor Green
Write-Host "Failed:        $FailedTests ❌" -ForegroundColor Red
Write-Host "Success Rate:  $SuccessRate%" -ForegroundColor Cyan
Write-Host ""
Write-Host "📄 Detailed report saved to: $DetailedLog" -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Yellow
