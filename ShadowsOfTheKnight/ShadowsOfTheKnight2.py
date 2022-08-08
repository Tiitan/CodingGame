import sys
from typing import Optional, Callable, List

from numpy import ndarray, array, dot, vstack, hstack, cross, ones, roll, pi, cos, sin
from numpy.linalg import norm
from pandas import Series, DataFrame


class IDebugCanvas:
    def draw_line(self, p0: ndarray, p1: ndarray, color: str):
        pass

    def draw_point(self, p: ndarray, color: str):
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

    @staticmethod
    def polygon_center(polygon: Series) -> ndarray:
        df = DataFrame({'point': polygon, 'next': roll(polygon, 1), 'prev': roll(polygon, -1)})
        weights = df.apply(lambda d: norm(d['point'] - d['next']) + norm(d['point'] - d['prev']), axis=1)
        return (df['point'] * weights).sum() / weights.sum()


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

    def evaluate_position(self, position: ndarray) -> bool:
        if not (0 <= position[0] <= self.width and 0 <= position[1] <= self.height):
            return False

        last_position = self.initial_position
        for jump_pos, jump_bomb_dir in self.jump_history:
            if jump_pos[0] == position[0] and jump_pos[1] == position[1]:
                return False
            prev_distance = norm(last_position - position)
            distance = norm(jump_pos - position)
            if jump_bomb_dir == "COLDER" and prev_distance > distance:
                return False
            if jump_bomb_dir == "WARMER" and prev_distance < distance:
                return False
            if jump_bomb_dir == "SAME" and abs(prev_distance - distance) > 0.1:
                return False
            last_position = jump_pos
        return True

    def build_final_target(self, center: ndarray):
        self.final_targets = []
        target = center.astype(int)
        array_root = array([target[0] - 2, target[1] - 2])
        for i in range(5):
            for j in range(5):
                t = array([array_root[0] + i, array_root[1] + j])
                if self.evaluate_position(t):
                    self.final_targets.append(t)
        print(f"Built final target. count={len(self.final_targets)}, center={center}", file=sys.stderr)

    def update_final_target(self):
        self.final_targets = [t for t in self.final_targets if self.evaluate_position(t)]
        print(f"updated final target. count={len(self.final_targets)}", file=sys.stderr)

    def update_target(self, center: ndarray):
        if not self.final_targets:
            self.build_final_target(center)
        else:
            self.update_final_target()

        for target in self.final_targets:
            self.debug.draw_point(target, "cyan")

    # Get new position (polygon opposite side))
    def calculate_new_position(self) -> ndarray:
        center = Geometry.polygon_center(self.search_polygon)
        has_converged = len(self.final_targets) > 0 or not self.search_polygon.apply(lambda x: norm(center-x) > 2).any()
        if self.step_left <= 3 or has_converged:
            self.update_target(center)
        if has_converged and len(self.final_targets) <= 3:
            return self.final_targets[0]

        # create a circle around the search area
        distance = norm(center - self.batman_position)
        if not self.step_left % 6:
            distance = 2
        circle_points = 8
        circle_slice = 2 * pi / circle_points
        points = []
        for i in range(circle_points):
            angle = circle_slice * i
            point = array([center[0] + distance * cos(angle), center[1] + distance * sin(angle)])
            if 0 <= point[0] <= self.width and 0 <= point[1] <= self.height:
                points.append(point)

        # jump on circle (halven search area)
        if len(points) < 2:
            return center.astype(int)
        points = sorted(points, key=lambda x: norm(x - self.batman_position))

        if len(points) == circle_points:
            target = points[int(len(points) - 2)].astype(int)
        else:
            target = points[int(len(points) - 1)].astype(int)

        self.debug.draw_line(center, target, "purple")
        return target

    def cut_search_polygon(self, bomb_dir: str):
        center_position = (self.prev_position + self.batman_position) / 2
        batman_direction = self.batman_position - center_position
        perpendicular = array([-batman_direction[1], batman_direction[0]])
        split_dir = center_position + perpendicular

        if bomb_dir == "SAME":
            line = Geometry.intersect_polygon(self.search_polygon, center_position, split_dir, True).tolist()
            if len(line) == 2:
                dir_half_norm = batman_direction / norm(batman_direction) / 2
                self.search_polygon = Series([line[0] + dir_half_norm, line[1] + dir_half_norm, line[1] - dir_half_norm, line[0] - dir_half_norm])
            return

        points = self.search_polygon.to_list()
        new_polygon = []
        prev_side = self.point_side(batman_direction, points[len(points) - 1] - center_position, bomb_dir)
        for i in range(len(points)):
            side = self.point_side(batman_direction, points[i] - center_position, bomb_dir)
            if prev_side != side:
                prev_index = i - 1 if i > 0 else len(points) - 1
                new_polygon.append(Geometry.intersect_segment(center_position, split_dir, points[prev_index], points[i], True))
            if side:
                new_polygon.append(points[i])
            prev_side = side
        self.search_polygon = Series(new_polygon)

    def step(self):
        self.prev_position = self.batman_position
        self.batman_position = self.calculate_new_position()

        print(f"{self.batman_position[0]} {self.batman_position[1]}")

        bomb_dir = self.get_input()
        print(bomb_dir, file=sys.stderr)

        self.jump_history.append((self.batman_position, bomb_dir))
        if not len(self.final_targets):
            self.cut_search_polygon(bomb_dir)
        self.step_left -= 1
        self.turn += 1
        self.debug.draw_line(self.prev_position, self.batman_position, "red")
        self.debug.draw_line(self.search_polygon.mean(), self.batman_position, "black")

    def run(self):
        while True:
            self.step()

    def __init__(self, input_callback: Callable[[], str]):
        self.get_input = input_callback
        self.width, self.height = [int(i) - 1 for i in self.get_input().split()]
        print(f"width:{self.width}, height:{self.height}", file=sys.stderr)

        self.step_left = int(self.get_input())  # maximum number of turns before game over.
        self.turn = 0
        x0, y0 = [int(i) for i in self.get_input().split()]

        self.debug = IDebugCanvas()
        self.prev_position = None
        self.initial_position = array([x0, y0])
        self.batman_position = self.initial_position
        self.search_polygon = Series([array([0, 0]), array([self.width, 0]), array([self.width, self.height]), array([0, self.height])])
        self.split_segment = None
        self.get_input()  # ignore first unknown
        self.jump_history: List[(ndarray, str)] = []
        self.final_targets = []


if __name__ == "__main__":
    exercise = ShadowsOfTheKnight2(input)
    exercise.run()
