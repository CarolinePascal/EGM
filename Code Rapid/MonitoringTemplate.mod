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

            EGMSetupUC ROB_1,egmID,"default","EGMSensor"

            EGMStreamStart egmID;

            !Motion procedure

            EGMStreamStop egmID;

      ENDPROC

ENDMODULE
