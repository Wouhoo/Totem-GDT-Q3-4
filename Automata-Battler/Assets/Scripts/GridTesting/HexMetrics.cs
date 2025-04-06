using UnityEngine;

//Holds the conversion factors in a static class for distances betwwen vertices etc
public static class HexMetrics {

	public const float outerRadius = 10f;

    //supposed to be sqrt(3)/2 but this is approx
	public const float innerRadius = outerRadius * 0.866025404f;

    //relative positions of the corners
    //flat side on top, on the xz plane
    public static Vector3[] corners = {
        new Vector3(0.5f*outerRadius, 0f, innerRadius), //top right corner
        new Vector3(outerRadius, 0f, 0f),
        new Vector3(0.5f*outerRadius, 0f, -innerRadius),
        new Vector3(-0.5f*outerRadius, 0f, -innerRadius),
        new Vector3(-outerRadius, 0f, 0f),
        new Vector3(-0.5f*outerRadius, 0f, innerRadius),

        //this is for a pointy side up
		// new Vector3(0f, 0f, outerRadius), //top corner
		// new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		// new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		// new Vector3(0f, 0f, -outerRadius),
		// new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		// new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
	};
}