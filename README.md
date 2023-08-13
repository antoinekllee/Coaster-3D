# Bezier Roller Coaster Construction Mechanism 🎢

This Unity and C# project showcases the mathematical principles behind roller coaster construction, inspired by Planet Coaster. It provides an exploration of Bezier curves in 3D, including techniques like De Casteljau's Algorithm, Bernstein Polynomial Form, Tangent Calculation, and Continuity (C0, C1, C2).

## Overview 📝

The project investigates:
- **Bezier Curves**: Used to construct roller coaster tracks utilizing cubic splines
- **De Casteljau's Algorithm**: Basis for evaluating Bezier curves
- **Tangent Orientation**: Keeps the coaster cart perpendicular to the track
- **Continuity**: Chaining cubic splines for smooth coaster designs

## Features 🚀

1. **Bezier Curve Calculation**: Creates each discrete Bezier curve using four waypoints for track construction
2. **Mesh Generation**: Visualizes the track by generating a 3D mesh from Bezier points
3. **Coaster Cart Movement**: A cart moving along the track, oriented to stay perpendicular to the track
4. **Gizmos**: Visual representation of waypoints and the Bezier curve in the Unity editor

### Prerequisites 🛠

- Unity Editor (Version: 2022.3.2f1)

## License 📜

This project is licensed under the MIT License. See the LICENSE file for details.