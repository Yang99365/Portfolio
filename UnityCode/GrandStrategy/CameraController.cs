using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : InitBase
{
    private Transform followTransform;
    private Vector3 newPosition;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;

    
     private float zoomMin = 3f;
     private float zoomMax = 8f;
     private float zoomSpeed = 1f;
     private float moveSpeed = 20f;
     private float dragSpeed = 2f;
     private float edgeScrollSize = 50f;

    // 카메라 Z 위치 상수 추가
    private const float CAMERA_Z = -10f;

    private bool enableKeyboardMovement = true;
    private bool enableEdgeScrolling = true;
    private bool enableDragMovement = true;
    private bool enableZoom = true;

    private Camera mainCamera;
    private float currentZoom;


    private float boundsPadding = 1f;
    private float minX, maxX, minY, maxY;
    private bool boundsInitialized = false;
    private Vector2 mapSize;
    private Vector2 mapCenter;
    private float aspectRatio;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        mainCamera = Camera.main;
        currentZoom = mainCamera.orthographicSize;

        // 초기 위치 설정 시 Z 값 고정
        transform.position = new Vector3(transform.position.x, transform.position.y, CAMERA_Z);
        newPosition = transform.position;

        InitializeBounds();

        return true;
    }

    private void Update()
    {
        if (!_init) return;

        // Follow target if set
        if (followTransform != null)
        {
            transform.position = ClampPosition(new Vector3(
                followTransform.position.x,
                followTransform.position.y,
                CAMERA_Z
            ));
            return;
        }

        HandleCameraMovement();
        HandleCameraZoom();
    }
    private void InitializeBounds()
    {
        if (Managers.Map.BattleMap != null)
        {
            // Tilemap이나 SpriteRenderer의 bounds를 가져옴
            Renderer mapRenderer = Managers.Map.BattleMap.GetComponentInChildren<Renderer>();
            if (mapRenderer != null)
            {
                Bounds mapBounds = mapRenderer.bounds;

                mapSize = new Vector2(mapBounds.size.x, mapBounds.size.y);
                mapCenter = new Vector2(mapBounds.center.x, mapBounds.center.y);
                aspectRatio = (float)Screen.width / Screen.height;

                CalculateBounds();
                boundsInitialized = true;
            }
        }
    }
    private void CalculateBounds()
    {
        float vertExtent = mainCamera.orthographicSize + boundsPadding;
        float horzExtent = vertExtent * aspectRatio;

        // 맵의 실제 크기와 카메라 뷰포트 크기를 비교하여 경계 조정
        float mapHalfWidth = mapSize.x * 0.5f;
        float mapHalfHeight = mapSize.y * 0.5f;

        // X축 경계 계산
        if (horzExtent < mapHalfWidth)
        {
            // 맵이 화면보다 큰 경우
            minX = mapCenter.x - mapHalfWidth + horzExtent;
            maxX = mapCenter.x + mapHalfWidth - horzExtent;
        }
        else
        {
            // 맵이 화면보다 작은 경우
            minX = maxX = mapCenter.x;
        }

        // Y축 경계 계산
        if (vertExtent < mapHalfHeight)
        {
            // 맵이 화면보다 큰 경우
            minY = mapCenter.y - mapHalfHeight + vertExtent;
            maxY = mapCenter.y + mapHalfHeight - vertExtent;
        }
        else
        {
            // 맵이 화면보다 작은 경우
            minY = maxY = mapCenter.y;
        }
    }
    private Vector3 ClampPosition(Vector3 targetPosition)
    {
        if (!boundsInitialized) return targetPosition;

        float vertExtent = mainCamera.orthographicSize;
        float horzExtent = vertExtent * aspectRatio;

        // 줌 레벨에 따른 동적 경계 계산
        float currentMinX = minX;
        float currentMaxX = maxX;
        float currentMinY = minY;
        float currentMaxY = maxY;

        // 맵이 화면보다 작은 경우 중앙으로 고정
        if (horzExtent >= mapSize.x * 0.5f)
        {
            targetPosition.x = mapCenter.x;
        }
        else
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, currentMinX, currentMaxX);
        }

        if (vertExtent >= mapSize.y * 0.5f)
        {
            targetPosition.y = mapCenter.y;
        }
        else
        {
            targetPosition.y = Mathf.Clamp(targetPosition.y, currentMinY, currentMaxY);
        }

        targetPosition.z = CAMERA_Z;
        return targetPosition;
    }
    private void HandleCameraMovement()
    {
        float deltaTime = Time.deltaTime;

        // Keyboard Movement
        if (enableKeyboardMovement)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                newPosition.y += moveSpeed * deltaTime;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                newPosition.y -= moveSpeed * deltaTime;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                newPosition.x += moveSpeed * deltaTime;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                newPosition.x -= moveSpeed * deltaTime;
        }

        // Edge Scrolling
        if (enableEdgeScrolling)
        {
            if (Input.mousePosition.x > Screen.width - edgeScrollSize)
                newPosition.x += moveSpeed * deltaTime;
            if (Input.mousePosition.x < edgeScrollSize)
                newPosition.x -= moveSpeed * deltaTime;
            if (Input.mousePosition.y > Screen.height - edgeScrollSize)
                newPosition.y += moveSpeed * deltaTime;
            if (Input.mousePosition.y < edgeScrollSize)
                newPosition.y -= moveSpeed * deltaTime;
        }

        // Mouse Drag Movement
        if (enableDragMovement)
            HandleMouseDrag();

        // Z 값을 항상 고정하고 이동 적용
        newPosition = ClampPosition(newPosition);
        transform.position = Vector3.Lerp(transform.position, newPosition, deltaTime * moveSpeed);

        // 마우스 커서가 화면 밖으로 못나가게
        //Cursor.lockState = CursorLockMode.Confined;
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(2) && !EventSystem.current.IsPointerOverGameObject())
        {
            // 2D에서는 XY 평면을 사용
            dragStartPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            dragStartPosition.z = 0;
        }

        if (Input.GetMouseButton(2) && !EventSystem.current.IsPointerOverGameObject())
        {
            dragCurrentPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            dragCurrentPosition.z = 0;
            newPosition = transform.position + (dragStartPosition - dragCurrentPosition) * dragSpeed;
        }
    }

    private void HandleCameraZoom()
    {
        if (!enableZoom) return;

        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            // 현재 마우스 위치 저장
            Vector3 mouseWorldPosBefore = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // 줌 레벨 변경
            float targetZoom = currentZoom - scrollDelta * zoomSpeed;
            currentZoom = Mathf.Clamp(targetZoom, zoomMin, zoomMax);
            mainCamera.orthographicSize = currentZoom; // Lerp 제거하여 즉시 적용

            // 경계 재계산
            CalculateBounds();

            // 새 마우스 위치 계산
            Vector3 mouseWorldPosAfter = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 worldPosDiff = mouseWorldPosBefore - mouseWorldPosAfter;

            // 카메라 위치 업데이트 및 즉시 클램프
            transform.position = ClampPosition(transform.position + worldPosDiff);
            newPosition = transform.position; // newPosition도 업데이트하여 동기화
        }
    }

    public void SetTarget(Transform target)
    {
        followTransform = target;
    }

    public void ClearTarget()
    {
        followTransform = null;
    }

    public void SetPosition(Vector3 position)
    {
        position = ClampPosition(position);
        position.z = CAMERA_Z;
        newPosition = position;
        transform.position = position;
    }

    public void SetZoom(float zoom)
    {
        currentZoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
        mainCamera.orthographicSize = currentZoom;
        InitializeBounds();
    }
    // 맵 경계를 수동으로 설정하는 메서드 추가
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        boundsInitialized = true;
    }
}