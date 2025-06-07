using UnityEngine;

public enum PlayerState
{
    Playing,
    Viewing,
    Transitioning,
    WatchingGame,
}

public enum PlayerCameraState
{
    ViewingBoard,
    ViewingHand
}

public enum PlayerRequestState
{
    None,
    Tiles,
    Tiles_ValidEmpty,
    Cards_InHand,
    Cards_InPlay,
    RotationArrows
}
