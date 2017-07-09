cd "%~dp0"
REM cd "c:\temp"

if exist .\DbFolder rmdir /s /q DbFolder
if exist .\UploadFolder rmdir /s /q UploadFolder

mkDir DbFolder
cd DbFolder
mkDir Folder1
echo This is file 1 > File1.txt
cd Folder1
echo This is File 2 > File2.txt
mkDir Folder2
cd Folder2
echo This is File 3 > File3.txt

cd ..\..\..
xcopy /i /e /y /q DbFolder UploadFolder

.\touch /scm /d "01-01-2017" /t "12:00:00.00" DbFolder
.\touch /scm /d "01-01-2017" /t "12:00:00.00" UploadFolder