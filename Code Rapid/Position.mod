MODULE Position

  VAR egmident egmID1;
  VAR egmstate egmSt1;

  CONST egm_minmax egm_minmax_lin1:=[-1,1];
  !in mm

  !Start position - Fine point
  CONST jointtarget j00:=[[0,0,0,0,0,0],[9E9,9E9,9E9,9E9,9E9,9E9]];

  VAR speeddata speeddata0:=[1000,1000,5000,1000];


  PROC main()

      ! Move to start position. Fine point is demanded.
      MoveAbsJ j00,speeddata0,fine,Tool0;

      testuc_UDP;
     

  ENDPROC


  PROC testuc_UDP()
      EGMReset egmID1;
      EGMGetId egmID1;

      ! Set up the EGM data source: UdpUc server using device "EGMsensor:"and configuration "default"
      EGMSetupUC ROB_1,egmID1,"default","EGMSensor"\Pose,\commTimeout:=10;

      !Which program to run
      runEGM;

      EGMStop egmID1,EGM_STOP_RAMP_DOWN;
  ENDPROC


  PROC runEGM()

      EGMActPose egmID1,\Tool:=tool0,\WObj:=wobj0,tool0.tframe,EGM_FRAME_BASE, tool0.tframe, EGM_FRAME_BASE, \x:=egm_minmax_lin1\y:=egm_minmax_lin1\z:=egm_minmax_lin1,\LpFilter:=4\Samplerate:=4\MaxSpeedDeviation:=1000;

      EGMRunPose egmID1,EGM_STOP_HOLD,\x\y\z\CondTime:=20\RampInTime:=0.05\RampOutTime:=0.5\PosCorrGain:=1;

      !Concerning the way the robot stops :
      !   - EGM_STOP_HOLD : The robot stays at its position and the motion can be recovered (eventually)
      !   - EGM_STOP_RAMP_DOWN : The robot goes back to its starting point in RampOutTime seconds

  ENDPROC

ENDMODULE