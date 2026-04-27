$password = Read-Host "Enter PFX password" -AsSecureString
$plainPass = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))
cd ..\NonBinary.WindowsAgent
dotnet publish -c Release -r win-arm64 --self-contained true /p:PublishSingleFile=true /p:PfxPassword=$plainPass
dotnet publish -c Release -r win-x64   --self-contained true /p:PublishSingleFile=true /p:PfxPassword=$plainPass
cd ..\Assets
Write-Host "Both architectures signed and published" -ForegroundColor Green