using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public Color[] playersColor;
    public List<Player> players;
    private int currentPlayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            MovePlayer(Vector3.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MovePlayer(Vector3.down);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MovePlayer(Vector3.right);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MovePlayer(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            BomberPlayer();
        }
    }

    private void MovePlayer(Vector3 direction)
    {
        Debug.Log(direction);
    }

    private void BomberPlayer()
    {
        Debug.Log("Bombe");
    }

    private void ChangeCurrentPlayer()
    {
        currentPlayer = (currentPlayer + 1) % players.Count;
    }

    public void AddPlayer(Player player)
    {
        player.GetComponent<SpriteRenderer>().color = playersColor[players.Count];
        players.Add(player);
    }
}
