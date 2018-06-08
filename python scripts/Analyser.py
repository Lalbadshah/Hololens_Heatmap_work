import cv2
import numpy as np
from sklearn.cluster import KMeans

img = cv2.imread('AImage1.png')
# hm = cv2.imread('Current_Map.png')

UltraList=[[],[]]


def get_map_mem():

    with open('mapmem.txt') as f:
        data = f.readlines()
    i=0
    for line in data:
        x,y =line.split(',')
        x = float(x)
        y = float(y)
        UltraList[0].append(x)
        UltraList[1].append(720-y)
        i+=1
    return i

def Analysis1(total):    #3 zones title, instructions and other
    title_c=0
    inst_c = 0
    other_c =0
    for i in range(total):
        x = UltraList[0][i]
        y = UltraList[1][i]
        if(x>=300 and x<=900):
            if(y>=20 and y<=100):
                #print("Title yesssss")
                title_c+=1
                continue
            if(y>=270 and y<=600):
                inst_c+=1
                continue
            else:
                other_c+=1
                continue
        else:
            other_c+=1

    print(title_c,inst_c,other_c,total)
    img1 = img.copy()
    font = cv2.FONT_HERSHEY_SIMPLEX
    fscale = 1.5
    cv2.putText(img1,str(title_c*100/total)+'%',(310,70), font, fscale,(0,0,255),2,cv2.LINE_AA)
    cv2.putText(img1,str(inst_c*100/total)+'%',(310,400), font, fscale,(0,255,0),2,cv2.LINE_AA)
    cv2.putText(img1,"Other "+str(other_c*100/total)+'%',(100,200), font, fscale,(255,0,0),2,cv2.LINE_AA)
    cv2.imshow('Analysis 1',img1)
    # cv2.imshow('Final Heatmap',hm)
    cv2.waitKey(0)

def Analysis2(total):
    npU = np.array([UltraList[0][0],UltraList[1][0]])
    for i in range(1,total):
        temp = np.array([UltraList[0][i],UltraList[1][i]])
        npU = np.array([npU,[UltraList[0][i],UltraList[1][i]]])
    #centers = np.array([[600,60],[600,435],[320,360],[960,540]])
    kmeans = KMeans(init ='k-means++' ,precompute_distances=True,verbose=1).fit(npU)
    centroids = kmeans.cluster_centers_
    print(centroids)



total = get_map_mem()
Analysis1(total)
# Analysis2(total)
