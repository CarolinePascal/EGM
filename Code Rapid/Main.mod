MODULE EGM_test_UDP

    VAR egmident egmID1;
    VAR egmstate egmSt1;

    CONST egm_minmax egm_minmax_lin1:=[-1,1];   !in mm

    CONST egm_minmax egm_minmax_rot1:=[-2,2];   ! in degees

    !Start position - Fine point required
    CONST robtarget p20:=[[300,0,200],[0,-1,0,0],[0,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];

    PERS tooldata ToolOfLove:=[TRUE,[[0.015258,-0.149222,106.919],[0.999048,0.043619,0,0]],[1,[0,0,0.1],[1,0,0,0],0,0,0]];

    VAR pose posecorTable:=[[0,0,0],[1,0,0,0]];
    VAR pose posesenTable:=[[0,0,0],[1,0,0,0]];

    VAR speeddata speeddata0:=[500,500,5000,1000];  !Only used to reach p20

    PERS bool flag := FALSE;


    PROC main()

        ! Move to start position. Fine point is demanded.
        MoveJ p20,speeddata0,fine,Tool0;

        testuc_UDP;

    ENDPROC


    PROC testuc_UDP()
        EGMReset egmID1;
        EGMGetId egmID1;

        ! Set up the EGM data source: UdpUc server using device "EGMsensor:"and configuration "default"
        EGMSetupUC ROB_1,egmID1,"default","EGMSensor"\Pose;

        !Which program to run
        runEGM;

        EGMStop egmID1;
    ENDPROC


    PROC runEGM()

        EGMActPose egmID1\Tool:=tool0\WObj:=wobj0,tool0.tframe,EGM_FRAME_WORLD,tool0.tframe,EGM_FRAME_WORLD
            \x:=egm_minmax_lin1\y:=egm_minmax_lin1\z:=egm_minmax_lin1
            \rx:=egm_minmax_rot1\ry:=egm_minmax_rot1\rz:=egm_minmax_rot1\LpFilter:=20\Samplerate:=4\MaxSpeedDeviation:=1000;


        EGMRunPose egmID1,EGM_STOP_HOLD,\x\y\z\Rx\Ry\Rz\CondTime:=20\RampInTime:=0.05\RampOutTime:=0.5\PosCorrGain:=5;

        !Concerning the way the robot stops :
        !   - EGM_STOP_HOLD : The robot stays at its position and the motion can be recovered (eventually)
        !   - EGM_STOP_RAMP_DOWN : The robot goes back to its starting point in RampOutTime seconds

    ENDPROC

ENDMODULE
