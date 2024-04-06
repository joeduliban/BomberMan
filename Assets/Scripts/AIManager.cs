using System;
using UnityEngine;

public class AIManager : GameManager
{

    override protected void Init(GridManager grid = null)
    {
        if (grid != null)
            board = grid;
    }

    override protected void eventAction(){}

    override protected void ApplyAction(Action action)
    {
        switch (action)
        {
            case Action.Up:
                MovePlayer(0);
                break;
            case Action.Down:
                MovePlayer(1);
                break;
            case Action.Right:
                MovePlayer(2);
                break;
            case Action.Left:
                MovePlayer(3);
                break;
            case Action.PlaceBomb:
                BomberPlayer();
                break;
        }
    }

    public Action GetAIAction()
    {
        int bestScore = int.MinValue;
        Action bestAction = Action.Up;

        foreach (Action action in Enum.GetValues(typeof(Action)))
        {
            int score = Minimax(action, 3, false); // Profondeur de recherche 3 pour cet exemple
            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        return bestAction;
    }

    // Implémentation récursive de l'algorithme Minimax
    private int Minimax(Action action, int depth, bool isMaximizingPlayer)
    {
        if (depth == 0 || isGameOver())
        {
            return EvaluateBoard();
        }

        ApplyAction(action); // Appliquer l'action sur le plateau de jeu

        if (isMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Action nextAction in Enum.GetValues(typeof(Action)))
            {
                maxEval = Math.Max(maxEval, Minimax(nextAction, depth - 1, false));
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Action nextAction in Enum.GetValues(typeof(Action)))
            {
                minEval = Math.Min(minEval, Minimax(nextAction, depth - 1, true));
            }
            return minEval;
        }
    }

    // Méthode pour évaluer l'état actuel du plateau de jeu
    private int EvaluateBoard()
    {
        int score = 0;

        // Parcours du plateau de jeu pour évaluer différentes conditions
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject go = board.GetAtPosition(new Vector2(i, j));
                if (go.tag.Equals("Wall"))
                    score -= 1;
                else
                    for (int k = 0; k < go.transform.childCount; k++)
                    {
                        if (go.transform.GetChild(k).tag.Equals("Cherrie"))
                            score += 20;
                        else if (go.transform.GetChild(k).tag.Equals("Player"))
                            score += 10;
                        else if (go.transform.GetChild(k).tag.Equals("Box"))
                            score -= 1;
                        else if (go.transform.GetChild(k).tag.Equals("Bombe"))
                            score -= 10;
                    }
            }
        }

        return score;
    }
}