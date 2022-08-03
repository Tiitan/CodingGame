import sys
from typing import Optional, List

from numpy import ndarray, array, dot, vstack, hstack, cross, ones, roll, clip
from numpy.linalg import norm
from pandas import Series, DataFrame


class IDebugCanvas:
    def draw_line(self, p0: ndarray, p1: ndarray, color: str):
        pass


class Geometry:
    @staticmethod
    def intersect_segment(hlo: ndarray, hld: ndarray, s1: ndarray, s2: ndarray, full_line: bool = False) -> Optional[ndarray]:
        """
        intersect line or half-line with segment.
        line intersection:https://stackoverflow.com/questions/3252194/numpy-and-line-intersections
        segment https://stackoverflow.com/questions/328107/how-can-you-determine-a-point-is-between-two-other-points-on-a-line-segment
        hlo: [x, y] half-line origin
        hld: [x, y] half-line direction
        s1: [x, y] segment point 1
        s2: [x, y] segment point 2
        test_full_line: check half-line intesect if false, ignoring any intersection before hlo
        """

        # Get line Intersection. (no idea how that part works)
        s = vstack([hlo, hld, s1, s2])
        h = hstack((s, ones((4, 1))))
        l1 = cross(h[0], h[1])
        l2 = cross(h[2], h[3])
        x, y, z = cross(l1, l2)
        if z == 0:
            return None  # Parallel
        i = array([x/z, y/z])

        # Check if the intersection point is at a valid position.
        sv = s1 - s2
        c = dot(sv, s1 - i)
        if c < 0 or c > dot(sv, sv):
            return None  # outside segment
        if not full_line and dot(hld-hlo, i - hlo) < 0:
            return None  # Wrong half-line side
        return i

    @staticmethod
    def intersect_polygon(polygon: Series, hlo: ndarray, hld: ndarray, full_line: bool = False) -> Series:
        return DataFrame({'a': polygon, 'b': roll(polygon, 1)}).apply(
            lambda s: Geometry.intersect_segment(hlo, hld, s['a'], s['b'], full_line), axis=1, result_type='reduce').dropna()


class ShadowsOfTheKnight2:
    @staticmethod
    def point_side(ab: ndarray, ap: ndarray, bomb_dir) -> bool:
        angle = dot(ab, ap)
        if bomb_dir == "COLDER":
            return angle < 0
        if bomb_dir == "WARMER":
            return angle > 0
        if bomb_dir == "SAME":
            return False

    # Get new position (polygon opposite side))
    def calculate_new_position(self) -> ndarray:
        polygon_center = self.search_polygon.mean()
        direction = polygon_center - self.batman_position
        perpendicular = array([-direction[1], direction[0]])
        intersection_vector = direction + perpendicular + polygon_center
        segments_intersect = Geometry.intersect_polygon(self.search_polygon, hlo=polygon_center, hld=intersection_vector).values[0]

        segments_intersect[0] = min(max(0, segments_intersect[0]), self.width)
        segments_intersect[1] = min(max(0, segments_intersect[1]), self.height)

        self.debug.draw_line(polygon_center, intersection_vector, "purple")
        self.debug.draw_line(polygon_center, segments_intersect, "orange")

        return segments_intersect

    def cut_search_polygon(self, bomb_dir: str):
        center_position = (self.prev_position + self.batman_position) / 2
        batman_direction = self.batman_position - center_position
        perpendicular = array([-batman_direction[1], batman_direction[0]])
        split_dir = center_position + perpendicular

        if bomb_dir == "SAME":
            line = Geometry.intersect_polygon(self.search_polygon, center_position, split_dir, True).tolist()
            dir_half_norm = batman_direction / norm(batman_direction) / 2
            self.search_polygon = Series([line[0] + dir_half_norm, line[1] + dir_half_norm, line[1] - dir_half_norm, line[0] - dir_half_norm])
            return

        points = self.search_polygon.to_list()
        new_polygon = []
        prev_side = self.point_side(batman_direction, points[len(points) - 1] - center_position, bomb_dir)
        for i in range(len(points)):
            side = self.point_side(batman_direction, points[i] - center_position, bomb_dir)
            if prev_side is not None and prev_side != side:
                prev_index = i - 1 if i > 0 else len(points) - 1
                new_polygon.append(Geometry.intersect_segment(center_position, split_dir, points[prev_index], points[i], True))
            if side:
                new_polygon.append(points[i])
            prev_side = side
        self.search_polygon = Series(new_polygon)

    def step(self):
        self.prev_position = self.batman_position
        self.batman_position = self.calculate_new_position()
        print(f"{int(self.batman_position[0])} {int(self.batman_position[1])}")
        self.cut_search_polygon(input())

    def run(self):
        while True:
            self.step()

    def __init__(self):
        self.width, self.height = [int(i) - 1 for i in input().split()]
        _ = int(input())  # maximum number of turns before game over.
        x0, y0 = [int(i) for i in input().split()]

        self.debug = IDebugCanvas()
        self.prev_position = None
        self.batman_position = array([x0, y0])
        self.search_polygon = Series([array([0, 0]), array([self.width, 0]), array([self.width, self.height]), array([0, self.height])])
        self.split_segment = None
        input()  # ignore first unknown


if __name__ == "__main__":
    exercise = ShadowsOfTheKnight2()
    exercise.run()
