MODULE GripperFunctions(SYSMODULE, NOSTEPIN)
 PERS tooldata EGP_40_N_N_B:=[TRUE,[[-0.025,0.1688,178.237],[1,0,0,0]],[1,[0,0,87.6686],[1,0,0,0],0,0,0]];
 VAR robtarget positionOffset;
 VAR speeddata transport;
 VAR speeddata place;
 PERS tooldata gripper;
     !======
     PROC StartGripper()
         Set DO_01;
         WaitTime 0.5;
     ENDPROC
     PROC ShutdownGripper()
         Reset DO_01;
         WaitTime 0.5;
     ENDPROC
     PROC OpenGripper()
         Reset DO_03;
         Set DO_02;
         WaitTime 0.5;
     ENDPROC
     PROC CloseGripper()
         Reset DO_02;
         Set DO_03;
         WaitTime 0.5;
     ENDPROC
     PROC TakeBloc(robtarget posOffset, robtarget position, tooldata toolsetting,speeddata speedTransport,speeddata speedPlace)
         gripper:=toolsetting;
         transport:=speedTransport;
 place:=speedPlace;
         positionOffset:=posOffset;
         MoveL positionOffset,transport,fine,gripper;
         OpenGripper;
         MoveL position,place,fine,gripper;
         CloseGripper;
         MoveL positionOffset,place,fine,gripper;
     ENDPROC
     PROC ReleaseBloc(robtarget posOffset, robtarget position, tooldata toolsetting,speeddata speedTransport, speeddata speedPlace)
         gripper:=toolsetting;
       transport:=speedTransport;
 place:=speedPlace;
         positionOffset:=posOffset;
          MoveL positionOffset,transport,fine,gripper;
         MoveL position,place,fine,gripper;
         OpenGripper;
         MoveL positionOffset,transport,fine,gripper;
     ENDPROC
ENDMODULE
