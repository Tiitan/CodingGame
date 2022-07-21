from math import dist
from typing import List

# Character used for human, Ash ("player") and zombies
class Character:
    id: int
    x: int
    y: int

    @property
    def position(self) -> List[int]:
        return [self.x, self.y]

    def __init__(self, id: int, x: int, y: int):
        self.id = id
        self.x = x
        self.y = y


# Get the input human distance to its closest zombie.
def get_zombie_distance(human: Character, zombies: List[Character]) -> float:
    zombie_distance = float('inf')
    for zombie in zombies:
        new_distance = dist(zombie.position, human.position)
        if new_distance < zombie_distance:
            zombie_distance = new_distance
    return zombie_distance


# Is there time to reach the human before the zombie catch him.
def is_savable(player_distance: float, zombie_distance: float) -> bool:
    return zombie_distance * 2.5 > player_distance - 2000


# Pick the human closest to a zombie that can still be saved.
def choose_human(player: Character, humans: List[Character], zombies: List[Character]) -> Character:
    chosen_human = humans[0]
    closest_zombie_distance = float('inf')
    for human in humans:
        player_distance = dist(human.position, player.position)
        zombie_distance = get_zombie_distance(human, zombies)
        if zombie_distance < closest_zombie_distance and is_savable(player_distance, zombie_distance):
            chosen_human = human
            closest_zombie_distance = zombie_distance
    return chosen_human


# game loop: each turn, read all characters position then select a human to help.
while True:
    x, y = [int(i) for i in input().split()]
    player = Character(-1, x, y)
    humans = [Character(int(c[0]), int(c[1]), int(c[2])) for c in [input().split() for _ in range(int(input()))]]
    zombies = [Character(int(c[0]), int(c[1]), int(c[2])) for c in [input().split() for _ in range(int(input()))]]

    human = choose_human(player, humans, zombies)
    print(f"{human.x} {human.y}")