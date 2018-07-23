!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!PREREQUISITE CONFIGURATION!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
! 1) In Configuration>Communication>TransmissionProtocol create a new
!    transmission TransmissionProtocol
! 2) Specify the following entires :
! |    Name   | Type  | Serial Port | Remote Address | Remote port number |
! | EgmSensor | UDPUC |     N/A     |    127.0.0.1   |        6510        |
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


MODULE Monitoring

      VAR egmident egmID;

      !robtargets declaration

      !speeddata declaration

      !tooldata declaration

      !wobjdata declaration

      !zonedata declaration

      PROC Main()

            ConfL\On;
            ConfJ\On;
            SignArea\Wrist;

            EGMReset egmID;
            EGMGetId egmID;

            EGMSetupUC ROB_1,egmID,"default","EgmSensor"

            EGMStreamStart egmID;

            !Motion procedure

            EGMStreamStop egmID;

      ENDPROC

ENDMODULE
