using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FaceMapGenerator : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int defaultFaceCount = 14;
    [SerializeField] private float radius = 15f;

    private FaceGrid _faceGrid;

    public bool IsInitialized => _faceGrid != null;

    private void Awake()
    {
        lineRenderer ??= GetComponent<LineRenderer>();
        ConfigureLineRenderer();
        InitializeBoard(defaultFaceCount);
    }

    public void Init(int gon)
    {
        InitializeBoard(gon);
    }

    public void InitializeBoard(int faceCount)
    {
        if (faceCount < 4)
        {
            Debug.LogWarning("FaceMapGenerator requires at least 4 faces.");
            ClearBoard();
            return;
        }

        if (faceCount % 2 != 0)
        {
            Debug.LogWarning("FaceMapGenerator requires an even face count for opposite-side movement.");
            ClearBoard();
            return;
        }

        _faceGrid = new FaceGrid(faceCount, transform.position, radius);
        DrawBoard();
    }

    public int GetFaceCount()
    {
        if (!TryEnsureBoard(out _))
        {
            return 0;
        }

        return _faceGrid.FaceCount;
    }

    public int GetWrappedFaceIndex(int faceIndex)
    {
        return TryEnsureBoard(out _) ? _faceGrid.Wrap(faceIndex) : 0;
    }

    public int GetLeftFaceIndex(int currentFaceIndex)
    {
        return TryEnsureBoard(out _) ? _faceGrid.GetAdjacentIndex(currentFaceIndex, -1) : 0;
    }

    public int GetRightFaceIndex(int currentFaceIndex)
    {
        return TryEnsureBoard(out _) ? _faceGrid.GetAdjacentIndex(currentFaceIndex, 1) : 0;
    }

    public int GetOppositeFaceIndex(int currentFaceIndex)
    {
        return TryEnsureBoard(out _) ? _faceGrid.GetOppositeIndex(currentFaceIndex) : 0;
    }

    public Vector3 GetBoardCenterWorldPosition()
    {
        if (!TryEnsureBoard(out _))
        {
            return transform.position;
        }

        return new Vector3(_faceGrid.Center.x, _faceGrid.Center.y, transform.position.z);
    }

    public bool TryGetFaceWorldPose(int faceIndex, out Vector3 worldPosition, out float worldRotationDegrees)
    {
        if (!TryEnsureBoard(out worldPosition))
        {
            worldRotationDegrees = 0f;
            return false;
        }

        FaceData face = _faceGrid.GetFace(faceIndex);
        worldPosition = new Vector3(face.MidPoint.x, face.MidPoint.y, transform.position.z);
        worldRotationDegrees = face.AngleRad * Mathf.Rad2Deg;
        return true;
    }

    public bool TryGetFaceWorldEdge(int faceIndex, out Vector3 startWorldPosition, out Vector3 endWorldPosition)
    {
        startWorldPosition = transform.position;
        endWorldPosition = transform.position;

        if (!TryEnsureBoard(out _))
        {
            return false;
        }

        FaceData face = _faceGrid.GetFace(faceIndex);
        startWorldPosition = new Vector3(face.Start.x, face.Start.y, transform.position.z);
        endWorldPosition = new Vector3(face.End.x, face.End.y, transform.position.z);
        return true;
    }

    private void ConfigureLineRenderer()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
    }

    private void DrawBoard()
    {
        int faceCount = _faceGrid.FaceCount;
        lineRenderer.positionCount = faceCount;

        for (int i = 0; i < faceCount; i++)
        {
            FaceData face = _faceGrid.GetFace(i);
            lineRenderer.SetPosition(i, new Vector3(face.Start.x, face.Start.y, transform.position.z));
        }
    }

    private void ClearBoard()
    {
        _faceGrid = null;
        lineRenderer.positionCount = 0;
    }

    private bool TryEnsureBoard(out Vector3 fallbackPosition)
    {
        fallbackPosition = transform.position;

        if (_faceGrid != null)
        {
            return true;
        }

        Debug.LogWarning("FaceMapGenerator has not been initialized.");
        return false;
    }
}
