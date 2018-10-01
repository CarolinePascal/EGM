MODULE Capteur
    
    CONST jointtarget T0:=[[0,0,0,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST jointtarget T1:=[[30,0,0,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST jointtarget T2:=[[-30,0,0,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST jointtarget T3:=[[0,30,0,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST jointtarget T4:=[[0,-30,0,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST jointtarget T5:=[[0,0,30,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST jointtarget T6:=[[0,0,-30,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    
    VAR speeddata Speed:=[500,1000,5000,1000];
    
    Proc Main()
        ConfL\On;
        ConfJ\On;
        SingArea\Wrist;
        
        MoveAbsJ T0, Speed, z0, tool0;
        MoveAbsJ T1, Speed, z0, tool0;
        MoveAbsJ T2, Speed, z0, tool0;
        MoveAbsJ T0, Speed, z0, tool0;
        WaitTime 1;
        MoveAbsJ T3, Speed, z0, tool0;
        MoveAbsJ T4, Speed, z0, tool0;
        MoveAbsJ T0, Speed, z0, tool0;
        WaitTime 1;
        MoveAbsJ T5, Speed, z0, tool0;
        MoveAbsJ T6, Speed, z0, tool0;
    
    EndProc

ENDMODULE