using System;
using UnityEngine;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    [SerializeField] public List<Vector3Int> _tiles;

    [SerializeField] private float tileRadius = 1f;

    [SerializeField] private GameObject tile;


    private static float R;
    private static float r;

    void Awake()
    {
        R = tileRadius;
        r = R * Mathf.Sqrt(3) / 2;

        foreach (Vector3Int tilePos in _tiles)
        {
            GameObject newTile = Instantiate(tile, HexToGridCoordinates(tilePos), Quaternion.identity);
            Tile newTileScript = newTile.GetComponent<Tile>();
            newTileScript.Set_Position(tilePos);
            newTile.transform.position += new Vector3(0, -3f, 0);
        }
    }


    public Vector3 HexToGridCoordinates(Vector3Int hexPos)
    {
        Vector3 gridPos = new Vector2(0f, 0f);
        gridPos.x = 4 * R * (hexPos.x + 0.5f * hexPos.z);
        gridPos.y = 0;
        gridPos.z = 2 * r * (hexPos.y + 0.5f * hexPos.z);
        return gridPos;
    }
}
