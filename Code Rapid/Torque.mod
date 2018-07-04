MODULE Torque


    PERS bool flag;

    VAR string msg:="Hello";

    VAR socketdev udp_socket;
    VAR string IPAdress:="172.29.2.47";
    VAR num Port:=5000;

    PERS tasks task_list{2} := [["Torque"], ["T_ROB1"]];   !Varibales de stnchronisation des tasks
    VAR syncident sync1;

    VAR clock timer;
    VAR num time;
    VAR num tronc := 0;

    VAR string FilePath := "C:/Users/carol/Desktop/Stage_1/Solution_EGM/Stations";

    PROC main()

        SocketCreate udp_socket \UDP;

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

            time := ClkRead(timer)*1000;
            tronc := Trunc(time);

            msg:=ValToStr(time)+" "+ValtoStr(TestSignRead(1))+" "+
            ValtoStr(TestSignRead(2))+" "+
            ValtoStr(TestSignRead(3))+" "+
            ValtoStr(TestSignRead(4))+" "+
            ValtoStr(TestSignRead(5))+" "+
            ValtoStr(TestSignRead(6));

            SocketSendTo  udp_socket, IPAdress, Port \Str:=msg;
            WaitTime 0.0005;

        ENDWHILE

        SocketClose udp_socket;

    ENDPROC


ENDMODULE
