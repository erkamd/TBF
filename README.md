# Turn Based Football (TBF)

This project contains a minimal prototype of a turn based football game built with Unity. The game uses a 15x40 grid and spawns two teams of three agents. Each agent has several attributes such as Passing, Shooting and Awareness. A simple dice system is used for actions.

## How to run

1. Open the project in Unity 6 or later.
2. Open the scene `TBFGame` located under `Assets/Scenes`.
3. Press Play to generate the grid and spawn the placeholder agents.

Agents and grid visuals are simple primitives. GameManager automatically loads the agent prefab from `Resources/Prefabs/Agent`.
