MODULE Torque


    PERS bool flag;

    VAR string msg;

    VAR socketdev udp_socket;
    VAR string IPAdress:="172.29.1.131";
    VAR num Port:=5000;

    PERS tasks task_list{2} := [["Torque"], ["T_ROB1"]];
    VAR syncident sync1;

    VAR clock timer;
    VAR num time;

    PROC main()

        SocketCreate udp_socket \UDP;

        !Getting the torques values on the 6 axes

        !Also works with :
        !   - Position (4000) in degrees
        !   - Speed (4001) in degrees/second
        !   - Torque (4002) in Nm
        !   - External Torque (4003) in Nm

        TestSignDefine 1,4002,ROB_1,1,0.004;
        TestSignDefine 2,4002,ROB_1,2,0.004;
        TestSignDefine 3,4002,ROB_1,3,0.004;
        TestSignDefine 4,4002,ROB_1,4,0.004;
        TestSignDefine 5,4002,ROB_1,5,0.004;
        TestSignDefine 6,4002,ROB_1,6,0.004;

        flag:=TRUE;

        WaitSyncTask sync1, task_list;

        ! Clock initialisation
        ClkReset timer;
        ClkStart timer;

        WHILE flag DO

            time := ClkRead(timer)*1000;    !Gets time in ms

            msg:=ValToStr(time)+" "+ValtoStr(TestSignRead(1))+" "+
            ValtoStr(TestSignRead(2))+" "+
            ValtoStr(TestSignRead(3))+" "+
            ValtoStr(TestSignRead(4))+" "+
            ValtoStr(TestSignRead(5))+" "+
            ValtoStr(TestSignRead(6));

            SocketSendTo  udp_socket, IPAdress, Port \Str:=msg;
            WaitTime 0.0005;    !buffer

        ENDWHILE

        SocketClose udp_socket;

    ENDPROC


ENDMODULE
