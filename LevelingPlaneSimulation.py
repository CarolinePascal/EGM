import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import matplotlib.animation as anim

###WARNING###
#This programm is a very simplified simulation of a prototype of an auto-leveling command for a 4 actuators table.
#Our hypthesis :
#   - The two angles, alongside the longest and the sortest side can be decoupled
#   - We only consider small angles gap between the initial configuration and the leveled one : in these conditions, the plane moves only by straight tranlations (represented by d1 and d2), and not circular ones
#   - We assume that the actuators, when still, behave like ball-joint connections (in reality, they are embedded in the plane, and their bases are ball-joints connections = for small angles, the connection with the plane can be assimilated with a ball-joint connection, but not with huge ones).

#To build a more realistic model, one should /
#   - Take into account that the plane moves alongside with its normal, due to the embedding of the actuators
#   - Take into account that the non-moving actuators don't behave like joint-ball connections
#   - Take into account the complexity of the motion of the plane for huge angles : the border of the plane moves normaly alongside a circle whose center is located on the joint-ball connection

#Command of the angle along the largest side of the plane
def commande1(t20,t10,d1,l):
    c1 = d1/l
    t2 = t20
    t1 = c1+t10
    return(t1)

#Command of the angle along the shortest side of the plane
def commande2(t20,t10,d2,h):
    c2 = d2/h
    t1 = t10
    t2 = c2+t20
    return(t2)

#Calculation of the normal vector to the plane
def nt(t2,t1):
    n=(np.array([-np.cos(t2)*np.sin(t1),-np.sin(t2)*np.cos(t1),np.cos(t1)+np.cos(t2)]))
    return(n/np.linalg.norm(n))

#Scalar product
def Scalar(v1,v2):
    retrun(v1[0]*v2[0]+v1[1]*v2[1]+v1[2]*v2[2])

#Display of the plane according to t1 and t2 with given lengths
def drawRectangle(axes,h,l,t2,t1,couleur,offset):
    
    normale=nt(t2,t1)*5
    
    axes.plot([0,l],[0,0],[offset,offset+l*np.tan(t1)],color=couleur)
    axes.plot([0,0],[0,h],[offset,offset+h*np.tan(t2)],color=couleur)
    axes.plot([l,l],[0,h],[offset+l*np.tan(t1),offset+l*np.tan(t1)+h*np.tan(t2)],color=couleur)
    axes.plot([0,l],[h,h],[offset+h*np.tan(t2),offset+h*np.tan(t2)+l*np.tan(t1)],color=couleur)
    
    axes.plot([0,normale[0]],[0,normale[1]],[0,normale[2]],color='b')
    
    plt.axis('equal')
    plt.show()

#Gets the values of t1 and t2 according to unphased measurements alpha and beta
def getTethas(mesA,mesB,alpha,beta):
    A = [[np.cos(alpha)**2,np.sin(alpha)**2],[np.cos(beta)**2,np.sin(beta)**2]]
    B=[np.cos(mesA),np.cos(mesB)]
    X = np.linalg.solve(A,B)
    t2 = np.arccos(X[1])
    t1 = np.arccos(X[0])
    return(t2,t1)

#Simulation
def test():

    t20 = 1*np.pi/180
    t10 = 3*np.pi/180

    print(t20)

    t20draw = t20
    t10draw = t10

    l = 100
    h = 50
    d1 = 0
    d2 = 0

    fig = plt.figure()
    ax = Axes3D(fig)
    drawRectangle(ax,h,l,0,0,'blue',0)
    drawRectangle(ax,h,l,t20,t10,'red',10)
    plt.show()

    T= np.empty((2,1000))

    if(t10>0):
        pas = -0.001
    else :
        pas = +0.001

    print(pas)

    i=0
    counterd1=0

    while(abs(t10)>=0.001 and i<500):
        T[0,i]=t20
        T[1,i]=commande1(t20,t10,d1,l)
        t20=T[0,i]
        t10=T[1,i]
        d1+=pas
        counterd1+=d1
        i+=1
        
    print(test)
    print(t20)

    if(t20>0):
        pas = -0.001
    else :
        pas = +0.001

    print(pas)
    
    counterd2=0

    while(abs(t20)>=0.001 and i<500):
        T[0,i]=commande2(t20,t10,d2,h)
        T[1,i]=t10
        t20=T[0,i]
        t10=T[1,i]
        d2+=pas
        counterd2+=d2
        i+=1

    print(t20)
    print(t10)
    
    print(counterd1)
    print(counterd2)
    
    #print(T[0])
    
    fig1= plt.figure()
    ax1 = Axes3D(fig1)

    def update(i):
        ax1.clear()
        drawRectangle(ax1,h,l,0,0,'blue',0)
        drawRectangle(ax1,h,l,t20draw,t10draw,'red',10)
        drawRectangle(ax1,h,l,T[0,i],T[1,i],'green',10)

    a = anim.FuncAnimation(fig1,update,frames = 600,repeat=False)
    plt.show()
    


#test()











