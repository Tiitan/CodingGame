# Mars lander README

Game link: https://www.codingame.com/training/expert/mars-lander-episode-3

This is a coding game exercice, The program pilot the lander to land on the flat ground.
steps:
- rasterized vector terrain into a grid
- A* pathfinding on the grid
- Cleaned up the pathfinding to remove intermediate steps when there is a direct line of sight
- Ad-hoc rules to make the lander follow the path 

# Description

This folder contains the main game script: https://github.com/Tiitan/CodingGame/blob/master/MarsLander/MarsLander/MarsLander.cs
It also contains a WPF application to visualize the pathfinding

# Result

WPF app image:

![WPF](https://github.com/Tiitan/CodingGame/blob/master/MarsLander/Resources/WPFAppScreenshoot.png)

Video:

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/QqbNuwMlmI8/0.jpg)](https://www.youtube.com/watch?v=QqbNuwMlmI8)

# Post-mortem
If someone wants to try a similar approch, there are a few things that I wish I did differently and improvements that could be made:
- Create a navmesh from the terrain vectors instead of rasterizing, that would be more efficient and require less work.
- Reprocess the list of segment to make a spline for the path.
- Train a neural network to follow the path! currently the control rules wouldn't work well in a lot of cases
