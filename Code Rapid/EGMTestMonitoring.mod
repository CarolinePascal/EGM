Module MainModule
    !Declarations
    VAR egmident egmID1;

    !robtarget
    CONST robtarget robtarget0:=[[428.877017,271.324299,356.367929],[0,1,0,0],[0,0,2,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget1:=[[349.970776,235.556832,356.367929],[0,1,0,0],[0,0,2,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget2:=[[282.557647,182.141059,356.367929],[0,1,0,0],[0,-1,2,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget3:=[[286.672442,102.193587,356.367929],[0,1,0,0],[0,-1,2,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget4:=[[350.717198,44.212286,356.367929],[0,1,0,0],[0,0,2,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget5:=[[421.182897,-6.292088,356.367929],[0,1,0,0],[-1,0,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget6:=[[485.872813,-63.646317,356.367929],[0,1,0,0],[-1,-1,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget7:=[[489.58536,-142.82468,356.367929],[0,1,0,0],[-1,-1,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget8:=[[425.743713,-201.076409,356.367929],[0,1,0,0],[-1,-1,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget9:=[[355.840258,-252.355202,356.367929],[0,1,0,0],[-1,-1,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget robtarget10:=[[294.540636,-312.799363,356.367929],[0,1,0,0],[-1,0,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];

    !speeddata
    VAR speeddata speeddata0:=[250,572.957795,5000,1000];

    !tooldata
    PERS tooldata Tool:=[True,[[0,0,50],[1,0,0,0]],[0.001,[0,0,0.1],[1,0,0,0],0,0,0]];

    !wobjdata
    PERS wobjdata Root:=[False,True,"",[[0,0,0],[1,0,0,0]],[[0,0,0],[1,0,0,0]]];

    !zonedata
    VAR zonedata zonedata0:=[False,1,5,1,5,1,5];

    Proc Main()
        ConfL\On;
        ConfJ\On;
        SingArea\Wrist;

        EGMReset egmID1;
        EGMGetId egmID1;
        EGMSetupUC ROB_1,egmID1,"default","EGMSensor";
        EGMStreamStart egmID1;
        MoveL robtarget0,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget1,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget2,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget3,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget4,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget5,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget6,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget7,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget8,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget9,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget10,speeddata0,zonedata0,Tool\WObj:=Root;
        WaitTime 2;
        MoveL robtarget0,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget1,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget2,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget3,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget4,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget5,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget6,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget7,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget8,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget9,speeddata0,zonedata0,Tool\WObj:=Root;
        MoveL robtarget10,speeddata0,zonedata0,Tool\WObj:=Root;

        EGMStreamStop egmID1;

    EndProc
EndModule