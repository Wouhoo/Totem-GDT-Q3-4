using UnityEngine;

public enum PlayerState
{
    Playing,
    Viewing,
    Transitioning,
    Acting,
    WatchingGame,
}

public enum PlayerCameraState
{
    ViewingBoard,
    ViewingHand,
    ViewingActions
}

public enum PlayerRequestState
{
    None,
    Tiles,
    Tiles_ValidEmpty,
    Cards_InHand,
    Cards_InPlay
}
