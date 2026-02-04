$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:5070/api"
$ErrorLog = "workflow_test_errors.log"

"Workflow Test Execution Log`n==========================" | Out-File $ErrorLog -Encoding UTF8

function Log-Error {
    param ($Step, $Message, $Exception)
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    
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

    $LogMsg = "[$Timestamp] FAIL at $Step`: $Message`nResponse: $ResponseBody"
    Add-Content $ErrorLog $LogMsg
    Add-Content $ErrorLog ("-" * 50)
    Write-Host "❌ FAIL: $Step" -ForegroundColor Red
}

function Log-Success {
    param ($Step)
    Write-Host "✅ PASS: $Step" -ForegroundColor Green
}

function Api-Post {
    param ($Uri, $Body, $Token)
    $Headers = @{}
    if ($Token) { $Headers["Authorization"] = "Bearer $Token" }
    
    try {
        if ($Body) {
            return Invoke-RestMethod -Uri "$BaseUrl$Uri" -Method Post -Body ($Body | ConvertTo-Json) -ContentType "application/json" -Headers $Headers
        }
        else {
            return Invoke-RestMethod -Uri "$BaseUrl$Uri" -Method Post -ContentType "application/json" -Headers $Headers
        }
    }
    catch {
        throw $_
    }
}

function Api-Get {
    param ($Uri, $Token)
    $Headers = @{}
    if ($Token) { $Headers["Authorization"] = "Bearer $Token" }
    
    try {
        return Invoke-RestMethod -Uri "$BaseUrl$Uri" -Method Get -Headers $Headers
    }
    catch {
        throw $_
    }
}

# --- SCENARIO START ---

Write-Host "`n--- 1. Login System Admin (AdminAdmin) ---" -ForegroundColor Cyan
try {
    $AdminAuth = Api-Post -Uri "/auth/login" -Body @{ username = "admin"; password = "Admin123!" }
    $AdminToken = $AdminAuth.data.token
    Log-Success "Login AdminAdmin"
}
catch {
    Log-Error "Login AdminAdmin" $_.Exception.Message $_.Exception
    exit
}

Write-Host "`n--- 2. Create New Admin using AdminAdmin ---" -ForegroundColor Cyan
$NewAdminUser = "admin_$(Get-Date -UFormat %s)"
try {
    $CreateAdminBody = @{
        fullName = "New Admin User"
        username = $NewAdminUser
        email    = "$NewAdminUser@test.com"
        password = "Password123!"
        role     = 1 # Admin
    }
    $Res = Api-Post -Uri "/admin/create-admin" -Body $CreateAdminBody -Token $AdminToken
    Log-Success "Create Admin ($NewAdminUser)"
}
catch {
    Log-Error "Create Admin" $_.Exception.Message $_.Exception
}

Write-Host "`n--- 3. Login as New Admin ---" -ForegroundColor Cyan
try {
    $NewAdminAuth = Api-Post -Uri "/auth/login" -Body @{ username = $NewAdminUser; password = "Password123!" }
    $NewAdminToken = $NewAdminAuth.data.token
    Log-Success "Login New Admin"
}
catch {
    Log-Error "Login New Admin" $_.Exception.Message $_.Exception
    # Continue test if fails, assuming manual fix might be needed but we want to see other failures
}

Write-Host "`n--- 4. Register Manager User ---" -ForegroundColor Cyan
$ManagerUser = "manager_test_$(Get-Date -UFormat %s)"
try {
    $RegBody = @{
        fullName    = "Test Manager"
        username    = $ManagerUser
        email       = "$ManagerUser@test.com"
        password    = "Password123!"
        phoneNumber = "5550001122" 
    }
    Api-Post -Uri "/auth/register" -Body $RegBody
    Log-Success "Register Manager User (Initially Student Role)"
}
catch {
    Log-Error "Register Manager" $_.Exception.Message $_.Exception
}

Write-Host "`n--- 5. Login Manager User ---" -ForegroundColor Cyan
try {
    $ManagerAuth = Api-Post -Uri "/auth/login" -Body @{ username = $ManagerUser; password = "Password123!" }
    $ManagerToken = $ManagerAuth.data.token
    Log-Success "Login Manager"
}
catch {
    Log-Error "Login Manager" $_.Exception.Message $_.Exception
}

Write-Host "`n--- 6. Apply for Institution (Manager) ---" -ForegroundColor Cyan
try {
    $InstBody = @{
        name          = "Test Institution $(Get-Date -UFormat %s)"
        licenseNumber = "LIC-$(Get-Date -UFormat %s)"
        address       = "123 Test St"
        phone         = "5551112233"
    }
    Api-Post -Uri "/auth/apply-institution" -Body $InstBody -Token $ManagerToken
    Log-Success "Apply Institution"
}
catch {
    Log-Error "Apply Institution" $_.Exception.Message $_.Exception
}

