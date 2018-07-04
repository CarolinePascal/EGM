MODULE Torque


    PERS bool flag;

    VAR string msg:="Hello";

    VAR socketdev udp_socket;
    VAR string IPAdress:="172.29.1.170";
    VAR num Port:=6510;

    PERS tasks task_list{2} := [["Torque"], ["T_ROB1"]];   !Varibales de stnchronisation des tasks
    VAR syncident sync1;

    VAR clock timer;

    PROC main()

        SocketCreate udp_socket\UDP;

        TestSignDefine 1,4002,ROB_1,1,0.004;
        TestSignDefine 2,4002,ROB_1,2,0.004;
        TestSignDefine 3,4002,ROB_1,3,0.004;
        TestSignDefine 4,4002,ROB_1,4,0.004;
        TestSignDefine 5,4002,ROB_1,5,0.004;
        TestSignDefine 6,4002,ROB_1,6,0.004;

        ! Clock initialisation
        ClkReset timer;
        ClkStart timer;

        flag:=TRUE;

        WaitSyncTask sync1, task_list;

        WHILE flag DO

            msg:=ValtoStr(ClkRead(timer))+" "+ValtoStr(TestSignRead(1))+" "+
            ValtoStr(TestSignRead(2))+" "+
            ValtoStr(TestSignRead(3))+" "+
            ValtoStr(TestSignRead(4))+" "+
            ValtoStr(TestSignRead(5))+" "+
            ValtoStr(TestSignRead(6));

            SocketSendTo udp_socket,IPAdress,Port\Str:=msg;
            WaitTime 0.004;

        ENDWHILE

        SocketClose udp_socket;

    ENDPROC


ENDMODULE
