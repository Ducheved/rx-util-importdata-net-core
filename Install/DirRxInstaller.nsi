;!include MUI2.nsh
LoadLanguageFile "${NSISDIR}\Contrib\Language files\Russian.nlf"
; Переменные.
!define TEMP1 $R0 ;Temp variable
; Наименование инсталятора.
Name "утилита импорта данных CorpTec"
; Директория инсталяции.
InstallDir "$PROGRAMFILES64\Directum Company\DirectumRX\ImportData"
; Наименование инсталятора.
OutFile "Setup_CorpTec.exe"

VIProductVersion 4.0.41139.0
VIAddVersionKey FileVersion 4.0.41139.0
VIAddVersionKey ProductVersion 4.0.41139.0
XPStyle on
; Управление страницами.
Page directory 
Page instfiles

Section "Components"
 SetOutPath $INSTDIR
 File /r "..\src\ImportData\bin\Debug\netcoreapp3.0\*.*" 
SectionEnd

