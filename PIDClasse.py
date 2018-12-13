import numpy as np
import sys
sys.path.append('C:/Users/Nahkriin/Desktop/Cesure1')

from PenduleClasse import *

class PID:
    def __init__(self,K=np.diag([1,1,1]),G=np.diag([1,1,1]),D=np.diag([1,1,1])):
        self.K = K
        self.G = G
        self.D = D
    
    #Coefficient proportionnel
    def modifK(self,K):
        self.K=K
    
    #Coefficient intégral
    def modifG(self,G):
        self.G=G
        
    #Coefficient dérivé
    def modifD(self,D):
        self.D=D
        
    def modifKGD(self,K,G,D):
        self.K=K
        self.G=G
        self.D=D
        
    def command(self,n,pendule):
        #Position intiale
        X0=pendule.X0
        #Mesure initiale
        Fp0 = pendule.F(X0[0],X0[1],X0[2])
        
        X = []
        Fl = []
        
        recordF1 = []
        recordF2 = []
        recordF3 =[]
        
        Pa = []
        Pl = []
        Pt = []
        
        Fl.append(Fp0)
        recordF1.append(Fp0[0])
        recordF2.append(Fp0[1])
        recordF3.append(Fp0[2])
        
        X.append(X0)
        Pt.append(X0[0])
        Pl.append(X0[1])
        Pa.append(X0[2])
        
        #Récurrence
        for i in range(n):
            #Position n+1
            X1 = X0 - np.dot(self.K,Fp0) - np.dot(self.G,(sum(Fl)/len(Fl))) - np.dot(self.D,(Fp0-Fl[len(Fl)-2]))
            #Efforts n+1
            Fp1 = pendule.F(X1[0],X1[1],X1[2])
            
            recordF1.append(Fp1[0])
            recordF2.append(Fp1[1])
            recordF3.append(Fp1[2])
            Fl.append(Fp1)
            
            X.append(X1)
            Pt.append(X1[0])
            Pl.append(X1[1])
            Pa.append(X1[2])
            
            #Iteration
            X0=X1
            Fp0=Fp1
            
        N=np.linspace(0,n,n+1);
    
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
        del self.D
        del self.G
        
        return(Pt,Pl,Pa)
        
    