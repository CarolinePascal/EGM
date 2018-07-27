using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace EGMProjet
{
    public class Plot
    {
        /// <summary>
        /// .py file path to write in
        /// </summary>
        private string _filePath = "C:/Users/carol/Desktop/Stage_1/plot.py";

        /// <summary>
        /// python.exe file path
        /// </summary>
        private string _pythonPath = "C:/Users/carol/appdata/local/programs/python/python36-32/python.exe";

        private StringBuilder _text;

        /// <summary>
        /// Plot class constructor - Creates the StringBuilder _text and initializes the header of the python script
        /// </summary>
        public Plot()
        {
            _text = new StringBuilder();

            _text.AppendLine("#Plot Python ");
            _text.AppendLine("import matplotlib.pyplot as plt");
            _text.AppendLine("from mpl_toolkits.mplot3d import Axes3D");

            _text.AppendLine(" ");
            _text.AppendLine("X=[]");
            _text.AppendLine("Y=[]");
            _text.AppendLine("Z=[]");
            _text.AppendLine("Psi=[]");
            _text.AppendLine("Theta=[]");
            _text.AppendLine("Phi=[]");
            _text.AppendLine("TEGM=[]");
            _text.AppendLine(" ");
            _text.AppendLine(" ");
            _text.AppendLine("Xc=[]");
            _text.AppendLine("Yc=[]");
            _text.AppendLine("Zc=[]");
            _text.AppendLine("Psic=[]");
            _text.AppendLine("Thetac=[]");
            _text.AppendLine("Phic=[]");
            _text.AppendLine(" ");
            //_text.AppendLine("T1=[]");
            //_text.AppendLine("T2=[]");
            //_text.AppendLine("T3=[]");
            //_text.AppendLine("T4=[]");
            //_text.AppendLine("T5=[]");
            //_text.AppendLine("T6=[]");
            //_text.AppendLine("T=[]");
            //_text.AppendLine(" ");
        }

        /// <summary>
        /// Writes the recorded positions values in _text
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="z">z position</param>
        /// <param name="t">time from the EGM message header</param>
        public void Fill(string x, string y, string z, string t, string psi, string theta, string phi)
        {
            var newLine = string.Format("X.append({0})", x);
            _text.AppendLine(newLine);
            newLine = string.Format("Y.append({0})", y);
            _text.AppendLine(newLine);
            newLine = string.Format("Z.append({0})", z);
            _text.AppendLine(newLine);
            newLine = string.Format("TEGM.append({0})", t);
            _text.AppendLine(newLine);
            newLine = string.Format("Psi.append({0})", psi);
            _text.AppendLine(newLine);
            newLine = string.Format("Theta.append({0})", theta);
            _text.AppendLine(newLine);
            newLine = string.Format("Phi.append({0})", phi);
            _text.AppendLine(newLine);
        }

        public void FillCommand(string x, string y, string z, string psi, string theta, string phi)
        {
            var newLine = string.Format("Xc.append({0})", x);
            _text.AppendLine(newLine);
            newLine = string.Format("Yc.append({0})", y);
            _text.AppendLine(newLine);
            newLine = string.Format("Zc.append({0})", z);
            _text.AppendLine(newLine);
            newLine = string.Format("Psic.append({0})", psi);
            _text.AppendLine(newLine);
            newLine = string.Format("Thetac.append({0})", theta);
            _text.AppendLine(newLine);
            newLine = string.Format("Phic.append({0})", phi);
            _text.AppendLine(newLine);
        }

        /// <summary>
        /// Writes the recorded torques values in _text
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
            _text.AppendLine(newLine);
            newLine = string.Format("T2.append({0})", t2);
            _text.AppendLine(newLine);
            newLine = string.Format("T3.append({0})", t3);
            _text.AppendLine(newLine);
            newLine = string.Format("T4.append({0})", t4);
            _text.AppendLine(newLine);
            newLine = string.Format("T5.append({0})", t5);
            _text.AppendLine(newLine);
            newLine = string.Format("T6.append({0})", t6);
            _text.AppendLine(newLine);
            newLine = string.Format("T.append({0})", t);
            _text.AppendLine(newLine);
        }

        /// <summary>
        /// Writes the plotting instructions in the python script and executes it
        /// </summary>
        public void Trace()
        {
            //_text.AppendLine(" ");
            //_text.AppendLine("fig=plt.figure()");
            //_text.AppendLine("ax=fig.gca(projection='3d')");

            //_text.AppendLine("ax.plot(X,Y,Z)");

            //_text.AppendLine("ax.set_xlabel('X')");
            //_text.AppendLine("ax.set_ylabel('Y')");
            //_text.AppendLine("ax.set_zlabel('Z')");

            //_text.AppendLine("plt.axis('equal')");
            //_text.AppendLine("plt.legend(loc='best')");

            _text.AppendLine(" ");

            _text.AppendLine("fig2=plt.figure()");

            _text.AppendLine("plt.plot(TEGM,X,label='x')");
            _text.AppendLine("plt.plot(TEGM,Y,label='y')");
            _text.AppendLine("plt.plot(TEGM,Z,label='z')");
            _text.AppendLine("plt.plot(TEGM,Xc,'--',label='x')");
            _text.AppendLine("plt.plot(TEGM,Yc,'--',label='y')");
            _text.AppendLine("plt.plot(TEGM,Zc,'--',label='z')");

            _text.AppendLine("plt.xlabel('Temps en ms')");
            _text.AppendLine("plt.ylabel('Axes')");

            _text.AppendLine("plt.legend(loc='best')");

            _text.AppendLine("figc=plt.figure()");

            _text.AppendLine("plt.plot(TEGM,Psi,label='psi')");
            _text.AppendLine("plt.plot(TEGM,Theta,label='theta')");
            _text.AppendLine("plt.plot(TEGM,Phi,label='phi')");
            _text.AppendLine("plt.plot(TEGM,Psic,'--',label='psi')");
            _text.AppendLine("plt.plot(TEGM,Thetac,'--',label='theta')");
            _text.AppendLine("plt.plot(TEGM,Phic,'--',label='phi')");

            _text.AppendLine("plt.xlabel('Temps en ms')");
            _text.AppendLine("plt.ylabel('Axes')");

            _text.AppendLine("plt.legend(loc='best')");

            _text.AppendLine(" ");

            //_text.AppendLine("fig3=plt.figure()");

            //_text.AppendLine("plt.plot(T,T1,'--',label='Axe 1')");
            //_text.AppendLine("plt.plot(T,T2,'--',label='Axe 2')");
            //_text.AppendLine("plt.plot(T,T3,'--',label='Axe 3')");
            //_text.AppendLine("plt.plot(T,T4,'--',label='Axe 4')");
            //_text.AppendLine("plt.plot(T,T5,'--',label='Axe 5')");
            //_text.AppendLine("plt.plot(T,T6,'--',label='Axe 6')");

            //_text.AppendLine("plt.xlabel('Temps en ms')");
            //_text.AppendLine("plt.ylabel('Couples en Nm')");

            //_text.AppendLine("plt.legend(loc='best')");

            //_text.AppendLine(" ");

            //_text.AppendLine("if(T==TEGM):");  //Synchronisation condition

            //_text.AppendLine("   fig4=plt.figure()");
            //_text.AppendLine("   plt.plot(Y,T1,label='Axe 1')");

            //_text.AppendLine("   plt.xlabel('Déplacement en Y en mm')");
            //_text.AppendLine("   plt.ylabel('Couple axe 1 en Nm')");

            //_text.AppendLine("   plt.legend(loc='best')");

            _text.AppendLine("plt.show()");

            File.WriteAllText(_filePath, _text.ToString());

            ProcessStartInfo start = new ProcessStartInfo();
            string cmd = _pythonPath;
            string args = _filePath;

            start.FileName = cmd;
            start.Arguments = args;
            start.UseShellExecute = false;// Do not use OS shell
            start.CreateNoWindow = true; // We don't need new window
            start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
            start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)

            Process process = Process.Start(start);
        }

        /// <summary>
        /// Clears the StringBuilder _text
        /// </summary>
        public void Clear()
        {
            _text.Clear();
        }
    }
}
