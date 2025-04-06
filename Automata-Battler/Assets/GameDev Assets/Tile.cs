using UnityEngine;

public class Tile : MonoBehaviour
{
    private Vector3Int _position;

    public void Set_Position(Vector3Int position)
    {
        _position = position;
    }
    public Vector3Int Get_Position()
    {
        return _position;
    }
}
