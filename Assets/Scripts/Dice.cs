using UnityEngine;

public static class Dice
{
    public static int Roll(int sides)
    {
        return Random.Range(1, sides + 1);
    }
}
