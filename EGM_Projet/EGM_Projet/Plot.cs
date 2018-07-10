using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace EGM_Projet
{
    public class Plot
    {
        /// <summary>
        /// .py file path to write in
        /// </summary>
        private string filePath = "C:/Users/carol/Desktop/Stage_1/plot.py";

        /// <summary>
        /// python.exe file path
        /// </summary>
        private string python = "C:/Users/carol/appdata/local/programs/python/python36-32/python.exe";

        private StringBuilder text;

        /// <summary>
        /// Plot class constructor - Creates the StringBuilder text and initializes the header of the python script
        /// </summary>
        public Plot()
        {
            text = new StringBuilder();

            text.AppendLine("#Plot Python ");
            text.AppendLine("import matplotlib.pyplot as plt");
            text.AppendLine("from mpl_toolkits.mplot3d import Axes3D");

            text.AppendLine(" ");
            text.AppendLine("X=[]");
            text.AppendLine("Y=[]");
            text.AppendLine("Z=[]");
            text.AppendLine("TEGM=[]");
            text.AppendLine(" ");
            text.AppendLine("T1=[]");
            text.AppendLine("T2=[]");
            text.AppendLine("T3=[]");
            text.AppendLine("T4=[]");
            text.AppendLine("T5=[]");
            text.AppendLine("T6=[]");
            text.AppendLine("T=[]");
            text.AppendLine(" ");
        }

        /// <summary>
        /// Writes the recorded positions values in text
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="z">z position</param>
        /// <param name="t">time from the EGM message header</param>
        public void Fill(string x, string y, string z, string t)
        {
            var newLine = string.Format("X.append({0})", x);
            text.AppendLine(newLine);
            newLine = string.Format("Y.append({0})", y);
            text.AppendLine(newLine);
            newLine = string.Format("Z.append({0})", z);
            text.AppendLine(newLine);
            newLine = string.Format("TEGM.append({0})", t);
            text.AppendLine(newLine);
        }

        /// <summary>
        /// Writes the recorded torques values in text
        /// </summary>
        /// <param name="t1">axis 1 torque</param>
        /// <param name="t2">axis 2 torque</param>
        /// <param name="t3">axis 3 torque</param>
        /// <param name="t4">axis 4 torque</param>
        /// <param name="t5">axis 5 torque</param>
        /// <param name="t6">axis 6 torque</param>
        /// <param name="t">time from the recieved message</param>
        public void FillTorque(string t1, string t2, string t3, string t4, string t5, string t6, string t)
        {
            var newLine = string.Format("T1.append({0})", t1);
            text.AppendLine(newLine);
            newLine = string.Format("T2.append({0})", t2);
            text.AppendLine(newLine);
            newLine = string.Format("T3.append({0})", t3);
            text.AppendLine(newLine);
            newLine = string.Format("T4.append({0})", t4);
            text.AppendLine(newLine);
            newLine = string.Format("T5.append({0})", t5);
            text.AppendLine(newLine);
            newLine = string.Format("T6.append({0})", t6);
            text.AppendLine(newLine);
            newLine = string.Format("T.append({0})", t);
            text.AppendLine(newLine);
        }

        /// <summary>
        /// Writes the plotting instructions in the python script and executes it
        /// </summary>
        public void Trace()
        {
            text.AppendLine(" ");
            text.AppendLine("fig=plt.figure()");
            text.AppendLine("ax=fig.gca(projection='3d')");

            text.AppendLine("ax.plot(X,Y,Z)");

            text.AppendLine("ax.set_xlabel('X')");
            text.AppendLine("ax.set_ylabel('Y')");
            text.AppendLine("ax.set_zlabel('Z')");

            text.AppendLine("plt.axis('equal')");
            text.AppendLine("plt.legend(loc='best')");

            text.AppendLine(" ");

            text.AppendLine("fig2=plt.figure()");

            text.AppendLine("plt.plot(TEGM,X,label='x')");
            text.AppendLine("plt.plot(TEGM,Y,label='y')");
            text.AppendLine("plt.plot(TEGM,Z,label='z')");

            text.AppendLine("plt.xlabel('Temps en ms')");
            text.AppendLine("plt.ylabel('Axes')");

            text.AppendLine("plt.legend(loc='best')");

            text.AppendLine(" ");

            text.AppendLine("fig3=plt.figure()");

            text.AppendLine("plt.plot(T,T1,'--',label='Axe 1')");
            text.AppendLine("plt.plot(T,T2,'--',label='Axe 2')");
            text.AppendLine("plt.plot(T,T3,'--',label='Axe 3')");
            text.AppendLine("plt.plot(T,T4,'--',label='Axe 4')");
            text.AppendLine("plt.plot(T,T5,'--',label='Axe 5')");
            text.AppendLine("plt.plot(T,T6,'--',label='Axe 6')");

            text.AppendLine("plt.xlabel('Temps en ms')");
            text.AppendLine("plt.ylabel('Couples en Nm')");

            text.AppendLine("plt.legend(loc='best')");

            text.AppendLine(" ");

            text.AppendLine("if(T==TEGM):");  //Synchronisation condition

            text.AppendLine("   fig4=plt.figure()");
            text.AppendLine("   plt.plot(Y,T1,label='Axe 1')");

            text.AppendLine("   plt.xlabel('Déplacement en Y en mm')");
            text.AppendLine("   plt.ylabel('Couple axe 1 en Nm')");

            text.AppendLine("   plt.legend(loc='best')");

            text.AppendLine("plt.show()");

            File.WriteAllText(filePath, text.ToString());

            ProcessStartInfo start = new ProcessStartInfo();
            string cmd = python;
            string args = filePath;

            start.FileName = cmd;
            start.Arguments = args;
            start.UseShellExecute = false;// Do not use OS shell
            start.CreateNoWindow = true; // We don't need new window
            start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
            start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)

            Process process = Process.Start(start);
        }

        /// <summary>
        /// Clears the StringBuilder text
        /// </summary>
        public void Clear()
        {
            text.Clear();
        }
    }
}
