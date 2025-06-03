using Unity.Netcode;
using UnityEngine;

public class WouterTestButton : NetworkBehaviour
{
    [SerializeField] HexCell testCell;
    [SerializeField] Card testCard;
    [SerializeField] Board board;

    public void SayHello()
    {
        Debug.Log("TEST BUTTON PRESSED");
    }

    public void WouterTestSetCard()
    {
        // Sets occupant of given test cell
        //testCell.SetCard(testCard);
        // Check if both client and server board have correctly received the update
        // I don't wanna bother making HexCoordinates serializable (yet), so this is fixed to coordinate (0, 0, 0) for now
        PrintOccupantRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PrintOccupantRpc()
    {
        HexCoordinates coordinates = new HexCoordinates(0, 0);
        Debug.Log(board.cells[coordinates].GetCard());
    }
}
