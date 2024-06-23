!define LOGICLIB_VERBOSITY 4
!include nsArray.nsh

OutFile nsArray.exe
Name `nsArray.nsh Header Test`
ShowInstDetails show
XPStyle on
RequestExecutionLevel user

Page InstFiles

Section

  nsArray::Split MyArray a|b|c|d|e|f |
  ${nsArray_ToString} MyArray $R0
  DetailPrint $R0

  ${nsArray_Copy} MyArray MyArrayCopy
  ${nsArray_ToString} MyArrayCopy $R0
  DetailPrint $R0

  ${nsArray_CopyKeys} MyArray MyArrayKeys
  ${nsArray_ToString} MyArrayKeys $R0
  DetailPrint $R0

  ${ForEachIn} MyArray $R0 $R1
    DetailPrint `MyArray[$R0] => $R1`
  ${Next}

  ${ForEachIn} MyArray99 $R0 $R1
    DetailPrint `MyArray[$R0] => $R1`
  ${Next}

  ${ForEachInReverse} MyArray $R0 $R1
    DetailPrint `MyArray[$R0] => $R1`
  ${Next}

SectionEnd