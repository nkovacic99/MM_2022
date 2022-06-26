import pandas as pd 
import random
import math


def generate(num_of_added_galaxies:int, viableCubeSide:float, m:float, radius:float, num_in_galaxy:int, k:int, zacRazdalja:float, m1:float, m2:float, m_in_1:float, m_in_2:float, radius1:float, radius2:float, num_in_one:int, num_in_two:int, G:float, precision=5, filename="data.csv"):
    """
    Generates a .csv file with following columns:
        - positionX
        - positionY
        - positionZ
        - velocityX
        - velocityY
        - velocityZ
        - mass
    All represent float numbers.

    sample_size: number of rows in .csv (excluding header)  
    """ 
    data = {
        "positionX": [],
        "positionY": [],
        "positionZ": [],
        "velocityX": [],
        "velocityY": [],
        "velocityZ": [],
        "mass": [],
    }


    frameSideLength = viableCubeSide



    # Sredisci:

    m1 = m1
    m2 = m2
    pos1x = 0
    pos1y = 0
    pos1z = 0
    pos2x = zacRazdalja
    pos2y = 0
    pos2z = 0
    vel1x = 0
    vel1y = 0
    vel1z = 0
    vel2x = 0
    vel2y = k * math.sqrt(G * (m1 + num_in_one * (2*m_in_1/3)) / math.pow(pos2x, 3))
    vel2z = 0
    

    
    data["positionX"].append(pos1x)
    data["positionY"].append(pos1y)
    data["positionZ"].append(pos1z)
    data["velocityX"].append(vel1x)
    data["velocityY"].append(vel1y)
    data["velocityZ"].append(vel1z)
    data["mass"].append(m1)

    data["positionX"].append(pos2x)
    data["positionY"].append(pos2y)
    data["positionZ"].append(pos2z)
    data["velocityX"].append(vel2x)
    data["velocityY"].append(vel2y)
    data["velocityZ"].append(vel2z)
    data["mass"].append(m2)



    # Prva galaksija:

    for x in range(1, num_in_one):
        dvaPi = 2 * math.pi
        angleInCircle = round(random.uniform(0, dvaPi), precision)
        specificRadius = round(random.uniform(0, radius1), precision)
        mass = round(random.uniform(m_in_1/3, m_in_1), precision)

        posx = pos1x + specificRadius * math.cos(angleInCircle)
        posy = pos1y + specificRadius * math.sin(angleInCircle)

        r = [specificRadius * math.cos(angleInCircle), specificRadius * math.sin(angleInCircle)]
        rPravokotenNormiran = [math.cos(angleInCircle + math.pi/2), math.sin(angleInCircle + math.pi/2)]

        kotnaHitrost = math.sqrt(G * m1 / math.pow(specificRadius, 3))


        data["positionX"].append(posx)
        data["positionY"].append(posy)
        data["positionZ"].append(pos1z)
        data["velocityX"].append(vel1x + kotnaHitrost * specificRadius * rPravokotenNormiran[0])
        data["velocityY"].append(vel1y + kotnaHitrost * specificRadius * rPravokotenNormiran[1])
        data["velocityZ"].append(vel1z)
        data["mass"].append(mass)
    


# Druga galaksija:

    for x in range(1, num_in_two):
        dvaPi = 2 * math.pi
        angleInCircle = round(random.uniform(0, dvaPi), precision)
        specificRadius = round(random.uniform(0, radius2), precision)
        mass = round(random.uniform(m_in_2 / 3, m_in_2), precision)

        posx = pos2x + specificRadius * math.cos(angleInCircle)
        posy = pos2y + specificRadius * math.sin(angleInCircle)

        r = [specificRadius * math.cos(angleInCircle), specificRadius * math.sin(angleInCircle)]
        rPravokotenNormiran = [math.cos(angleInCircle + math.pi/2), math.sin(angleInCircle + math.pi/2)]

        kotnaHitrost = math.sqrt(G * m2 / math.pow(specificRadius, 3))

        data["positionX"].append(posx)
        data["positionY"].append(posy)
        data["positionZ"].append(pos2z)
        data["velocityX"].append(vel2x + kotnaHitrost * specificRadius * rPravokotenNormiran[0])
        data["velocityY"].append(vel2y + kotnaHitrost * specificRadius * rPravokotenNormiran[1])
        data["velocityZ"].append(vel2z)
        data["mass"].append(mass)




    for i in range (1, num_of_added_galaxies+1):

        mass = m
        posx = round(random.uniform(-frameSideLength, frameSideLength), precision)
        posy = round(random.uniform(-frameSideLength, frameSideLength), precision)
        posz = round(random.uniform(-frameSideLength, frameSideLength), precision)

        velx = 0
        vely = 0
        velz = 0

        data["positionX"].append(posx)
        data["positionY"].append(posy)
        data["positionZ"].append(posz)
        data["velocityX"].append(velx)
        data["velocityY"].append(vely)
        data["velocityZ"].append(velz)
        data["mass"].append(m)
            

        for x in range(1, num_in_galaxy):
            dvaPi = 2 * math.pi
            angleInCircle = round(random.uniform(0, dvaPi), precision)
            specificRadius = round(random.uniform(0, radius), precision)
            mass = round(random.uniform(1, 2), precision)

            posx = posx + specificRadius * math.cos(angleInCircle)
            posy = posy + specificRadius * math.sin(angleInCircle)

            r = [specificRadius * math.cos(angleInCircle), specificRadius * math.sin(angleInCircle)]
            rPravokotenNormiran = [math.cos(angleInCircle + math.pi/2), math.sin(angleInCircle + math.pi/2)]

            kotnaHitrost = math.sqrt(G * mass / math.pow(specificRadius, 3))


            data["positionX"].append(posx)
            data["positionY"].append(posy)
            data["positionZ"].append(posz)
            data["velocityX"].append(velx + kotnaHitrost * specificRadius * rPravokotenNormiran[0])
            data["velocityY"].append(vely + kotnaHitrost * specificRadius * rPravokotenNormiran[1])
            data["velocityZ"].append(velz)
            data["mass"].append(mass)






    df = pd.DataFrame(data)
    df.to_csv("dveGala.csv", index=False)


# prvi parameter je stevilo dodatnih galaksij, ki imajo in all 100 teles
# drugi parameter je dolzina stranice kocke, v kateri se vse te galaksije nahajajo
generate(1, 400,         100, 100, 1000,      220, 400, 100000, 30000, 1.5, 1.5, 100, 100,     100, 100,    10)
# num_of_added_galaxies:int, viableCubeSide:float         m:float, radius:float, num_in_galaxy:int, k:int, zacRazdalja:float, m1:float, m2:float, m_in_1:float, m_in_2:float, radius1:float, radius2:float, num_in_one:int, num_in_two:int, G:float