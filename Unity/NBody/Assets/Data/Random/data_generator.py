import pandas as pd 
import random


def generate(lower_bound:float, upper_bound:float, sample_size:int, precision=5, filename="data.csv"):
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

    while (sample_size > 0):
        for column in data:
            randfloat = round(random.uniform(lower_bound, upper_bound), precision)
            if column == "velocityX" or column == "velocityY" or column == "velocityZ":
                randfloat = randfloat / 10000

            if column == "mass":
                randfloat = 10 * abs(randfloat)
            data[column].append(randfloat)
        sample_size -= 1

    df = pd.DataFrame(data)
    df.to_csv(filename, index=False)

generate(-50, 50, 200)
