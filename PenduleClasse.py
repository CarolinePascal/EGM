import matplotlib.pyplot as plt
import numpy as np
import time

#Calcul la rotation du vecteur v d'un angle a autour de z (troisième axe)
def rotation(v,a):
        M=np.array([[np.cos(a),-np.sin(a)],[np.sin(a),np.cos(a)]])
        return(np.dot(M,v))

class pendule_simple:
    def __init__(self,m,L,R,X0):
        #Masse du pendule
        self.m=m
        #Longueur du pendule
        self.L=L
        #Raideur du poignet du robot
        self.R=R
        #Position initiale sous la forme (np.array([t,l,a]))
        self.X0=X0
        
    #Efforts dus à la raideur du poignet du robot
    def T(self,l):
        return(self.R*(l-self.L))

    #PFD au niveau du contact entre la pince et le pendule simple
    def F(self,t,l,a):
        x=l*np.sin(t)
        y=l*np.cos(t)
        
        M = self.m*10*x/2
        Fr = -(self.m*10*np.cos(t)) + self.T(l)*np.cos(a)
        Ft = -(self.m*10*np.sin(t)) + self.T(l)*np.sin(a)
        return(np.array([M,Fr,Ft]))

    #Tracé du pendule et du poignet du robot
    def trace(self,t,l,a,animate=False):
        x=l*np.sin(t)
        y=l*np.cos(t)
        
        if(l>=self.L):
            plt.plot([0,self.L*np.sin(t)],[0,self.L*np.cos(t)],'r',zorder=1)
            plt.plot([0,x],[0,y],'r--',zorder=1)
            
        elif(l<self.L):
            plt.plot([0,self.L*np.sin(t)],[0,self.L*np.cos(t)],'r--',zorder=1)
            plt.plot([0,x],[0,y],'r',zorder=1)
                    
        plt.scatter([0,x],[0,y],color="white",edgecolor="black",zorder=2)
        
        v=np.array([x*0.05,y*0.05])
        v1=rotation(v,np.pi/2+a)
        v2=rotation(v,-np.pi/2+a)
        v1=np.array([x,y])+v1
        v2=np.array([x,y])+v2
        
        plt.plot([x,v1[0]],[y,v1[1]],'b',zorder=1)
        plt.plot([x,v2[0]],[y,v2[1]],'b',zorder=1)
        
        v3=rotation(v,a)
        v31 = v1+v3
        v32 = v2+v3
        
        plt.plot([v1[0],v31[0]],[v1[1],v31[1]],'b')
        plt.plot([v2[0],v32[0]],[v2[1],v32[1]],'b')
        
        plt.xlabel(r'x (en m)')
        plt.ylabel(r'y (en m)')
        plt.legend()
        
        
        plt.axis('equal')
        if(animate==False):
            plt.show()
         
    #Animation
    def animate(self,command):
        Pt,Pl,Pa = command(50,self);
        
        
        figure=plt.figure()
        time.sleep(5)
        self.trace(self.X0[0],X0[1],X0[2])
        time.sleep(5)
        for i in range(len(Pa)):
            self.trace(Pt[i],Pl[i],Pa[i],True)
            plt.pause(0.5)
            figure.clear()
            
        


    
    
        
    