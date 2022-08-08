# Shadows of the Knight

Game link: https://www.codingame.com/training/medium/shadows-of-the-knight-episode-1

This is a coding game exercice, The program pilot "batman", looking for a bomb in a building. on each step we choose a window and get told if we are "COLDER" or "WARMER".

- The search area is a polygon that is cut on each step. 
- each step the search area must be cut in half, to do that the polygon center is calculated, then a point is chosen on the circle who's center is the polygon center and radius the distance batman-center, this guarantee that the search area is cut in half.

- every 6 jump, batman jump on the center of the polygon to help keeping a good polygon shape and avoid very narrow bands that gets cut into the decimals.

- when the search area is small enough (5x5), an array of possible positions is evaluated against the history or jump to pinpoint the final location, this step is required because the polygons verteces and edges are floating point values, causing extra steps to converge in most cases. 


# Result

Tkinter app image:

![TKinter](https://github.com/Tiitan/CodingGame/blob/master/ShadowsOfTheKnight/Resources/Tkinter.png)