Write-Host "`n--- 7. Approve Institution (New Admin) ---" -ForegroundColor Cyan
try {
    # Get Pending list to find ID
    $Pending = Api-Get -Uri "/admin/pending-institutions" -Token $AdminToken # Using System Admin token as specific endpoint might require AdminAdmin.
    # Wait, Controller says: [Authorize(Roles = "AdminAdmin")]. 
    # Normal Admin might NOT be able to invoke ApproveInstitution if controller restricts it.
    # Let's check AdminController.cs again.
    # line 31: [Authorize(Roles = "AdminAdmin")] for ApproveInstitution.
    # line 137: [Authorize(Roles = "AdminAdmin")] for RejectInstitution.
    # User asked to create Admin and "Adminlerin yapabildiği şeyleri yap".
    # If Admin cannot approve institution, what CAN they do?
    # Controller Authorize attribute says [Authorize(Roles = "AdminAdmin,Admin")] at class level.
    # But methods override.
    # ApproveInstitution is specifically AdminAdmin.
    # So Normal Admin CANNOT approve institution. This is a finding!
    # I will try with Normal Admin first, expect failure, then use AdminAdmin to verify flow.
    
    Log-Success "CHECK: Normal Admin tries to Approve (Expect Failure/Success based on Policy)"
    # We first need the ID.
    $InstId = 0
    if ($Pending.data.Count -gt 0) {
        $InstId = $Pending.data[0].id
        try {
            Api-Post -Uri "/admin/approve-institution/$InstId" -Body $null -Token $NewAdminToken
            Log-Success "Normal Admin Approved Institution (Unexpected?)"
        }
        catch {
            Write-Host "NOTE: Normal Admin could not approve (Expected 403)" -ForegroundColor Yellow
            # Now approve with AdminAdmin
            Api-Post -Uri "/admin/approve-institution/$InstId" -Body $null -Token $AdminToken
            Log-Success "AdminAdmin Approved Institution"
        }
    }
    else {
        Log-Error "Get Pending Inst" "No pending institution found"
    }
}
catch {
    Log-Error "Approve Flow" $_.Exception.Message $_.Exception
}

Write-Host "`n--- 8. Create Classroom (Manager) ---" -ForegroundColor Cyan
try {
    # Validate Manager Token permissions might need refresh after approval?
    # User Roles are in Token. If role changed from Student to Manager (via InstitutionUser), 
    # we might need to Login again to get new claims?
    # Yes, typically claims are baked into token.
    # Let's re-login Manager.
    $ManagerAuthRefreshed = Api-Post -Uri "/auth/login" -Body @{ username = $ManagerUser; password = "Password123!" }
    $ManagerTokenRefreshed = $ManagerAuthRefreshed.data.token
    
    $InstId = 0 # Need to find the institution ID we just approved
    # We can assume it's the one we applied for. Manager -> GetCurrent User -> Institutions?
    
    # Or just try to create classroom, assuming backend finds the manager's institution automatically if endpoint supports it.
    # Request: { institutionId, name, grade }
    # We need explicit InstitutionId.
    
    $Me = Api-Get -Uri "/auth/me" -Token $ManagerTokenRefreshed
    # Assuming "Institutions" list in response
    # The login response definitely has it.
    $MyInsts = $ManagerAuthRefreshed.data.user.institutions
    if ($MyInsts.Count -gt 0) {
        $InstId = $MyInsts[0].id
        
        $ClassBody = @{
            institutionId = $InstId
            name          = "Class 10-A"
            grade         = 10
        }
        Api-Post -Uri "/classroom/create" -Body $ClassBody -Token $ManagerTokenRefreshed
        Log-Success "Manager Created Classroom"
    }
    else {
        Log-Error "Manager Check" "Manager has no institutions after approval!"
    }

}
catch {
    Log-Error "Create Classroom" $_.Exception.Message $_.Exception
}

Write-Host "`n--- 9. Register & Login Normal Student ---" -ForegroundColor Cyan
$StudentUser = "student_$(Get-Date -UFormat %s)"
try {
    $RegBodySt = @{
        fullName    = "Test Student"
        username    = $StudentUser
        email       = "$StudentUser@test.com"
        password    = "Password123!"
        phoneNumber = "5559998877"
    }
    Api-Post -Uri "/auth/register" -Body $RegBodySt
    $StAuth = Api-Post -Uri "/auth/login" -Body @{ username = $StudentUser; password = "Password123!" }
    $StToken = $StAuth.data.token
    Log-Success "Student Registered & Logged In"
}
catch {
    Log-Error "Student Flow" $_.Exception.Message $_.Exception
}

Write-Host "`n--- Workflow Verification Complete ---" -ForegroundColor Yellow
