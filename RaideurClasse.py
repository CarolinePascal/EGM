import numpy as np
import warnings
warnings.filterwarnings('error')
import sys
sys.path.append('C:/Users/Nahkriin/Desktop/Cesure1')

from PenduleClasse import *
"""
#Matrice de raideur complète - un vrai fiasco, même pas la peine de commenter ce code tellement c'est inutile
class Raideur_Complete:
    def __init__(self,K=np.diag(np.ones(3)),A=np.ones(3),D=np.ones(3)):
        self.K=K
        self.A=A
        self.D=D
    
    def modifK(self,K):
        self.K=K
    
    def modifA(self,A):
        self.A=A
    
    def modifD(self,D):
        self.D=D
        
    def Kc(self,F0,FX,FY,FZ,X0,XX,XY,XZ):
        dFX = FX - F0
        dFY = FY - F0
        dFZ = FZ - F0
        if(dFX.all()==0):
            dFX=np.array([self.D[0],0,0])
        if(dFY.all()==0):
            dFY=np.array([0,self.D[1],0])
        if(dFZ.all()==0):
            dFZ=np.array([0,0,self.D[2]])
        dXX = (XX - X0)[0]
        dXY = (XY - X0)[1]
        dXZ = (XZ - X0)[2]
        K=np.zeros((3,3))
        for i in range(3):
            K[i,0]=dFX[i]/dXX
            K[i,1]=dFY[i]/dXY
            K[i,2]=dFZ[i]/dXZ
        self.K=K
        return()
        
    def command(self,n,pendule):
        
        X0=pendule.X0
        F0 = pendule.F(X0[0],X0[1],X0[2])
    
        Xt=[]
        Xl=[]
        Xa=[]
    
        Xt.append(X0[0])
        Xl.append(X0[1])
        Xa.append(X0[2])
    
        for i in range(n):
        
            X10 = X0 + np.array([self.A[0],0,0])
            X11 = X0 + np.array([0,self.A[1],0])
            X12 = X0 + np.array([0,0,self.A[2]])
            
            F10 = pendule.F(X10[0],X10[1],X10[2])
            F11 = pendule.F(X11[0],X11[1],X11[2])
            F12 = pendule.F(X12[0],X12[1],X12[2])
            
            try :
                self.Kc(F0,F10,F11,F12,X0,X10,X11,X12)
            except Warning:
                break
            
            try :
                X1=X0-np.dot(np.linalg.inv(self.K),F0)
            except Warning:
                break
            
            F1=pendule.F(X1[0],X1[1],X1[2])
            
            X0 = X1
            F0 = F1
    
            Xt.append(X1[0])
            Xl.append(X1[1])
            Xa.append(X1[2])
        
        return(Xt,Xl,Xa)
"""

#Matrice de raideur constante
class Raideur:
    def __init__(self,K):
        #Matrice de raideur K
        self.K=K
        
    def command(self,n,pendule):
        #Position initiale
        X0 = pendule.X0
        
        Xt=[]
        Xl=[]
        Xa=[]
        
        Xt.append(X0[0])
        Xl.append(X0[1])
        Xa.append(X0[2])
        
        recordF1 = []
        recordF2 = []
        recordF3 =[]
        
        #Récurrence
        for i in range(n):
            #Efforts n
            F0 = pendule.F(X0[0],X0[1],X0[2])
        
            recordF1.append(F0[0])
            recordF2.append(F0[1])
            recordF3.append(F0[2])
            
            #Position n+1
            X1 = X0 - np.dot(np.linalg.inv(self.K),F0)
            
            #Itération
            X0=X1
            
            Xt.append(X0[0])
            Xl.append(X0[1])
            Xa.append(X0[2])
            
        F0 = pendule.F(X0[0],X0[1],X0[2])
        recordF1.append(F0[0])
        recordF2.append(F0[1])
        recordF3.append(F0[2])
        
        N=np.linspace(0,len(Xt),len(Xt));
        
        #Plot
        fig = plt.figure()
        plt.plot(N,Xt,'g',label=r"$\theta$")
        plt.plot(N,Xl,'r',label="$l$")
        plt.plot(N,Xa,'b',label="$a$")
        plt.xlabel(r'Itérations')
        plt.ylabel(r'Amplitude (en rad ou m)')
        plt.legend()
        plt.show()
        
        figbis=plt.figure()
        plt.plot(N,recordF1,'k',label=r"$M_z$")
        plt.plot(N,recordF2,'orange',label=r"$F_l$")
        plt.plot(N,recordF3,'pink',label=r"$F_{\theta}$")
        plt.xlabel(r'Itérations')
        plt.ylabel(r'Amplitude (en N ou Nm)')
        plt.legend()
        plt.show()
        
        
        del self.K
        
        return(Xt,Xl,Xa)
            
#Matrice de raideur diagonale
class Raideur_Simple:
    def __init__(self,K=np.diag(np.ones(3))):
        #Intialisation de la matrice K
        self.K=K
        
    def modifK(self,K):
        self.K=K
        
    #Calcul de la matrice Kn à partir de Xn et Fn
    def Kd(self,F,F0,X,X0):
        dF = F-F0
        dX = X-X0
        K=np.diag([dF[0]/dX[0],dF[1]/dX[1],dF[2]/dX[2]])
        self.K=K
        return()
            
    def command(self,n,pendule):
        #Position initiale
        X0=pendule.X0
        #Efforts initiaux
        F0 = pendule.F(X0[0],X0[1],X0[2])
        
        #Première position
        X1 = X0 + np.dot(np.linalg.inv(self.K),F0)
        
        Xt=[]
        Xl=[]
        Xa=[]
        
        recordF1 = []
        recordF2 = []
        recordF3 =[]
        
        recordF1.append(F0[0])
        recordF2.append(F0[1])
        recordF3.append(F0[2])
    
        Xt.append(X0[0])
        Xt.append(X1[0])
        Xl.append(X0[1])
        Xl.append(X1[1])
        Xa.append(X0[2])
        Xa.append(X1[2])
        
        #Récurrence
        for i in range(n):
                #Efforts
                F1 = pendule.F(X1[0],X1[1],X1[2])
                
                recordF1.append(F0[0])
                recordF2.append(F0[1])
                recordF3.append(F0[2])
                
                #Calcul de Kn
                try:
                    self.Kd(F1,F0,X1,X0)
                except Warning:
                    break
                    
                #Inversion de Kn
                try:
                    X2=X1 - np.dot(np.linalg.inv(self.K),F1)
                except Warning:
                    break
                    
                Xt.append(X2[0])
                Xl.append(X2[1])
                Xa.append(X2[2])
                
                #Itération
                X0 = X1
                X1 = X2
                F0 = F1
                
        N=np.linspace(0,len(Xt),len(Xt));

        #Plot
        fig = plt.figure()
        plt.plot(N,Xt,'g',label=r"$\theta$")
        plt.plot(N,Xl,'r',label="$l$")
        plt.plot(N,Xa,'b',label="$a$")
        plt.xlabel(r'Itérations')
        plt.ylabel(r'Amplitude (en rad ou m)')
        plt.legend()
        plt.show()
        
        figbis=plt.figure()
        plt.plot(N,recordF1,'k',label=r"$M_z$")
        plt.plot(N,recordF2,'orange',label=r"$F_l$")
        plt.plot(N,recordF3,'pink',label=r"$F_{\theta}$")
        plt.xlabel(r'Itérations')
        plt.ylabel(r'Amplitude (en N ou Nm)')
        plt.legend()
        plt.show()
        
        del self.K
        
        return(Xt,Xl,Xa)
        
    