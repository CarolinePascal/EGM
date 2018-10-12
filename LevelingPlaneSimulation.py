import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import matplotlib.animation as anim

def commande1(t10,t20,d1,l):
    c1 = np.arctan(d1/l)
    t1 = t10
    t2 = np.arcsin(np.sin(c1+t20)+np.sin(c1)*np.cos(t10))
    return(t2)

def commande2(t10,t20,d2,h):
    c2 = np.arctan(d2/h)
    t2 = t20
    t1 = np.arcsin(np.sin(c2+t10)+np.sin(c2)*np.cos(t20))
    return(t1)

def nt(t1,t2):
    return([np.sin(t2),-np.sin(t1),np.cos(t1)+np.cos(t2)])

def Scalar(v1,v2):
    retrun(v1[0]*v2[0]+v1[1]*v2[1]+v1[2]*v2[2])

def drawRectangle(axes,h,l,t1,t2,couleur,offset):
    axes.plot([0,l],[0,0],[offset,offset+l*np.tan(t2)],color=couleur)
    axes.plot([0,0],[0,h],[offset,offset+h*np.tan(t1)],color=couleur)
    axes.plot([l,l],[0,h],[offset+l*np.tan(t2),offset+l*np.tan(t2)+h*np.tan(t1)],color=couleur)
    axes.plot([0,l],[h,h],[offset+h*np.tan(t1),offset+h*np.tan(t1)+l*np.tan(t2)],color=couleur)
    plt.axis('equal')

def getTethas(mesA,mesB,alpha,beta):
    A = [[np.cos(alpha)**2,np.sin(alpha)**2],[np.cos(beta)**2,np.sin(beta)**2]]
    B=[np.cos(mesA),np.cos(mesB)]
    X = np.linalg.solve(A,B)
    t1 = np.arccos(X[1])
    t2 = np.arccos(X[0])
    return(t1,t2)

def test():

    t10 = 60*np.pi/180
    t20 = 0*np.pi/180

    print(t10)

    t10draw = t10
    t20draw = t20

    l = 100
    h = 50
    d1 = 0
    d2 = 0

    fig = plt.figure()
    ax = Axes3D(fig)
    drawRectangle(ax,h,l,0,0,'blue',0)
    drawRectangle(ax,h,l,t10,t20,'red',10)
    plt.show()

    T= np.empty((2,1000))

    if(t20>0):
        pas = -0.001
    else :
        pas = +0.001

    print(pas)

    i=0

    while(abs(t20)>=0.005 and i<500):
        T[0,i]=t10
        T[1,i]=commande1(t10,t20,d1,l)
        t10=T[0,i]
        t20=T[1,i]
        d1+=pas
        i+=1

    if(t10>0):
        pas = -0.001
    else :
        pas = +0.001

    print(pas)

    while(abs(t10)>=0.005 and i<500):
        T[0,i]=commande2(t10,t20,d2,h)
        T[1,i]=t20
        t10=T[0,i]
        t20=T[1,i]
        d2+=pas
        i+=1

    print(t10)
    print(t20)

    fig1= plt.figure()
    ax1 = Axes3D(fig1)

    def update(i):
        ax1.clear()
        drawRectangle(ax1,h,l,0,0,'blue',0)
        drawRectangle(ax1,h,l,t10draw,t20draw,'red',10)
        drawRectangle(ax1,h,l,T[0,i],T[1,i],'green',10)

    a = anim.FuncAnimation(fig1,update,frames = 600,repeat=False)
    plt.show()


test()











