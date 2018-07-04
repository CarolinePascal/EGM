MODULE EGM_test_UDP

    VAR egmident egmID1;
    VAR egmstate egmSt1;

    CONST egm_minmax egm_minmax_lin1:=[-1,1];
    !in mm
    CONST egm_minmax egm_minmax_rot1:=[-2,2];
    ! in degees
    CONST robtarget p20:=[[400,0,0],[0,-1,0,0],[-1,0,1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];

    PERS tooldata UISpenholder:=[TRUE,[[0,0,114.25],[1,0,0,0]],[1,[-0.095984607,0.082520613,38.69176324],[1,0,0,0],0,0,0]];
    PERS tooldata ToolOfLove:=[TRUE,[[0.015258,-0.149222,106.919],[0.999048,0.043619,0,0]],[1,[0,0,0.1],[1,0,0,0],0,0,0]];

    VAR pose posecorTable:=[[0,0,0],[1,0,0,0]];
    VAR pose posesenTable:=[[0,0,0],[1,0,0,0]];

    VAR speeddata speeddata0:=[500,500,5000,1000];

    VAR intnum routint;

    VAR string msg;

    VAR socketdev udp_socket;
    VAR string IPAdress:="172.29.1.170";
    VAR num Port:=6510;

    VAR clock timer;

    PERS tasks task_list{2} := [["Torque"], ["T_ROB1"]];   !Varibales de stnchronisation des tasks
    VAR syncident sync1;

    PERS bool flag := FALSE;



    !Send torque monitorings to the C# program with reference time for the plot

!    TRAP routine
!        msg:=ValtoStr(ClkRead(timer))+" "+ValtoStr(TestSignRead(1))+" "+
!            ValtoStr(TestSignRead(2))+" "+
!            ValtoStr(TestSignRead(3))+" "+
!            ValtoStr(TestSignRead(4))+" "+
!            ValtoStr(TestSignRead(5))+" "+
!            ValtoStr(TestSignRead(6));

!        SocketSendTo udp_socket,IPAdress,Port\Str:=msg;
!    ENDTRAP


    PROC main()

        ! Move to start position. Fine point is demanded.
        MoveJ p20,speeddata0,fine,ToolOfLove;

        ! Torques on the 6 axis of th robot

!        TestSignDefine 1,4002,ROB_1,1,0.004;
!        TestSignDefine 2,4002,ROB_1,2,0.004;
!        TestSignDefine 3,4002,ROB_1,3,0.004;
!        TestSignDefine 4,4002,ROB_1,4,0.004;
!        TestSignDefine 5,4002,ROB_1,5,0.004;
!        TestSignDefine 6,4002,ROB_1,6,0.004;

        !CONNECT routint WITH routine;
        !ITimer 0.1,routint;

        !SocketCreate udp_socket\UDP;

        testuc_UDP;

    ENDPROC


    PROC testuc_UDP()
        EGMReset egmID1;
        EGMGetId egmID1;
        egmSt1:=EGMGetState(egmID1);
        TPWrite "EGM state: "\Num:=egmSt1;

        WaitSyncTask sync1, task_list;

        IF egmSt1<=EGM_STATE_CONNECTED THEN

            ! Set up the EGM data source: UdpUc server using device "EGMsensor:"and configuration "default"
            EGMSetupUC ROB_1,egmID1,"default","EGMSensor"\pose;

        ENDIF

        !Which program to run
        runEGM;

        TPWrite "Hello";

        IF egmSt1=EGM_STATE_CONNECTED THEN
            TPWrite "Reset EGM instance egmID1";
            EGMReset egmID1;
        ENDIF
    ENDPROC


    PROC runEGM()

        EGMActPose egmID1\Tool:=ToolOfLove\WObj:=wobj0,posecorTable,EGM_FRAME_WOBJ,posesenTable,EGM_FRAME_WOBJ
            \x:=egm_minmax_lin1\y:=egm_minmax_lin1\z:=egm_minmax_lin1
            \rx:=egm_minmax_rot1\ry:=egm_minmax_rot1\rz:=egm_minmax_rot1\LpFilter:=5\Samplerate:=4\MaxSpeedDeviation:=40;

        EGMRunPose egmID1,EGM_STOP_HOLD\x\y\z\CondTime:=20\RampInTime:=0.05\RampOutTime:=0.5\PosCorrGain:=2;

        flag:=FALSE;

        egmSt1:=EGMGetState(egmID1);

    ERROR

        IF ERRNO=ERR_UDPUC_COMM THEN
            !In case the computer stops sending positions
            TPWrite "The computer stoped sending corrdinates";
            flag:=FALSE;
            EXIT;
        ENDIF

    ENDPROC

ENDMODULE
