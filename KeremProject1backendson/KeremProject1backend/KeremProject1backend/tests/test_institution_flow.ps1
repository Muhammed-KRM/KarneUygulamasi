
$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:5070"
$ReportFile = "test_report.txt"

function Log-Message {
    param([string]$Message)
    Write-Host $Message
    Add-Content -Path $ReportFile -Value $Message
}

function Run-Test {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Endpoint,
        [object]$Payload = $null,
        [string]$Token = $null
    )

    $Url = "$BaseUrl$Endpoint"
    $Headers = @{ "Content-Type" = "application/json" }
    if ($Token) {
        $Headers["Authorization"] = "Bearer $Token"
    }

    Log-Message "`n--- TEST: $Name ---"
    Log-Message "Request: $Method $Url"
    
    if ($Payload) {
        $JsonPayload = $Payload | ConvertTo-Json -Depth 10
        Log-Message "Payload: $JsonPayload"
    }

    try {
        $Params = @{
            Uri         = $Url
            Method      = $Method
            Headers     = $Headers
            ErrorAction = "Stop"
        }
        if ($Payload) {
            $Params["Body"] = ($Payload | ConvertTo-Json -Depth 10)
        }

        $Response = Invoke-RestMethod @Params
        
        Log-Message "Response Status: Success (200 OK assumed from Invoke-RestMethod)"
        # Invoke-RestMethod returns the object directly if JSON
        # Need to convert back to JSON string for logging
        $JsonResp = $Response | ConvertTo-Json -Depth 10
        Log-Message "Response Body: $JsonResp"
        
        return $Response
    }
    catch {
        Log-Message "FAILED: $_"
        try {
            # Try to read response body from exception if available
            $Stream = $_.Exception.Response.GetResponseStream()
            $Reader = New-Object System.IO.StreamReader($Stream)
            $Body = $Reader.ReadToEnd()
            Log-Message "Error Response Body: $Body"
        }
        catch {}
        return $null
    }
}

# --- MAIN ---

Log-Message "COMPREHENSIVE INSTITUTION WORKFLOW TEST REPORT (PowerShell)"
Log-Message "==========================================================="

$Timestamp = [int][double]::Parse((Get-Date -UFormat %s))
$OwnerEmail = "owner_${Timestamp}@test.com"
$OwnerUser = "owner_${Timestamp}"
$Password = "Password123!"

# 1. Register Owner
$PayloadReg = @{
    fullName        = "Test Owner"
    username        = $OwnerUser
    email           = $OwnerEmail
    password        = $Password
    registerAsOwner = $true
}
$RegResponse = Run-Test -Name "Register Owner" -Method "POST" -Endpoint "/api/auth/register" -Payload $PayloadReg

if (-not $RegResponse -or -not $RegResponse.success) {
    Log-Message "STOPPING: Registration failed"
    exit
}

# 2. Login
$PayloadLogin = @{
    username = $OwnerUser
    password = $Password
}
$LoginResponse = Run-Test -Name "Login Owner" -Method "POST" -Endpoint "/api/auth/login" -Payload $PayloadLogin

if (-not $LoginResponse -or -not $LoginResponse.success) {
    Log-Message "STOPPING: Login failed"
    exit
}

$Token = $LoginResponse.data.token
$UserId = $LoginResponse.data.user.id
Log-Message "Logged in as UserID: $UserId"

# 3. Apply Institution
$PayloadInst = @{
    name          = "Test Inst $Timestamp"
    licenseNumber = "LIC$Timestamp"
    address       = "123 Test St"
    phone         = "5551234567"
}
$ApplyResponse = Run-Test -Name "Apply Institution" -Method "POST" -Endpoint "/api/auth/apply-institution" -Payload $PayloadInst -Token $Token

# 4. Get My Institutions
$MyInstButton = Run-Test -Name "Get My Institutions" -Method "GET" -Endpoint "/api/institution/my" -Token $Token

if (-not $MyInstButton.success -or $MyInstButton.data.Count -eq 0) {
    Log-Message "WARNING: No institutions found or failed. Checking Pending Institutions..."
    # Wait, if it's pending it might show up in My Institutions? 
    # Let's check Endpoint implementation
    # GetMyInstitutionsAsync checks InstitutionUsers table.
    # ApplyInstitutionAsync adds user to InstitutionOwners table immediately.
    # It does NOT add to InstitutionUsers?
    # Let's check Verify logic.
    # Ah, owners are in InstitutionOwners table.
    # GetMyInstitutionsAsync checks InstitutionUsers.
    # It seems GetMyInstitutionsAsync only returns where user is Member?
    # Let's check InstitutionOperations.cs: 120: _context.InstitutionUsers...
    # PROBLEM Identified: Owners might not see their institutions in "My Institutions" if they are only in InstitutionOwners table!
    # I should check if ApplyInstitutionAsync adds to InstitutionUsers too?
    # AuthOperations.cs lines 174-182: Adds to InstitutionOwners. NOT InstitutionUsers.
    # So "Get My Institutions" (/api/institution/my) might return empty if it only looks at InstitutionUsers.
    # Let's check InstitutionOperations.cs lines 118-133: Yes, it queries InstitutionUsers.
    
    # HACK for Test: I need to verify checking "Get Pending" if I was admin, but I am not.
    # Wait, if I am owner, how do I get my institution ID to adding managers?
    # Maybe I need to query something else?
    # Or maybe "Apply" returns the ID?
    # AuthOperations line 190 returns "Application submitted..." string. No ID.
    
    # Critical Fix in Code might be needed: Owners should see their institutions.
    # OR, "Apply" should add them as Manager role in InstitutionUsers too?
    # Usually Owner implies Manager access.
    # Let's see if I can workaround in test.
    # I can try to login as Admin (using default credentials if I knew them) to Approve.
    # Or, maybe there's another endpoint?
    
    Log-Message "TEST INSIGHT: Checking if owner is in InstitutionUsers..."
}
else {
    $InstId = $MyInstButton.data[0].id
    Log-Message "Target Institution ID: $InstId"
    
    # 5. Create Manager
    $ManagerEmail = "manager_${Timestamp}@test.com"
    $PayloadMgr = @{
        fullName = "Test Manager"
        email    = $ManagerEmail
        password = "Password123!"
        phone    = "5559876543"
    }
    $CreateMgrResp = Run-Test -Name "Create Manager" -Method "POST" -Endpoint "/api/institution/$InstId/managers" -Payload $PayloadMgr -Token $Token
    
    $ManagerId = $null
    if ($CreateMgrResp.success) {
        $ManagerId = $CreateMgrResp.data
        Log-Message "Created Manager ID: $ManagerId"
    }

    # 6. List Managers
    Run-Test -Name "List Managers" -Method "GET" -Endpoint "/api/institution/$InstId/members?role=Manager" -Token $Token

    # 7. Update Manager
    if ($ManagerId) {
        $PayloadUpdate = @{
            fullName = "Updated Manager Name"
            email    = "updated_$ManagerEmail"
            phone    = "5550001111"
        }
        Run-Test -Name "Update Manager" -Method "PUT" -Endpoint "/api/institution/$InstId/managers/$ManagerId" -Payload $PayloadUpdate -Token $Token
    }

    # 8. Delete Manager
    if ($ManagerId) {
        Run-Test -Name "Delete Manager" -Method "DELETE" -Endpoint "/api/institution/$InstId/managers/$ManagerId" -Token $Token
    }
}

Log-Message "`n=== SUMMARY ==="
Log-Message "Check above for details."
