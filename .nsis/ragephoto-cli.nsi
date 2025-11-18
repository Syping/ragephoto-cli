######################################################################

!define APP_CREATOR "Syping"
!define APP_EXECUTABLE "ragephoto-cli.exe"
!define APP_NAME "ragephoto-cli"
!define APP_GUID "{8AC8F2D8-DC6B-40BF-A0AF-CF939029B1ED}"
!define APP_VERSION "1.0.0.0"
!define INSTALLER_NAME "${APP_NAME}_setup.exe"
!define INSTALL_TYPE "SetShellVarContext all"
!define REG_APP_PATH "Software\Microsoft\Windows\CurrentVersion\App Paths\${APP_EXECUTABLE}"
!define REG_DESCRIPTION "Open Source RAGE Photo CLI based on libragephoto"
!define REG_ROOT "HKLM"
!define UNINSTALL_PATH "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_GUID}"

######################################################################

VIProductVersion "${APP_VERSION}"
VIAddVersionKey "ProductName" "${APP_NAME}"
VIAddVersionKey "ProductVersion" "${APP_VERSION}"
VIAddVersionKey "CompanyName" "${APP_CREATOR}"
VIAddVersionKey "LegalCopyright" "Copyright © 2025 ${APP_CREATOR}"
VIAddVersionKey "FileDescription" "${REG_DESCRIPTION}"
VIAddVersionKey "FileVersion" "${APP_VERSION}"

######################################################################

!include "x64.nsh"
SetCompressor LZMA
Name "${APP_NAME}"
Caption "${APP_NAME}"
OutFile "${INSTALLER_NAME}"
XPStyle on
Unicode true
ManifestDPIAware true
InstallDirRegKey "${REG_ROOT}" "${REG_APP_PATH}" "Path"
InstallDir "$PROGRAMFILES64\${APP_NAME}"

######################################################################

!include "MUI2.nsh"

!define MUI_ABORTWARNING
!define MUI_UNABORTWARNING

!define MUI_LANGDLL_REGISTRY_ROOT "${REG_ROOT}"
!define MUI_LANGDLL_REGISTRY_KEY "${UNINSTALL_PATH}"
!define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY

!ifdef REG_START_MENU
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "${APP_NAME}"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "${REG_ROOT}"
!define MUI_STARTMENUPAGE_REGISTRY_KEY "${UNINSTALL_PATH}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "${REG_START_MENU}"
!insertmacro MUI_PAGE_STARTMENU Application $SM_Folder
!endif

!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\${APP_EXECUTABLE}"
!define MUI_FINISHPAGE_SHOWREADME
!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_SHOWREADME_TEXT "Create a desktop shortcut"
!define MUI_FINISHPAGE_SHOWREADME_FUNCTION CreateDesktopShortcut
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_RESERVEFILE_LANGDLL

######################################################################

Function .onInit
!insertmacro MUI_LANGDLL_DISPLAY
SetRegView 64
FunctionEnd

######################################################################

Function CreateDesktopShortcut
	CreateShortcut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${APP_EXECUTABLE}"
FunctionEnd

######################################################################

!include "FileFunc.nsh"

Section -UninstallPrevious
ReadRegStr $0 ${REG_ROOT} "${UNINSTALL_PATH}" "UninstallString"
${If} $0 != ""
	${GetParent} $0 $1
	ExecWait '"$0" /S "_?=$1"'
${EndIf}
SectionEnd

######################################################################

Section -MainProgram
${INSTALL_TYPE}
SetOverwrite ifnewer
SetOutPath "$INSTDIR"
!include ".nsis/install.nsh"
SectionEnd

######################################################################

Section -Icons_Reg
SetOutPath "$INSTDIR"
WriteUninstaller "$INSTDIR\uninstall.exe"

!ifdef REG_START_MENU
!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
CreateDirectory "$SMPROGRAMS\$SM_Folder"
CreateShortCut "$SMPROGRAMS\$SM_Folder\${APP_NAME}.lnk" "$INSTDIR\${APP_EXECUTABLE}"
CreateShortCut "$SMPROGRAMS\$SM_Folder\Uninstall ${APP_NAME}.lnk" "$INSTDIR\uninstall.exe"
!insertmacro MUI_STARTMENU_WRITE_END
!endif

!ifndef REG_START_MENU
CreateDirectory "$SMPROGRAMS\${APP_NAME}"
CreateShortCut "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk" "$INSTDIR\${APP_EXECUTABLE}"
CreateShortCut "$SMPROGRAMS\${APP_NAME}\Uninstall ${APP_NAME}.lnk" "$INSTDIR\uninstall.exe"
!endif

WriteRegStr ${REG_ROOT} "${REG_APP_PATH}" "" "$INSTDIR\${APP_EXECUTABLE}"
WriteRegStr ${REG_ROOT} "${REG_APP_PATH}" "Path" "$INSTDIR"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}" "DisplayName" "${APP_NAME}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}" "DisplayVersion" "${APP_VERSION}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}" "Publisher" "${APP_CREATOR}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}" "UninstallString" "$INSTDIR\uninstall.exe"

ExecWait '"$INSTDIR/${APP_EXECUTABLE}" path register'
SectionEnd

Section Uninstall
${INSTALL_TYPE}
ExecWait '"$INSTDIR/${APP_EXECUTABLE}" path unregister'
!include ".nsis/uninstall.nsh"
Delete "$INSTDIR\uninstall.exe"
RmDir "$INSTDIR"

Delete "$DESKTOP\${APP_NAME}.lnk"

!ifdef REG_START_MENU
!insertmacro MUI_STARTMENU_GETFOLDER "Application" $SM_Folder
Delete "$SMPROGRAMS\$SM_Folder\${APP_NAME}.lnk"
Delete "$SMPROGRAMS\$SM_Folder\Uninstall ${APP_NAME}.lnk"
RmDir "$SMPROGRAMS\$SM_Folder"
!endif

!ifndef REG_START_MENU
Delete "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk"
Delete "$SMPROGRAMS\${APP_NAME}\Uninstall ${APP_NAME}.lnk"
RmDir "$SMPROGRAMS\${APP_NAME}"
!endif

DeleteRegKey ${REG_ROOT} "${REG_APP_PATH}"
DeleteRegKey ${REG_ROOT} "${UNINSTALL_PATH}"
SectionEnd

######################################################################

Function un.onInit
!insertmacro MUI_UNGETLANGUAGE
SetRegView 64
FunctionEnd

######################################################################