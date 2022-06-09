# N-Body Problem

A plan for working on the *Mathematical Modelling* project is described below. Perhaps it would
also be good to dedicate some time to writing a draft of the **Report** during all phases of
working on the project.

## 0. Shitty Prototype in Python
This is already mostly done. However, it is very slow and can only run calculations and
rendering for about 100 bodies at once. This is mostly cause by the fact that even the
rendering takes place on the CPU and not the GPU.

## 1. Basic Prototype in _Unity_

First we need to create a working prototype in Unity that is capable of reading 
initial conditions of a system of N bodies from a file and then simulating the dynamics
of that system. It should be able to do so in real time (later on also export to video).

The general idea (which is also roughly implemented in _Unity_ folder):
* Create a new project in Unity
* Add an `Empty Object` to the scene. Attach a script to this object that does the following:
    * Read initial conditions (_position_, _velocity_, _mass_) from input file and instantiate a new primitive Sphere object for each body
    * Remove `SphereCollision` component from the created Sphere
    * Modify the `MeshRenderer` component settings of the Sphere for better performance
    * On every step, calculates the _gravitational force_ caused by other Spheres for each Sphere using _Newton's equations_.
    * After all forces are calculated, for each Sphere determine the _acceleration_ and, it to the Sphere's _velocity_ and modify its _position_

## 2. Intelligent Generator of Initial Conditions

After we have a working prototype, we can start working on an **Initial Conditions Generator**.
The generator should be able to at least do the following:
* Generating a [galaxy](https://en.wikipedia.org/wiki/Galaxy), where stars initially orbit some center point
* Generating two galaxies that pass each other
* Generating two galaxies that collide

More ideas of scenarios that we could (possibly) generate:
* Generating M galaxies that revolve around a center point
* Generating 3 galaxies that travel around a [Figure-8 Shape](https://www.youtube.com/watch?v=b0nlqX3j_bo) (if that is even possible)
* Adding very light super fast bodies to the system

## 3. Export Unity Scene to Video

In addition to _Real-Time_ simulation, the Unity implementation should also support a mode
that allows the Scene to be rendered longer and then exported to a video, which would allow
us to create scenes with many more bodies.

To do so, we will also need to figure out how to position the Unity camera well before running
the program, so that the output video will look _decent_ :)

## 4. Improve Performance

The performance can (and should) be improved on two sides: A and B described below.

### A) **Rendering** in Unity
Find a way to render many objects faster in Unity.

### B) **Calculation** of Forces
* Try using the [Runge-Kutta mathod](https://en.wikipedia.org/wiki/Runge%E2%80%93Kutta_methods) instead of [Euler's method](https://en.wikipedia.org/wiki/Euler_method)
* Number of calculations can be halved by calculating the force for a pair of bodies only once
* Pray to god and then research if Unity already offers an API to run calculations on the GPU

## 5. Extra Candy

These are some extra features to _pimp out_ our project. It is probably best to save these for the end,
if there is any time left.

* Add colors to bodies based on their _velocity_ and change their _radius_ accoridng to their mass
* Implement a system for detecting and handling collisions. We could make it so that when two bodies collide,
  they are merged into one with _mass_ equal to sum of masses of merged bodies. The _velocity_ of the merged
  body can be calculated by using the _Law of Momenutm Conservation_
* Hire Morgan Freeman to narrate the exported video
