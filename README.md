# Introduction

Due to the project being quite large, we include this simple README file to make it easier to navigate for others.

# Structure of the project
Project is structured in folders:

## Time Analysis
Time Analysis folder contains analysis of the times required for a single iteration at different numbers of bodies for different implementations.
A python script `plot.py` for plotting said analysis is also included in the folder.

## Presentation 
Contains the videos of the main (and more impressive) simulations. We recommend having a look at them, because they truly are beautiful.

## Python 
Contains the Python implementation of the project. It contains two python scripts:
- `n_body_problem.py` performs the main simulation of the system.
- `transform.py` scirpt includes a class for applying transformations to the scene (e.g for moving around the scene).

## Unity/NBody
Contains the Unity implementation of the project. All of our code is located in the `Assets` folder, which conatains two important subfolders:
- `Scripts` folder contains all of scripts we wrote for Unity implementation. It is (again) divided into two subfolders. The most relevant scripts
  for simulation are located in the `Simulation` folder.
- `Data Generation` contains all .csv files we generated furing developement. It also contains a `Scripts` subfolder with python scripts we
  used for generating data.

# Running the project

## Python
1. Head into the Python directory
`cd Python`
2. Install the requirements with pip
`pip install -r requirements.txt`
3. Run the python implementation
`python n_body_problem.py`
4. Once opened, you first need to select the .csv file containing inital conditions to use in the pop-up.
5. When the simulation is running, you can use the
    * mouse wheel, to zoom in and out
    * mouse drag, to drag around the scene
    * W and S, A and D, Q and E pairs of keys to rotate around the scene

## Unity
1. Head into the Unity/Build directory
`cd Unity/Build`
2. Select the system you're running (if you're using MacOS, you should head into Linux directory)
3. If you're on Linux, change the permissions of the _NBody.x86\_64_ file to executable
`chmod +x NBody.x86_64`
4. If on Windows, run the _NBody.exe_, else run the `./NBody.x86_64`
5. Once opened, press the _Start Simulation_ and then select the .csv file you wish to read (these files are located in the _unity/Build/{system}/Data_ directory)

## Additional notes
* For running the _Export to video_ option, _ffmpeg_ has to be installed on the system and added to PATH.
* We recommend running in the GPU mode, but the .csv files used **must contain** multiple of a 100 bodies
* We also ecommend using colissions on a smaller set of bodies
* It is also recommended to start small with data samples and then gradually select files with larger number of bodies. 
PCs were restarted many times when building this project due to forgetting this point specifically. :)


