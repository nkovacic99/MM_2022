import pygame
import numpy as np
from math import cos, sin

from tkinter import Tk
from tkinter.filedialog import askopenfilename

from transform import Transform

# Constants
G = 1
DT = 0.001

DISP_W = 1280
DISP_H = 720

def main():

    # Initialize pygame
    pygame.init()
    screen = pygame.display.set_mode((DISP_W, DISP_H))
    pygame.display.set_caption('N-Body Problem')
    clock = pygame.time.Clock()

    # Read initial conditions from a csv file
    Tk().withdraw()
    filename = askopenfilename(title='Select a file with initial conditions of the system', filetypes=[('CSV Files', '*.csv')])
    data = np.genfromtxt(filename, delimiter=',')[1:]
    num_bodies = len(data)
    positions  = data[:, 0:3]
    velocities = data[:, 3:6]
    masses     = data[:, 6]

    # Initialize object for transforming 3d points for displaying on the screen
    transform = Transform()
    
    # Move "camera" so that all bodies are shown on screen (initially)
    x_max, y_max, _ = np.max(positions, axis=0)
    x_min, y_min, _ = np.min(positions, axis=0)
    scale = max( DISP_W / (x_max - x_min), DISP_H / (y_max - y_min) )
    transform.set_scale(scale / 3)
    transform.set_translation(x_min + (x_max - x_min + DISP_W) / 2, y_min + (y_max - y_min + DISP_H) / 2, 0)

    # Main loop
    while True:

        # PyGame events
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                exit()

            # Handle scaling
            if event.type == pygame.MOUSEBUTTONDOWN:
                if event.button == pygame.BUTTON_WHEELUP:
                    transform.set_scale(1.25 * transform.scale)
                if event.button == pygame.BUTTON_WHEELDOWN:
                    transform.set_scale(0.80 * transform.scale)
        
        transform.handle_input()

        # Show bodies on the screen
        screen.fill('Black')
        for pos in positions:
            screen_pos = transform.apply_transformation(pos)[:2]
            pygame.draw.circle(screen, 'White', screen_pos, 1)
        pygame.display.update()

        # Compute accelerations
        accelerations = np.zeros((num_bodies, 3))
        for i in range(num_bodies):
            for j in range(i + 1, num_bodies):
                # Newton's Gravity Equation
                dist = positions[j] - positions[i]
                a = G * dist / (np.linalg.norm(dist) ** 3)
                accelerations[i] += masses[j] * a
                accelerations[j] -= masses[i] * a

        # Apply acceleration and move bodies
        velocities += DT * accelerations
        positions  += DT * velocities

        # PyGame clock
        clock.tick(60)
        pygame.display.set_caption(f'N-Body Problem    [ FPS: {clock.get_fps()} ]')

if __name__ == '__main__':
    main()
