$debugVersion = ""
$version = ""

$hasChanges = $false
$diff = git diff-index HEAD
if ($diff) {
    $hasChanges = $true
}

$currentBranch = git rev-parse --abbrev-ref HEAD
$commitsCount = git rev-list --count HEAD

if (!$hasChanges -and ($currentBranch -eq "master")) {
    $versionString = Get-Content "version.txt"
    $versionSegments = $versionString.Split("{.}")
    if ($versionSegments.Length -ne 2)
    {
        Throw "���� version.txt ������ ��������� ����� ������ � ������� X.Y"
    }

    $debugVersion = "0.0.$commitsCount.*"
    $version = "$($versionSegments[0]).$($versionSegments[1]).$commitsCount"
}
else {
    $debugVersion = "0.0.$commitsCount.*"
    $version = $debugVersion
}

$versionInfo = @"
/* 
    ���� ���� ������������ ������������� ��� ������ �������.
    ����� ������ ������� � "version.txt" � ������� X.Y
   
    ������ ��� ����� ������������� �� ������, ������ ���� ��������� �������:
    * � ������� ����� ����������� ����������������� ���������
    * ������ ������������ �� ����� master
    * ������ ������������ � ������������ Release
*/

using System.Reflection;

#if DEBUG
[assembly: AssemblyVersion("$debugVersion")]
#else
[assembly: AssemblyVersion("$version")]
#endif
"@

Set-Content -Path "AssemblyVersion.cs" -Value $versionInfo