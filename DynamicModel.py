import sympy as sp
import numpy as np
#exp.evalf(subs={a:6, b:5, c:2})





## Usefull parameters of the robot 

#External forces and momentum
F = sp.Matrix([sp.Symbol('Fx'),sp.Symbol('Fy'),sp.Symbol('Fz'),sp.Symbol('Mx'),sp.Symbol('My'),sp.Symbol('Mz'),0])

#Friction parameters of the motors
Fsecs = sp.Matrix([sp.Symbol('Fs1'),sp.Symbol('Fs2'),sp.Symbol('Fs3'),sp.Symbol('Fs4'),sp.Symbol('Fs5'),sp.Symbol('Fs6'),0])

Fvisc = sp.Matrix([sp.Symbol('Fv1'),sp.Symbol('Fv2'),sp.Symbol('Fv3'),sp.Symbol('Fv4'),sp.Symbol('Fv5'),sp.Symbol('Fv6'),0])

#Inertia of the rotors and reductors
Ia = sp.diag(sp.Symbol("Ia1"),sp.Symbol("Ia2"),sp.Symbol("Ia3"),sp.Symbol("Ia4"),sp.Symbol("Ia5"),sp.Symbol("Ia6"))

#First moments of the 7 solids of the robot (6 + 1 tool)
def Moment(i):
    M = sp.Symbol('M'+str(i))
    OGX = sp.Symbol('OG'+str(i)+'X'+str(i))
    OGY = sp.Symbol('OG'+str(i)+'Y'+str(i))
    OGZ = sp.Symbol('OG'+str(i)+'Z'+str(i))
    
    m = sp.Matrix([OGX,OGY,OGZ])
    
    return(M*m)
    
Moments = []
for i in range(7):
    Moments.append(Moment(i+1))

#Inertia matrices of the 7 solids of the robot (6 + 1 tool)
def MatriceInertie(i):
    IXX = sp.Symbol('IXX'+str(i))
    IYY = sp.Symbol('IYY'+str(i))
    IZZ = sp.Symbol('IZZ'+str(i))
    IXY = sp.Symbol('IXY'+str(i))
    IXZ = sp.Symbol('IXZ'+str(i))
    IYZ = sp.Symbol('IYZ'+str(i))
    
    m=sp.Matrix([[IXX,IXY,IXZ],[IXY,IYY,IYZ],[IXZ,IYZ,IZZ]])
    
    return(m)
    
MatricesInertie = []
for i in range(7):
    MatricesInertie.append(MatriceInertie(i+1))

#Masses of the 7 solids of the robot (6 + 1 tool)
Masses = sp.Matrix([sp.Symbol("M1"),sp.Symbol("M2"),sp.Symbol("M3"),sp.Symbol("M4"),sp.Symbol("M5"),sp.Symbol("M6"),sp.Symbol("M7")])

## Geometrical parameters of the robot - Modified Denavit Hartenberg parameters

DHParameters = []

DHParameters.append([0,'t1',0,'L1'])
DHParameters.append([-sp.pi/2,'t2',0,0])
DHParameters.append([0,'t3','-d3',0])
DHParameters.append([sp.pi/2,'t4','-d4','r4'])
DHParameters.append([-sp.pi/2,'t5',0,0])
DHParameters.append([sp.pi/2,'t6',0,0])
DHParameters.append([0,0,0,'Lt'])





## Inital computation of the geometrical matrices

#Rotation matrices between the 7 joints frames
def MatriceRot(alphai,thetai,di,ri):
    
    if(type(ri) is str):
        if(ri[0]=='-'):
            ri=ri.replace('-','')
            r = -sp.Symbol(ri)
        else:
            r = sp.Symbol(ri)
    else:
        r=ri
        
    if(type(di) is str):
        if(di[0]=='-'):
            di=di.replace('-','')
            d = -sp.Symbol(di)
        else:
            d = sp.Symbol(di)
    else:
        d=di
        
    if(type(thetai) is str):
        if(thetai[0]=='-'):
            thetai=thetai.replace('-','')
            t = -sp.Symbol(thetai)
        else:
            t = sp.Symbol(thetai)
    else:
        t=thetai
        
    if(type(alphai) is str):
        if(alphai[0]=='-'):
            alphai=alphai.replace('-','')
            alpha = -sp.Symbol(alphai)
        else:
            alpha = sp.Symbol(alphai)
    else:
        a=alphai
    
    m=sp.Matrix([[sp.cos(t),-1*sp.sin(t),0,d],[sp.cos(a)*sp.sin(t),sp.cos(a)*sp.cos(t),-1*sp.sin(a),-r*sp.sin(a)],[sp.sin(a)*sp.sin(t),sp.sin(a)*sp.cos(t),sp.cos(a),r*sp.cos(a)],[0,0,0,1]])
    
    return(m)
    
