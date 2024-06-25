!include nsArray.nsh

OutFile nsArray.exe
Name nsArray
ShowInstDetails show
XPStyle on
RequestExecutionLevel user

Page Components
Page InstFiles

Section

  nsArray::Split MyArray |hello|my|name |

  ${nsArray_ToString} MyArray $R0
  DetailPrint $R0

  nsArray::SetList MyArray 11 14 /key=4 meh blah /key=jajaj 9 /end

  ${nsArray_ToString} MyArray $R0
  DetailPrint $R0

  nsArray::Get MyArray 0
  Pop $R0
  DetailPrint `MyArray[0] is $R0`
  nsArray::Get MyArray 1
  Pop $R0
  DetailPrint `MyArray[1] is $R0`
  nsArray::Get MyArray 4
  Pop $R0
  DetailPrint `MyArray[4] is $R0`
  nsArray::Get MyArray 5
  Pop $R0
  DetailPrint `MyArray[5] is $R0`
  nsArray::Get MyArray jajaj
  Pop $R0
  DetailPrint `MyArray[jajaj] is $R0`

  nsArray::Set MyArray /key=jajaj 22
  DetailPrint `Set MyArray[jajaj] = 22`

  nsArray::Get MyArray jajaj
  Pop $R0
  DetailPrint `MyArray[jajaj] is $R0`

  ${nsArray_ToString} MyArray $R0
  DetailPrint $R0

  nsArray::Length MyArray
  Pop $R0
  DetailPrint `MyArray length: $R0`

  nsArray::Set MyArray /at=0 12
  DetailPrint `Set MyArray+0 = 12`

  nsArray::Set MyArray /at=1 13
  DetailPrint `Set MyArray+1 = 13`

  nsArray::Set MyArray /at=7 10
  DetailPrint `Set MyArray+7 = 10`

  ${nsArray_ToString} MyArray $R0
  DetailPrint $R0

  ClearErrors
  nsArray::Get MyArray 2
  ${If} ${Errors}
    DetailPrint `Error!`
  ${Else}
    Pop $R0
  ${EndIf}

  nsArray::SetList MyArray /key=hello 18 12 14 /end

  nsArray::Length MyArray
  Pop $R0
  DetailPrint `MyArray length: $R0`

  ${nsArray_ToString} MyArray $R0
  DetailPrint $R0

  DetailPrint `Remove key "hello", value "hello", value "18" from MyArray`
  ClearErrors
  nsArray::RemoveList MyArray hello /val=hello /val=18 /end
  ${If} ${Errors}
    DetailPrint `Error!`
  ${EndIf}

  ${nsArray_ToString} MyArray $R0
  DetailPrint $R0

  DetailPrint `Remove value "22", value "blah" from MyArray`
  ClearErrors
  nsArray::RemoveList MyArray /val=22 /val=blah /end
  ${If} ${Errors}
    DetailPrint `Error!`
  ${EndIf}

  ${nsArray_ToString} MyArray $R0
  DetailPrint $R0

  DetailPrint `Remove first element from MyArray`
  ClearErrors
  nsArray::Remove MyArray /at=0
  ${If} ${Errors}
    DetailPrint `Error!`
  ${EndIf}

  ${nsArray_ToString} MyArray $R0
  DetailPrint $R0

  nsArray::Length MyArray
  Pop $R0
  DetailPrint `MyArray length: $R0`

  ${nsArray_Copy} MyArray MyArray2

  DetailPrint `Before sorting...`
  StrCpy $R1 0
  ${DoWhile} $R1 < $R0
    nsArray::Get MyArray2 /at=$R1
    Pop $R2
    Pop $R3
    DetailPrint `($R1) MyArray2[$R2] is $R3`
    IntOp $R1 $R1 + 1
  ${Loop}

  nsArray::Sort MyArray2 16

  ${nsArray_ToString} MyArray2 $R0
  DetailPrint `MyArray2 sorted without reordering keys/indices:`
  DetailPrint $R0

  nsArray::Sort MyArray2 2

  ${nsArray_ToString} MyArray2 $R0
  DetailPrint `MyArray2 values sorted numerically:`
  DetailPrint $R0

  nsArray::Sort MyArray2 8

  ${nsArray_ToString} MyArray2 $R0
  DetailPrint `MyArray2 sorted by keys:`
  DetailPrint $R0

  nsArray::SetList MyArray2 -1 `Hi there!` /end

  ${nsArray_ToString} MyArray2 $R0
  DetailPrint $R0

  nsArray::Get MyArray2 0
  ${If} ${Errors}
    DetailPrint `MyArray2[0] is not set`
  ${Else}
    Pop $R0
    DetailPrint `MyArray2[0] is $R0`
  ${EndIf}
  nsArray::Get MyArray2 1
  ${If} ${Errors}
    DetailPrint `MyArray2[1] is not set`
  ${Else}
    Pop $R0
    DetailPrint `MyArray2[1] is $R0`
  ${EndIf}

  ${nsArray_ToString} MyArray2 $R0
  DetailPrint $R0

  nsArray::Length MyArray2
  Pop $R0
  DetailPrint `MyArray2 length: $R0`

  StrCpy $R1 0
  ${DoWhile} $R1 < $R0
    nsArray::Get MyArray2 /at=$R1
    Pop $R2
    Pop $R3
    DetailPrint `($R1) MyArray2[$R2] is $R3`
    IntOp $R1 $R1 + 1
  ${Loop}

  StrCpy $R1 -1
  ${DoUntil} $R1 < -$R0
    nsArray::Get MyArray2 /at=$R1
    Pop $R2
    Pop $R3
    DetailPrint `($R1) MyArray2[$R2] is $R3`
    IntOp $R1 $R1 - 1
  ${Loop}

  ${nsArray_CopyKeys} MyArray2 MyArray2Keys

  ${nsArray_ToString} MyArray2Keys $R0
  DetailPrint $R0

  nsArray::Clear MyArray2

  nsArray::SetList MyArray2 /key=blah value1 value2 /key=2 value3 value4 /end

  ${nsArray_ToString} MyArray2 $R0
  DetailPrint $R0

  ClearErrors
  nsArray::Length MyArray2
  ${IfNot} ${Errors}
    Pop $R0
    DetailPrint `MyArray2 length: $R0`
  ${EndIf}

  nsArray::SetList MyArray2 `` `` /end

  ${ForEachIn} MyArray2 $R0 $R1
   DetailPrint `MyArray2[$R0] => $R1`
  ${Next}

  ${ForEachInReverse} MyArray2 $R0 $R1
   DetailPrint `MyArray2[$R0] => $R1`
  ${Next}

  nsArray::Join MyArray2 | /noempty
  Pop $R0
  DetailPrint $R0

SectionEnd