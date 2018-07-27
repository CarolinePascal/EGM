MODULE Path_Correction

    VAR egmident egmID;
    VAR egmstate egmSt;

    !Start position - Fine point required
    CONST robtarget p1:=[[400,-200,0],[0,-1,0,0],[-1,0,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    CONST robtarget p2:=[[400,200,0],[0,-1,0,0],[-1,0,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];

    PERS tooldata Tool:=[TRUE,[[0.015258,-0.149222,106.919],[0.999048,0.043619,0,0]],[1,[0,0,0.1],[1,0,0,0],0,0,0]]

    VAR speeddata speed_data:=[500,500,5000,1000];

    PROC main()

        ! Move to start position. Fine point is demanded.
        MoveJ p1,speed_data,fine,Tool;

        testuc_UDP;

    ENDPROC


    PROC testuc_UDP()

        EGMReset egmID;
        EGMGetId egmID;

        IF egmSt<=EGM_STATE_CONNECTED THEN

            ! Set up the EGM data source: UdpUc server using device "EGMsensor" and configuration "pathCorr" which must be created in the External Motion Interface Data
            EGMSetupUC ROB_1,egmID,"pathCorr","EGMSensor"\PathCorr\APTR;

        ENDIF

        EGMActMove egmID,Tool.tframe\SampleRate:=24;

        runToolPath;

        IF egmSt=EGM_STATE_CONNECTED THEN
            TPWrite "Reset EGM instance egmID1";
            EGMReset egmID;
        ENDIF

    ENDPROC


    PROC runToolPath()

        EGMMoveL egmID, p2, speed_data, z5, Tool\WObj:=wobj0;

    ENDPROC


ENDMODULE
