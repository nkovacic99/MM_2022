import pygame
import numpy as np
from math import cos, sin

class Transform():

    def __init__(self):
        # Transforamtion variables
        self.rot_matrix  = np.eye(3)
        self.translation = np.zeros(3)
        self.rotation    = np.zeros(3)
        self.scale       = 1
        # Input variables
        self.mouse_click_prev = False
        self.mouse_pos_prev  = (0, 0)

    def update_rot_matrix(self):
        a, b, c = self.rotation
        yaw   = np.array([ [cos(a), -sin(a), 0], [sin(a), cos(a), 0], [0, 0, 1] ])
        pitch = np.array([ [cos(b), 0, sin(b)], [0, 1, 0], [-sin(b), 0, cos(b)] ])
        roll  = np.array([ [1, 0, 0], [0, cos(c), -sin(c)], [0, sin(c), cos(c)] ])
        self.rot_matrix = yaw.dot(pitch).dot(roll)

    def set_translation(self, x, y, z):
        self.translation = np.array([x, y, z])

    def set_scale(self, scale):
        self.scale = scale

    def apply_transformation(self, pt):
        pt = self.scale_pt(pt)
        pt = self.rotate_pt(pt)
        pt = self.translate_pt(pt)
        return pt

    def handle_input(self):
        # Handle rotation input
        keys = pygame.key.get_pressed()
        changed = False
        if keys[pygame.K_a]: self.rotation[0] += 0.01; changed = True
        if keys[pygame.K_d]: self.rotation[0] -= 0.01; changed = True
        if keys[pygame.K_q]: self.rotation[1] += 0.01; changed = True
        if keys[pygame.K_e]: self.rotation[1] -= 0.01; changed = True
        if keys[pygame.K_w]: self.rotation[2] += 0.01; changed = True
        if keys[pygame.K_s]: self.rotation[2] -= 0.01; changed = True
        if changed: self.update_rot_matrix()

        # Handle movement (drag) input
        mouse_click = pygame.mouse.get_pressed()[0]
        mouse_pos = pygame.mouse.get_pos()
        if mouse_click and self.mouse_click_prev:
            translation = np.array( [ curr - prev for curr, prev in zip(mouse_pos, self.mouse_pos_prev) ] + [0] )
            self.translation += translation
        self.mouse_pos_prev = mouse_pos
        self.mouse_click_prev = mouse_click

    def rotate_pt(self, pt):
        return self.rot_matrix.dot(pt)

    def scale_pt(self, pt):
        return self.scale * pt

    def translate_pt(self, pt):
        return pt + self.translation
