# =============================================================================
# Non-Binary Base WDAC Policy Setup (STABLE)
# =============================================================================

$PolicyDir       = ".\"
$BasePolicyXml   = "$PolicyDir\BasePolicy.xml"
$BasePolicyCip   = "$PolicyDir\BasePolicy.cip"
$WinTemplateLocal  = "$PolicyDir\BaseTemplateMicrosoftPermissive.xml"

# ENSURE POLICY DIRECTORY EXISTS
if (!(Test-Path $PolicyDir)) { 
    New-Item -Path $PolicyDir -ItemType Directory -Force | Out-Null 
}

Write-Host "Creating NonBinary base WDAC policy..." -ForegroundColor Cyan

Copy-Item -Path $WinTemplateLocal -Destination $BasePolicyXml -Force

# Set Options
Set-RuleOption -FilePath $BasePolicyXml -Option 0    # Force CI checks in Userland
Set-RuleOption -FilePath $BasePolicyXml -Option 3    # Audit Mode
Set-RuleOption -FilePath $BasePolicyXml -Option 6    # Allowing unsigned policies
Set-RuleOption -FilePath $BasePolicyXml -Option 9    # Allow Advanced Boot Options (Safe Mode for e.g.)
Set-RuleOption -FilePath $BasePolicyXml -Option 10   # Allow regression into audit mode on bad driver boot failure
Set-RuleOption -FilePath $BasePolicyXml -Option 12   # Enforce rules on Universal Windows apps
Set-RuleOption -FilePath $BasePolicyXml -Option 15   # Enforce refreshing of code integrity policies on reboot (if option 14 is ever used in a supplimentary policy)
Set-RuleOption -FilePath $BasePolicyXml -Option 16   # Allow updates without need to reboot
Set-RuleOption -FilePath $BasePolicyXml -Option 17   # Allow Supplemental Policies
Set-RuleOption -FilePath $BasePolicyXml -Option 19   # Enforce rules on loaded .dll files and .NET JIT compiled code
Set-RuleOption -FilePath $BasePolicyXml -Option 20   # Enforce expired or revoked certificates as unsigned

Write-Host "Converting to .cip" -ForegroundColor Cyan
ConvertFrom-CIPolicy -XmlFilePath $BasePolicyXml -BinaryFilePath $BasePolicyCip

# TO DO sign the .cip with signing cert

Write-Host "`nSUCCESS: BasePolicy.cip created!" -ForegroundColor Green

# Uncomment below if we want to deploy the .cip from this script
# CiTool --update-policy "$BasePolicyCip" --verbose