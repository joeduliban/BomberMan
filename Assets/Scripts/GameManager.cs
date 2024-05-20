using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GridManager board;
    public Color[] playersColor;
    private List<Player> players;
    private int currentPlayer;

    private Vector2[] direct2D = { Vector2.up, Vector2.down, Vector2.right, Vector2.left };

    void Start()
    {
        players = new List<Player>();
        board.AddPlayer += AddPlayer;
        board.GenerateGrid();
    }

    void Update()
    {
        if (!isGameOver())
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ApplyAction(Action.Up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ApplyAction(Action.Down);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ApplyAction(Action.Right);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ApplyAction(Action.Left);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                ApplyAction(Action.PlaceBomb);
            }
        }
    }

    private void MovePlayer(int dir)
    {
        Player player = CurrentPlayer();
        player.NextParty();
        Vector2 pos2D = new Vector2(player.transform.position.x, player.transform.position.y);
        GameObject square = board.GetAtPosition(pos2D + direct2D[dir]);
        if (!square.tag.Equals("Wall"))
        {
            if (square.transform.childCount == 0)
                player.Move(square.transform, dir);
            else if (square.transform.GetChild(0).tag.Equals("Cherrie"))
            {
                player.Move(square.transform, dir);
                Cherrie cherrie = square.GetComponentInChildren<Cherrie>();
                player.rangeBombe += cherrie.bonusRange;
                Destroy(cherrie.gameObject);
            }
        }
    }

    private void BomberPlayer()
    {
        Bombe bb = CurrentPlayer().Bomber();
        if (bb != null)
            bb.Explosion += exposionBombe;
    }

    private void ApplyAction(Action action, bool ia = false)
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
        if (!ia)
        {
            ChangeCurrentPlayer();
            if (CurrentPlayer().isIA)
            {
                ApplyAction(GetAIAction());
            }
        }
    }

    private Player CurrentPlayer()
    {
        return players[currentPlayer];
    }

    private void ChangeCurrentPlayer()
    {
        currentPlayer = (currentPlayer + 1) % players.Count;
    }

    private void AddPlayer(object sender, EventArgs e)
    {
        Player player = (Player)sender;
        player.GetComponent<SpriteRenderer>().color = playersColor[players.Count];
        players.Add(player);
    }

    private void exposionBombe(object sender, EventArgs e)
    {
        Bombe bombe = (Bombe)sender;
        Vector2 pos2D = new Vector2(bombe.transform.position.x, bombe.transform.position.y);
        foreach (Vector2 direction in direct2D)
        {
            for (int i = 0; i <= bombe.range; i++)
            {
                GameObject go = board.GetAtPosition((i * direction) + pos2D);
                if (!go.tag.Equals("Wall"))
                    for (int j = 0; j < go.transform.childCount; j++)
                    {
                        if (!go.transform.GetChild(j).tag.Equals("Cherrie"))
                        {
                            if (go.transform.GetChild(j).tag.Equals("Player"))
                                players.Remove(go.transform.GetComponentInChildren<Player>());
                            Destroy(go.transform.GetChild(j).gameObject);
                        }

                    }
                else
                    break;
            }
        }
    }

    public Action GetAIAction()
    {
        Action bestAction = Action.Up; // Default action
        int bestValue = int.MinValue;

        var originalBoard = board.board;
        var clonedBoard = board.CloneBoard();

        foreach (Action action in Enum.GetValues(typeof(Action)))
        {
            ApplyAction(action, true);

            int boardValue = Minimax(action, 3, true); // Depth and isMaximizingPlayer might need adjustment

            if (boardValue > bestValue)
            {
                bestValue = boardValue;
                bestAction = action;
            }

            board.board = originalBoard; // Restore original board
        }

        board.CleanupClones(clonedBoard); // Clean up clones

        return bestAction;
    }

    // Implémentation récursive de l'algorithme Minimax
    private int Minimax(Action action, int depth, bool isMaximizingPlayer)
    {
        if (depth == 0 || isGameOver())
        {
            return EvaluateBoard();
        }

        // Cloner le plateau et les joueurs
        var originalBoard = board.board;
        var clonedBoard = board.CloneBoard();

        // Appliquer l'action sur le plateau de jeu cloné
        ApplyAction(action, true);

        int evaluation;
        if (isMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Action nextAction in Enum.GetValues(typeof(Action)))
            {
                maxEval = Math.Max(maxEval, Minimax(nextAction, depth - 1, false));
            }
            evaluation = maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Action nextAction in Enum.GetValues(typeof(Action)))
            {
                minEval = Math.Min(minEval, Minimax(nextAction, depth - 1, true));
            }
            evaluation = minEval;
        }

        // Nettoyer les clones et restaurer le plateau original
        board.board = originalBoard;
        board.CleanupClones(clonedBoard);

        return evaluation;
    }

    // Méthode pour évaluer l'état actuel du plateau de jeu
    private int EvaluateBoard()
    {
        int score = 0;

        foreach (Player player in players)
        {
            score += 100; // Points pour chaque joueur restant
            score += player.rangeBombe * 5; // Bonus pour la portée des bombes

            Vector2 pos2D = new Vector2(player.transform.position.x, player.transform.position.y);
            foreach (Vector2 direction in direct2D)
            {
                Vector2 targetPos = pos2D + direction;
                if (board.GetAtPosition(targetPos) is GameObject square && square.tag.Equals("Cherrie"))
                {
                    score += 20; // Bonus pour la proximité d'une cherrie
                }
            }
        }

        foreach (var kvp in board.board)
        {
            GameObject go = kvp.Value;
            if (go.tag.Equals("Bombe"))
            {
                score -= 10; // Malus pour chaque bombe présente
            }
        }

        return score;
    }

    private bool isGameOver()
    {
        return players.Count <= 1;
    }
}
