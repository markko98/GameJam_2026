using UnityEngine;

public static class CameraVisibilityUtility
{
    public static bool IsVisibleFrom(Renderer renderer, Camera camera)
    {
        if (renderer == null || camera == null) return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}