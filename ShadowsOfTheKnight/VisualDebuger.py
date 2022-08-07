from tkinter import Tk, Canvas, Button, RIGHT, LEFT
from typing import List

from numpy import array, ndarray, concatenate
from numpy.linalg import norm
from pandas import Series

from ShadowsOfTheKnight2 import ShadowsOfTheKnight2, IDebugCanvas


class GameCanvas(IDebugCanvas):
    def __init__(self, parent, exercise: ShadowsOfTheKnight2, width, height):
        self._width = width
        self._height = height
        self._scale = 400 / exercise.height
        self._offset = array([width / 4, height / 4])
        self._canvas = Canvas(parent, width=width, height=height, background="grey")
        self._canvas.pack(side=LEFT)
        self._exercise = exercise

        self._canvas.bind("<ButtonPress-1>", self._scroll_start)
        self._canvas.bind("<B1-Motion>", self._scroll_move)
        self._canvas.bind("<MouseWheel>", self._zoomer)

        # draw polygon
        self._polygon_id = self._canvas.create_polygon(self._polygon_series_to_canvas(exercise.search_polygon), fill="cyan")

        # draw batman position
        p = self._point_to_canvas(exercise.batman_position)
        hs = self._scale / 2
        self._previous_id = self._canvas.create_oval(p[0] - hs, p[1] - hs, p[0] + hs, p[1] + hs, fill="red")
        self._batman_id = self._canvas.create_oval(p[0] - hs, p[1] - hs, p[0] + hs, p[1] + hs, fill="black")

        # target
        t = self._point_to_canvas(target)
        self._target_id = self._canvas.create_oval(t[0] - hs, t[1] - hs, t[0] + hs, t[1] + hs, fill="yellow")

    def _scroll_start(self, event):
        self._canvas.scan_mark(event.x, event.y)

    def _scroll_move(self, event):
        self._canvas.scan_dragto(event.x, event.y, gain=1)

    def _zoomer(self, event):
        if event.delta > 0:
            self._canvas.scale("all", event.x, event.y, 1.1, 1.1)
        elif event.delta < 0:
            self._canvas.scale("all", event.x, event.y, 0.9, 0.9)
        #self._canvas.configure(scrollregion=self._canvas.bbox("all"))

    def _point_to_canvas(self, position: ndarray) -> ndarray:
        return position * self._scale + self._offset

    def _polygon_series_to_canvas(self, polygon_series: Series) -> List[float]:
        polygon = polygon_series.apply(self._point_to_canvas)
        return list(concatenate(polygon).ravel())

    def update(self):
        hs = self._scale / 2
        p = self._point_to_canvas(self._exercise.prev_position)
        self._canvas.moveto(self._previous_id, p[0] - hs, p[1] - hs)
        p = self._point_to_canvas(self._exercise.batman_position)
        self._canvas.moveto(self._batman_id, p[0] - hs, p[1] - hs)

        if self._polygon_id:
            self._canvas.delete(self._polygon_id)
            self._polygon_id = None
        if not len(self._exercise.final_targets):
            self._polygon_id = self._canvas.create_polygon(self._polygon_series_to_canvas(self._exercise.search_polygon), fill="cyan")

        self._canvas.tag_raise("debug")
        self._canvas.tkraise(self._batman_id)

    def draw_line(self, p0: ndarray, p1: ndarray, color: str):
        p0 = self._point_to_canvas(p0)
        p1 = self._point_to_canvas(p1)
        self._canvas.create_line(p0[0], p0[1], p1[0], p1[1], fill=color, tags="debug")

    def clear_debug(self):
        self._canvas.delete("debug")


def step(game_canvas: GameCanvas, exercise:  ShadowsOfTheKnight2):
    game_canvas.clear_debug()
    exercise.step()
    game_canvas.update()


exercise: ShadowsOfTheKnight2
target = array([4, 10])


def get_input() -> str:
    try:
        return input()
    except EOFError:
        current_distance = norm(target - exercise.batman_position)
        previous_distance = norm(target - exercise.prev_position)
        if current_distance > previous_distance:
            return "COLDER"
        elif current_distance < previous_distance:
            return "WARMER"
        return "SAME"


def main():
    window = Tk()
    window.title('Visual debugger')
    window_width = 800
    window_height = 650
    position_right = int(window.winfo_screenwidth()/2 - window_width/2)
    position_down = int(window.winfo_screenheight()/2 - window_height/2)

    window.geometry(f"{window_width}x{window_height}+{position_right}+{position_down}")

    global exercise
    exercise = ShadowsOfTheKnight2(get_input)
    canvas = GameCanvas(window, exercise, width=window_width - 50, height=window_height)
    exercise.debug = canvas
    button = Button(window, text="step", command=lambda: step(canvas, exercise))
    button.pack(side=RIGHT, anchor="se")
    window.mainloop()


if __name__ == "__main__":
    main()
