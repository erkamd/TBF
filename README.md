# Turn Based Football (TBF)

This project contains a minimal prototype of a turn based football game built with Unity. The game uses a 15x40 grid and spawns two teams of three agents. Each agent has several attributes such as Passing, Shooting and Awareness. A simple dice system is used for actions.

## How to run

1. Open the project in Unity 6 or later.
2. Open the scene `TBFGame` located under `Assets/Scenes`.
3. Press Play to generate the grid and spawn the placeholder agents and ball.

Agents and grid visuals are simple primitives. GameManager automatically loads the agent and ball prefabs from `Resources/Prefabs`.
At the start of each cycle the turn order of all agents is randomized and displayed at the top of the screen using their jersey numbers. Agents take turns one after another regardless of team. When an agent finishes their actions the ball moves one step with its current velocity if it is not possessed by anyone.
Click your active agent to open a minimal menu showing the remaining action points and simple actions.
