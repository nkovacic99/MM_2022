import pandas as pd 
import random
import math


def generate(k:int, zacRazdalja:float, m1:float, m2:float, m_in_1:float, m_in_2:float, radius1:float, radius2:float, num_in_one:int, num_in_two:int, G:float, precision=5, filename="data.csv"):
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






    # while (sample_size > 0):
    #     for column in data:
    #         randfloat = round(random.uniform(lower_bound, upper_bound), precision)
    #         if column == "velocityX" or column == "velocityY" or column == "velocityZ":
    #             randfloat = randfloat / 10000

    #         if column == "mass":
    #             randfloat = 10 * abs(randfloat)
    #         data[column].append(randfloat)
    #     sample_size -= 1


    #print(data)
    # print(len(data["positionX"]))
    # print(len(data["positionY"]))
    # print(len(data["positionZ"]))
    # print(len(data["velocityX"]))
    # print(len(data["velocityY"]))
    # print(len(data["velocityZ"]))
    # print(len(data["mass"]))

    df = pd.DataFrame(data)
    df.to_csv("dveGala50.csv", index=False)

generate(220, 400, 100000, 30000, 1.5, 1.5, 100, 100, 50, 50, 10)
# k:int, zacRazdalja:float, m1:float, m2:float, m_in_1:float, m_in_2:float, radius1:float, radius2:float, num_in_one:int, num_in_two:int, G:float
#   deliteljHitrosti:float, offshoot:float, radius:float, num_in_galaxy:int, G:float