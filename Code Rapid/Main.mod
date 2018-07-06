MODULE EGM_test_UDP

    VAR egmident egmID1;
    VAR egmstate egmSt1;

    CONST egm_minmax egm_minmax_lin1:=[-1,1];   !in mm

    CONST egm_minmax egm_minmax_rot1:=[-2,2];   ! in degees

    !Start position - Fine point required
    CONST robtarget p20:=[[400,-300,100],[0,-1,0,0],[-1,0,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];

    PERS tooldata ToolOfLove:=[TRUE,[[0.015258,-0.149222,106.919],[0.999048,0.043619,0,0]],[1,[0,0,0.1],[1,0,0,0],0,0,0]];

    VAR pose posecorTable:=[[0,0,0],[1,0,0,0]];
    VAR pose posesenTable:=[[0,0,0],[1,0,0,0]];

    VAR speeddata speeddata0:=[500,500,5000,1000];  !Only used to reach p20

    PERS tasks task_list{2} := [["Torque"], ["T_ROB1"]];
    VAR syncident sync1;

    PERS bool flag := FALSE;


    PROC main()

        ! Move to start position. Fine point is demanded.
        MoveJ p20,speeddata0,fine,ToolOfLove;

        testuc_UDP;

    ENDPROC


    PROC testuc_UDP()
        EGMReset egmID1;
        EGMGetId egmID1;

        IF egmSt1<=EGM_STATE_CONNECTED THEN

            ! Set up the EGM data source: UdpUc server using device "EGMsensor:"and configuration "default"
            EGMSetupUC ROB_1,egmID1,"default","EGMSensor"\pose;

        ENDIF

        !Which program to run
        runEGM;

        IF egmSt1=EGM_STATE_CONNECTED THEN
            TPWrite "Reset EGM instance egmID1";
            EGMReset egmID1;
        ENDIF

    ENDPROC


    PROC runEGM()

        EGMActPose egmID1\Tool:=ToolOfLove\WObj:=wobj0,posecorTable,EGM_FRAME_WOBJ,posesenTable,EGM_FRAME_WOBJ
            \x:=egm_minmax_lin1\y:=egm_minmax_lin1\z:=egm_minmax_lin1
            \rx:=egm_minmax_rot1\ry:=egm_minmax_rot1\rz:=egm_minmax_rot1\LpFilter:=2\Samplerate:=4\MaxSpeedDeviation:=40;

        WaitSyncTask sync1, task_list;

        EGMRunPose egmID1,EGM_STOP_HOLD,\x\y\z\CondTime:=20\RampInTime:=0.05\RampOutTime:=0.5\PosCorrGain:=1;

        !Concerning the way the robot stops :
        !   - EGM_STOP_HOLD : The robot stays at its position and the motion can be recovered (eventually)
        !   - EGM_STOP_RAMP_DOWN : The robot goes back to its starting point in RampOutTime seconds


    ERROR

        IF ERRNO=ERR_UDPUC_COMM THEN
            !In case the computer stops sending positions
            TPWrite "The computer stopped sending corrdinates";
            flag:=FALSE;
            EXIT;
        ENDIF

    ENDPROC

ENDMODULE