#Computation function
def InitGeo(DHParameters):
    Matrices = []
    InvMatrices = []
    
    for i in range(len(DHParameters)):
        Matrices.append(MatriceRot(DHParameters[i][0],DHParameters[i][1],DHParameters[i][2],DHParameters[i][3]))
        InvMatrices.append(sp.expand(Matrices[i].inv()))
    
    return(Matrices, InvMatrices)
        
## Initial computation of the speeds

#Rotation speed of the solid i in the frame i
def VRotation(i):
    if(i==0):
        return(sp.Matrix([0,0,0]))
    else:
        if(i==7):
            t=0
        else :
            t = sp.Symbol('t'+ str(i) + "'")
        return(sp.expand(InvMatrices[i-1].col_del(3).row_del(3)*VRotation(i-1)+sp.Matrix([0,0,t])))
        
#Translation speed of the solid i in the frame i
def VTranslation(i):
    if(i==0):
        return(sp.Matrix([0,0,0]))
    else:
        t=sp.Symbol('t'+str(i)+"'")
        return(sp.expand(InvMatrices[i-1].col_del(3).row_del(3)*(VTranslation(i-1)+VRotation(i-1).cross(sp.Matrix([Matrices[i-1][0,3],Matrices[i-1][1,3],Matrices[i-1][2,3]])))))
        
#Computation function
def InitSpeed(Matrices, InvMatrices):
    VitessesTrans = []
    VitessesRot = []
    
    for i in range(len(Matrices)+1):
        VitessesTrans.append(VTranslation(i))
        VitessesRot.append(VRotation(i))
    
    return(VitessesTrans, VitessesRot)
    
## Computation of the kinematic energy

#Kinetic energy of the solid i
def Energie(i):
    M=sp.Symbol('M'+str(i))
    E=0
    E+=(1/2)*(VitessesRot[i].T*MatricesInertie[i-1]*VitessesRot[i])[0]
    E+=(1/2)*(M*VitessesTrans[i].T*VitessesTrans[i])[0]
    E+=(Moments[i-1].T*(VitessesTrans[i].cross(VitessesRot[i])))[0]
    return(sp.expand(E))
    
#Kinetic energy of the robot and its tool
def EnergieTot():
    E=0
    for i in range(8):
        E+=Energie(i)
    return(sp.expand(E))

## Computation of the dynamic matrices

#Global inertia matrix
def MatriceInertieTot(E):
    A=sp.zeros(7,7)
    for i in range(7):
        for j in range(i,7):
            print(i,j)
            if(i==j):
                A[i,i]=2*E.coeff(sp.Symbol('t'+str(i+1)+"'"),2)
            else:
                A[i,j]=E.coeff(sp.Symbol('t'+str(i+1)+"'")*sp.Symbol('t'+str(j+1)+"'"),1)
                A[j,i]=A[i,j]
    return(sp.expand(A))

#[Optional] Influence of the rotors' inertia
def CorrectifActionneurs(A,Ia=-1):
    if(Ia==-1):
        return(A)
    else:
        return(A+Ia)
    
#Global Coriolis and centrifugal matrix
def MatriceCoriolis(A):
    n=A.shape[0]
    C=sp.zeros(n,n)
    for i in range(n):
        for j in range(n):
            print(i,j)
            for k in range(n):
                C[i,j]+=(1/2)*(sp.diff(A[i,j],sp.Symbol('t'+str(k+1)))+sp.diff(A[i,k],sp.Symbol('t'+str(j+1)))-sp.diff(A[j,k],sp.Symbol('t'+str(i+1))))*sp.Symbol("t"+str(k+1)+"'")
    return(sp.expand(C))

## Computation of the potential energy and vector
##[Hypothesis] The only source of potential energy is the action of gravity

