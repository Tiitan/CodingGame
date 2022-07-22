width, height = [int(i) for i in input().split()] # Building size
n = int(input())  # maximum number of turns before game over.
x, y = [int(i) for i in input().split()] # batman position

range_x = range(0, width)
range_y = range(0, height)

while True:
    bomb_direction = input()

    if "D" in bomb_direction:
        range_y = range(y + 1, range_y.stop)
    elif "U" in bomb_direction:
        range_y = range(range_y.start,  y - 1)
    else:
        range_y = range(y, y)

    if "R" in bomb_direction:
        range_x = range(x + 1, range_x.stop)
    elif "L" in bomb_direction:
        range_x = range(range_x.start,  x - 1)
    else:
        range_x = range(x, x)

    x = int((range_x.stop - range_x.start) / 2 + range_x.start)
    y = int((range_y.stop - range_y.start) / 2 + range_y.start)
    print(f"{x} {y}")
