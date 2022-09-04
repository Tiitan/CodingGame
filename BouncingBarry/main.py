import sys
import numpy as np

from dataclasses import dataclass


@dataclass
class PathNode:
    d: str  # direction Y X
    b: int  # bounce

    def __str__(self):
        return f"({self.d}, {self.b})"


@dataclass
class Vector2i:
    x: int
    y: int

    def __str__(self):
        return f"({self.x}, {self.y})"


def main():
    path = [PathNode("X" if d in ["E", "W"] else "Y", b if d in ["S", "E"] else -b) for d, b in zip(input().split(" "), [int(x) for x in input().split(" ")])]

    # pre-process
    changed = True
    while changed:
        changed = False
        i = 0
        count = len(path) - 1
        while i < count:
            if path[i].d == path[i + 1].d:  # collapse
                path[i].b += path[i + 1].b
                del path[i + 1]
                count -= 1
                changed = True
            elif path[i].b == 0:  # remove 0
                del path[i]
                i -= 1
                count -= 1
                changed = True
            i += 1

    # Get dimensions
    print("----dimensions----", file=sys.stderr, flush=True)
    minimum = Vector2i(0, 0)
    maximum = Vector2i(0, 0)
    current = Vector2i(0, 0)
    for node in path:
        if node.d == "Y":
            current.y += node.b
            if current.y < minimum.y:
                minimum.y = current.y
            elif current.y > maximum.y:
                maximum.y = current.y
        elif node.d == "X":
            current.x += node.b
            if current.x < minimum.x:
                minimum.x = current.x
            elif current.x > maximum.x:
                maximum.x = current.x
        print(f"{node}: {minimum} < {current} < {maximum}", file=sys.stderr, flush=True)

    dimension = Vector2i(-minimum.x + maximum.x + 1, -minimum.y + maximum.y + 1)
    current = Vector2i(-minimum.x, -minimum.y)
    print(f"current={current} dimension={dimension}", file=sys.stderr, flush=True)

    # Bounce !
    print("----Bounce----", file=sys.stderr, flush=True)
    path[0].b += -1 if path[0].b > 0 else 1
    floor = np.zeros((dimension.y, dimension.x), dtype=bool)
    floor[current.y][current.x] = True
    for node in path:
        if node.d == "Y":
            masked_floor = floor[current.y + node.b: current.y, current.x] if node.b < 0 else floor[current.y + 1: current.y + node.b + 1, current.x]
            np.invert(masked_floor, out=masked_floor)
            current.y += node.b
        elif node.d == "X":
            masked_floor = floor[current.y, current.x + node.b: current.x] if node.b < 0 else floor[current.y, current.x + 1: current.x + node.b + 1]
            np.invert(masked_floor, out=masked_floor)
            current.x += node.b

    # Crop
    print("----Crop----", file=sys.stderr, flush=True)
    rows = np.flatnonzero(floor.sum(axis=1))
    cols = np.flatnonzero(floor.sum(axis=0))
    try:
        floor = floor[rows.min():rows.max()+1, cols.min():cols.max()+1]
    except ValueError:  # nothing left after crop
        print(".")
        return

    # Print
    print("----Print----", file=sys.stderr, flush=True)
    for row in floor:
        line = "".join(['#' if c else '.' for c in row])
        print(line)


if __name__ == "__main__":
    main()