#Potential energy of the solid i
def EnergiePot(i):
    if(i==0):
        return(0)
    else:
        E=0
        g = sp.Symbol('g')
        G = sp.Matrix([0,0,-g,0]).T
        
        M = sp.zeros(4,1)
        M[0,0]=Moments[i-1][0]
        M[1,0]=Moments[i-1][1]
        M[2,0]=Moments[i-1][2]
        M[3,0]=sp.Symbol('M'+str(i))
        
        T=sp.Identity(4)
        
        for i in range(i):
            T*=Matrices[i]
            
        E+=(G*T*M)[0]
        return(sp.expand(E))
        
#Potential energy of the robot and its tool
def EnergiePotTot():
    E=0
    for i  in range(8):
        E+=EnergiePot(i)
    return(sp.expand(E))

#Global gravity effect vector
def VecteurGravite(E):
    Q = sp.zeros(7,1)
    for i in range(7):
        Q[i,0]=sp.diff(E,sp.Symbol('t'+str(i+1)))
    return(Q)
        
## Computation of the friction term

#[Optional] Influence of the rotors' friction effect
def Frottement(Fsecs=-1,Fvisc=-1):
    G = sp.diag(sp.Symbol("t'1"),sp.Symbol("t'2"),sp.Symbol("t'3"),sp.Symbol("t'4"),sp.Symbol("t'5"),sp.Symbol("t'6"),0) 
    if(Fsecs==-1):
        if(Fvisc==-1):
            return(G*sp.zeros(7,1))
        else:
            return(sp.sign(G)*Fvisc)
    else:
        if(Fvisc!=-1):
            return(G*Fsecs+sp.sign(G)*Fvisc)
        else:
            return(G*Fsecs)
    
## Computation of the external efforts term

"""
# Le plus simple ici est de considérer que le joint 6 comprend également le porte outil et le capteur d'efforts. 
#L'outil à proprement parler ("joint 7"), correspondra au porte outil branché sur le capteur et à l'outil accroché dessus.

#Dans le cas d'une utilisation à vide, le repère 7 sera placé au bout du capteur, dans le même sens que le repère 6.
#Les propriétés du solide 7 seront toutes égales à 0, sauf la distance Lt (on suppose l'alignement parfait).
#Finalement, il faut utiliser le jacobien du dernier repère du bras, donc le septième ici (qui tient compte du décalage avec le bout du robot)

#Question a se poser plus tard = comment relier efforts mesurés aux efforts en bout d'outil ?
"""

# Jacobian matrix of the solid i in the frame i
def Jacobian(i):
    J = sp.zeros(7,7)
    for k in range(3):
        for j in range(6):
            J[k,j]=sp.expand(VitessesTrans[i][k]).coeff(sp.Symbol("t"+str(j+1)+"'"))
    for k in range(3,6):
        for j in range(6):
            J[k,j]=sp.expand(VitessesRot[i][k-3]).coeff(sp.Symbol("t"+str(j+1)+"'"))
    return(J)
            
# Global exterior efforts effect vector
def VecteurEfforts(F):
    J = Jacobian(7).T
    return(sp.expand(J*F))

## Final computation of the dynamic model

def ModeleDynamique():
    dq = sp.Matrix([sp.Symbol("t1'"),sp.Symbol("t2'"),sp.Symbol("t3'"),sp.Symbol("t4'"),sp.Symbol("t5'"),sp.Symbol("t6'"),0])
    ddq = sp.Matrix([sp.Symbol("t1''"),sp.Symbol("t2''"),sp.Symbol("t3''"),sp.Symbol("t4''"),sp.Symbol("t5''"),sp.Symbol("t6''"),0])
    Torques = Frot + Eff + A*ddq + C*dq + Q
    return(Torques)
    




## MAIN
        
def Main():
    print("Computing geometry")
    Matrices, InvMatrices = InitGeo(DHParameters)
    print("Computing speeds")
    VitessesTrans,VitessesRot = InitSpeed(Matrices,InvMatrices)  
    print("Computing kinematic energy")
    Ecin = EnergieTot()  
    print("Computing dynamic matrices")
    A = CorrectifActionneurs(MatriceInertieTot(Ecin))  
    C=MatriceCoriolis(A) 
    print("Computing potential energy")
    Epot = EnergiePotTot() 
    print("Computing gravity vector")
    Q = VecteurGravite(Epot)
    print("Computing friction vector")
    Frot = Frottement()
    print("Computing external efforts  vector")
    Eff = VecteurEfforts(F)
    print("Computing the global dynamic model")
    T = ModeleDynamique()
    
    