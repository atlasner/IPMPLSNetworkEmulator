@echo off
 
IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MsBuild\Current\Bin\MSBuild.exe" (
    set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MsBuild\Current\Bin\MSBuild.exe"
) ELSE IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MsBuild\Current\Bin\MSBuild.exe" (
    set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MsBuild\Current\Bin\MSBuild.exe"
) ELSE IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MsBuild\Current\Bin\MSBuild.exe" (
    set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MsBuild\Current\Bin\MSBuild.exe"
) ELSE IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MsBuild\15.0\Bin\MSBuild.exe" (
    set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MsBuild\15.0\Bin\MSBuild.exe"
) ELSE IF EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MsBuild\15.0\Bin\MSBuild.exe" (
    set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MsBuild\15.0\Bin\MSBuild.exe"
) ELSE IF EXIST "D:\VisualStudio2019\MSBuild\Current\Bin\MSBuild.exe" (
    set msbuild="D:\VisualStudio2019\MSBuild\Current\Bin\MSBuild.exe"
) ELSE (
    echo MSBuild.exe not found. Check if you have it in C:\Program Files x86\Microsoft Visual Studio\201x\xxxxxxxxx\MsBuild\xxxx\Bin\MSBuild.exe
    pause
    EXIT /b -1
)
 
SET SOL= "TSSSSST.sln"
 
FOR %%i IN (%SOL%) DO (
    IF NOT EXIST %%i (
        echo Fatal error. %%i does not exist.
        pause
        EXIT /b -1
    )
)
 
FOR %%i IN (%SOL%) DO (
     %msbuild% -restore -m %%i
    IF errorlevel 0 (
        echo Build success %%i
    ) ELSE (
        echo Build failed %%i
        pause
        EXIT /b -1
    )
)
 
 
START .\TSST.CableCloud\bin\Debug\TSST.CableCloud.exe "./configs/CableCloudConfig.txt"
REM START .\TSST.ManagementModule\bin\Debug\TSST.ManagementModule.exe "./configs/ManagementConfig.txt"
timeout 1

START .\TSST.NCC\bin\Debug\TSST.NCC.exe "./configs/NCC1.txt"
START .\TSST.NCC\bin\Debug\TSST.NCC.exe "./configs/NCC2.txt"
timeout 1
START .\TSST.Host\bin\Debug\TSST.Host.exe "./configs/H1.txt"
START .\TSST.Host\bin\Debug\TSST.Host.exe "./configs/H2.txt"
START .\TSST.Host\bin\Debug\TSST.Host.exe "./configs/H3.txt"
START .\TSST.Host\bin\Debug\TSST.Host.exe "./configs/H4.txt"
timeout 1
START .\TSST.Subnetwork\bin\Debug\TSST.Subnetwork.exe "./configs/Domain1.txt"
START .\TSST.Subnetwork\bin\Debug\TSST.Subnetwork.exe "./configs/Subnetwork1.txt"
timeout 1
START .\TSST.Subnetwork\bin\Debug\TSST.Subnetwork.exe "./configs/Domain2.txt"
START .\TSST.Subnetwork\bin\Debug\TSST.Subnetwork.exe "./configs/Subnetwork2.txt"
timeout 1
START .\TSST.NetworkNode\bin\Debug\TSST.NetworkNode.exe "./configs/R1.txt"
START .\TSST.NetworkNode\bin\Debug\TSST.NetworkNode.exe "./configs/R2.txt"
START .\TSST.NetworkNode\bin\Debug\TSST.NetworkNode.exe "./configs/R3.txt"
START .\TSST.NetworkNode\bin\Debug\TSST.NetworkNode.exe "./configs/R4.txt"
START .\TSST.NetworkNode\bin\Debug\TSST.NetworkNode.exe "./configs/R5.txt"
START .\TSST.NetworkNode\bin\Debug\TSST.NetworkNode.exe "./configs/R6.txt"
START .\TSST.NetworkNode\bin\Debug\TSST.NetworkNode.exe "./configs/R7.txt"
START .\TSST.NetworkNode\bin\Debug\TSST.NetworkNode.exe "./configs/R8.txt"
START .\TSST.NetworkNode\bin\Debug\TSST.NetworkNode.exe "./configs/R9.txt"

timeout 1
start ./cmdow.exe R1 /mov 0 0
start ./cmdow.exe R2 /mov 100 0
start ./cmdow.exe R3 /mov 200 0
start ./cmdow.exe R4 /mov 300 0
start ./cmdow.exe R5 /mov 400 0
start ./cmdow.exe R6 /mov 500 0
start ./cmdow.exe R7 /mov 600 0
start ./cmdow.exe R8 /mov 700 0
start ./cmdow.exe R9 /mov 800 0
start ./cmdow.exe H1 /mov 0 400
start ./cmdow.exe H3 /mov 0 400
start ./cmdow.exe H2 /mov 1050 400
start ./cmdow.exe H4 /mov 1050 400
start ./cmdow.exe CableCloud /mov 0 0
start ./cmdow.exe NCC1 /mov 0 0
start ./cmdow.exe NCC2 /mov 1050 0
start ./cmdow.exe CC1 /mov 200 0
start ./cmdow.exe CC2 /mov 700 0
start ./cmdow.exe RC1 /mov 220 0
start ./cmdow.exe RC2 /mov 720 0
start ./cmdow.exe CC11 /mov 200 400
start ./cmdow.exe CC21 /mov 700 400
start ./cmdow.exe RC11 /mov 220 400
start ./cmdow.exe RC21 /mov 720 400
