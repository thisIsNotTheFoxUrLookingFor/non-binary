# =============================================================================
# Non-Binary Base WDAC Policy Setup (STABLE)
# =============================================================================

$PolicyDir       = "C:\Program Files\NonBinary\Policy"
$InstallPath     = "C:\Program Files\NonBinary"
$BasePolicyXml   = "$PolicyDir\BasePolicy.xml"
$BasePolicyCip   = "$PolicyDir\BasePolicy.cip"
$PolicyName      = "NonBinary-BasePolicy"

# The official MS template path
$WinTemplateSource = "C:\Windows\schemas\CodeIntegrity\ExamplePolicies\DefaultWindows_Audit.xml"
$WinTemplateLocal  = "$PolicyDir\DefaultWindows_Audit.xml"

# 1. ENSURE DIRECTORY EXISTS
if (!(Test-Path $PolicyDir)) { 
    New-Item -Path $PolicyDir -ItemType Directory -Force | Out-Null 
}

# 2. COPY TEMPLATE LOCALLY (Fixes Access Denied error)
Write-Host "Copying Windows Template..." -ForegroundColor Yellow
Copy-Item -Path $WinTemplateSource -Destination $WinTemplateLocal -Force

Write-Host "Creating NonBinary base WDAC policy..." -ForegroundColor Cyan

# 3. & 4. SIMPLIFIED: Skip the full scan, just create the path rule
Write-Host "Creating allow rule for $InstallPath..." -ForegroundColor Yellow

# This creates a single rule that allows everything in your agent folder
$PathRule = New-CIPolicyRule -FilePathRule "$InstallPath\*"

# 5. MERGE: Merge the MS Template with ONLY your custom path rule
Write-Host "Merging Microsoft Template with NonBinary Path Rule..." -ForegroundColor Yellow
Merge-CIPolicy -OutputFilePath $BasePolicyXml -PolicyPaths $WinTemplateLocal -Rules $PathRule

# 6. Set Options
Set-RuleOption -FilePath $BasePolicyXml -Option 3    # Audit Mode
Set-RuleOption -FilePath $BasePolicyXml -Option 14   # Block Store Apps Unless Specified
Set-RuleOption -FilePath $BasePolicyXml -Option 16   # UpdatePolicyNoReboot
Set-RuleOption -FilePath $BasePolicyXml -Option 17   # Allow Supplemental Policies

# 7. SET FRIENDLY NAME & VERSION (The "Total" String Method)
$content = Get-Content $BasePolicyXml -Raw

# Replace the Main Name tag
$content = $content -replace '(?<=<PolicyName>)(.*?)(?=</PolicyName>)', $PolicyName

# Replace the Template name in the Settings block (This is what CiTool usually shows)
$content = $content -replace '(?<=<String>)(DefaultWindowsAudit)(?=</String>)', $PolicyName

# Replace Version
$content = $content -replace '(?<=<VersionEx>)(.*?)(?=</VersionEx>)', "1.0.0.0"

$content | Set-Content $BasePolicyXml -Force

Write-Host "Converting to .cip and deploying..." -ForegroundColor Cyan
ConvertFrom-CIPolicy -XmlFilePath $BasePolicyXml -BinaryFilePath $BasePolicyCip

# 8. Deploy
CiTool --update-policy "$BasePolicyCip" --verbose

Write-Host "`n✅ SUCCESS: $PolicyName deployed in AUDIT MODE!" -ForegroundColor Green