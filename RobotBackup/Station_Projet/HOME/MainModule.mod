MODULE MainModule
! ================INFOS================
! MainModule
! by IRB120_01
! 06/22/2018
! Generated with HAL 0053
! c Thibault Schwartz 2011-2014
! c HAL Robotics ltd 2015
! http://www.hal-robotics.com
! =====================================


! =============DECLARATIONS============
 VAR speeddata SoftSpeed:=[60,100,5000,1000];
 VAR zonedata Large:=[FALSE,5,7.5,7.5,0.75,7.5,0.75];
 CONST jointtarget TP0:=[[-137.79,56.61,-31.77,0,65.15,-47.79],[0,9E9,9E9,9E9,9E9,9E9]];
 CONST jointtarget TP1:=[[-137.79,56.43,-32.02,0,65.59,-47.79],[0,9E9,9E9,9E9,9E9,9E9]];


! ==============PROCEDURES=============
 PROC Main()
  ConfJ \Off;
  ConfL \Off;
  PathAccLim TRUE \AccMax:=0.1, TRUE \DecelMax:=0.1;
  MoveAbsJ TP0,SoftSpeed,Large,Tool0;
  MoveAbsJ TP1,SoftSpeed,Large,Tool0;
 ENDPROC
ENDMODULE
