using UnityEngine;
using System.Collections;

/// <summary>
/// Attach this script to any GameObject capable of moving such as characters,
/// moving platforms, or projectiles.
/// </summary>
public class MoveableObject : MonoBehaviour {

    const float RAY_GAP = 0.2f;

    public Vector3 velocity;
    public PlatformCollisionHandler currentPlatform;
    protected Vector3 _currentSurfaceNormal;
    protected AbstractCharacterController _characterController;
    protected AbstractCollisionHandler _collisionHandler;
    protected Transform _transform;
    protected float _skinThickness;

    void Awake() {
        _characterController = GetComponent<AbstractCharacterController>();
        _collisionHandler = GetComponent<AbstractCollisionHandler>();
        _transform = GetComponent<Transform>();
    }

    public virtual void Start() {
        _skinThickness = 0.01f;
    }

    public bool isMovingRight {
        get { return velocity.x > 0; }
    }

    public bool isMovingLeft {
        get { return velocity.x < 0; }
    }

    public bool isMovingUp {
        get { return velocity.y > 0; }
    }

    public bool isMovingDown {
        get { return velocity.y < 0; }
    }

    public Vector3 position {
        get { return _transform.position; }
    }

    public virtual void FixedUpdate() {
        if (_characterController == null) {
            // this is an "uncontrolled" moveable object
            float dt = Time.deltaTime;
            Move(dt);
        }
    }

    /// <summary>
    /// Based on this object's current velocity, checks to see if this object would collide
    /// with something.  If so, we inform the attached collision handler.  If not, we alter
    /// the position of our transform.
    /// </summary>
    /// <param name='deltaTime'>
    /// The amount of time that has passed since the last collision check.
    /// </param>
    public void Move(float deltaTime) {

        // modifies vertical velocity
        ApplyGravity(deltaTime);

        Bounds b = this.collider.bounds;
        Vector3 center = b.center;
        float ex = b.extents.x;
        float ey = b.extents.y;
        var topLeft = new Vector3(center.x - ex + _skinThickness, center.y + ey - _skinThickness);
        var topRight = new Vector3(center.x + ex - _skinThickness, center.y + ey - _skinThickness);
        var bottomLeft = new Vector3(center.x - ex + _skinThickness, center.y - ey + _skinThickness);
        var bottomRight = new Vector3(center.x + ex - _skinThickness, center.y - ey + _skinThickness);
        RaycastHit hitInfo;

        if (velocity.x != 0) {
            Vector3 hDirection = velocity.x > 0 ? Vector3.right : Vector3.left;
            float hDistance = velocity.x * deltaTime;

            if (this.currentPlatform != null) {
                // The horizontal rays we cast will be orthogonal to the normal
                // vector of the current surface we are on.
                //Debug.Log("Before, hDirection = " + hDirection.ToString());
                if (hDirection == Vector3.right) {
                    hDirection = new Vector3(_currentSurfaceNormal.y, -_currentSurfaceNormal.x);
                }
                else {
                    hDirection = new Vector3(-_currentSurfaceNormal.y, _currentSurfaceNormal.x);
                }
                //Debug.Log("After, hDirection = " + hDirection.ToString());
            }

            if (this.isMovingRight) {
                if (hDirection.y != 0) {
                    Debug.Log(hDirection);
                    // on a sloped surface
                    if (hDirection.y > 0) {
                        // sloping up
                        Debug.Log("Sloping up");
                        if (BottomToTopSweep(bottomRight, topRight, hDirection, hDistance, out hitInfo)) {
                            _collisionHandler.OnCollision(hitInfo.collider, Vector3.right, hitInfo.distance, hitInfo.normal);
                        }

                        if (RightToLeftRaySweep(topRight, topLeft, hDirection, hDistance, out hitInfo)) {
                            _collisionHandler.OnCollision(hitInfo.collider, Vector3.up, hitInfo.distance, hitInfo.normal);
                        }
                        else {
                            // no collision so move the game object we are attached to
                            float x = _transform.position.x + hDistance * hDirection.x;
                            float y = _transform.position.y + hDistance * hDirection.y;
                            _transform.position = new Vector3(x, y, 0);
                        }
                    }
                    else {
                        // sloping down
                        Debug.Log("Sloping down");
                        if (TopToBottomRaySweep(topRight, bottomRight, hDirection, hDistance, out hitInfo)) {
                            _collisionHandler.OnCollision(hitInfo.collider, Vector3.right, hitInfo.distance, hitInfo.normal);
                        }

                        if (RightToLeftRaySweep(bottomRight, bottomLeft, hDirection, hDistance, out hitInfo)) {
                            _collisionHandler.OnCollision(hitInfo.collider, Vector3.down, hitInfo.distance, hitInfo.normal);
                        }
                        else {
                            // no collision so move the game object we are attached to
                            float x = _transform.position.x + hDistance * hDirection.x;
                            float y = _transform.position.y + hDistance * hDirection.y;
                            _transform.position = new Vector3(x, y, 0);
                        }
                    }
                }
                else {
                    // not on a sloped surface
                    if (BottomToTopSweep(bottomRight, topRight, hDirection, hDistance, out hitInfo)) {
                        _collisionHandler.OnCollision(hitInfo.collider, Vector3.right, hitInfo.distance, hitInfo.normal);
                    }
                    else {
                        // no collision so move the game object we are attached to
                        float x = _transform.position.x + hDistance;
                        _transform.position = new Vector3(x, _transform.position.y, 0);
                    }
                }
            } else {
                // Moving left
                if (hDirection.y != 0) {
                    // on a sloped surface
                    if (hDirection.y > 0) {
                        // sloping up
                        Debug.Log("Sloping up");
                        if (BottomToTopSweep(bottomLeft, topLeft, hDirection, -hDistance, out hitInfo)) {
                            _collisionHandler.OnCollision(hitInfo.collider, Vector3.left, hitInfo.distance, hitInfo.normal);
                        }

                        if (LeftToRightRaySweep(topLeft, topRight, hDirection, -hDistance, out hitInfo)) {
                            _collisionHandler.OnCollision(hitInfo.collider, Vector3.up, hitInfo.distance, hitInfo.normal);
                        }
                        else {
                            // no collision so move the game object we are attached to
                            float x = _transform.position.x - hDistance * hDirection.x;
                            float y = _transform.position.y - hDistance * hDirection.y;
                            _transform.position = new Vector3(x, y, 0);
                        }
                    }
                    else {
                        // sloping down
                        Debug.Log("Sloping down");
                        if (TopToBottomRaySweep(topLeft, bottomLeft, hDirection, -hDistance, out hitInfo)) {
                            _collisionHandler.OnCollision(hitInfo.collider, Vector3.left, hitInfo.distance, hitInfo.normal);
                        }

                        if (LeftToRightRaySweep(bottomLeft, bottomRight, hDirection, -hDistance, out hitInfo)) {
                            _collisionHandler.OnCollision(hitInfo.collider, Vector3.down, hitInfo.distance, hitInfo.normal);
                        }
                        else {
                            // no collision so move the game object we are attached to
                            float x = _transform.position.x - hDistance * hDirection.x;
                            float y = _transform.position.y - hDistance * hDirection.y;
                            _transform.position = new Vector3(x, y, 0);
                        }
                    }
                }
                else {
                    // not on a sloped surface
                    if (BottomToTopSweep(bottomLeft, topLeft, hDirection, -hDistance, out hitInfo)) {
                        _collisionHandler.OnCollision(hitInfo.collider, Vector3.left, hitInfo.distance, hitInfo.normal);
                    }
                    else {
                        // no collision so move the game object we are attached to
                        float x = _transform.position.x + hDistance;
                        _transform.position = new Vector3(x, _transform.position.y, 0);
                    }
                }
            }
        }

        // TODO: Offset rays

        if (velocity.y != 0) {
            float vVelocity = velocity.y;
            Vector3 vDirection = vVelocity > 0 ? Vector3.up : Vector3.down;
            float vDistance = vVelocity * deltaTime;
            float rayLength = Mathf.Abs(vDistance) + _skinThickness;

            if (vDirection == Vector3.up && LeftToRightRaySweep(topLeft, topRight, vDirection, rayLength, out hitInfo)) {
                _collisionHandler.OnCollision(hitInfo.collider, vDirection, hitInfo.distance, hitInfo.normal);
            }
            else if (RightToLeftRaySweep(bottomRight, bottomLeft, vDirection, rayLength, out hitInfo)) {
                _collisionHandler.OnCollision(hitInfo.collider, vDirection, hitInfo.distance, hitInfo.normal);
            }
            else {
                // no collision so move the game object we are attached to
                float y = _transform.position.y + vDistance;
                _transform.position = new Vector3(_transform.position.x, y, 0);
            }
        }
    }

    /// <summary>
    /// Gets the normal vector for the surface of the current platform we are on.
    /// </summary>
    private void GetCurrentPlatformNormal() {
        Vector3 rayOrigin = this.collider.bounds.center;

        //Debug.Log(string.Format("rayOrigin = {0}", rayOrigin));

        Vector3 target = new Vector3(rayOrigin.x, rayOrigin.y - this.collider.bounds.extents.y);

        //Debug.Log(string.Format("target = {0}", target));

        Vector3 direction = target - rayOrigin;
        float distance = Vector3.Distance(rayOrigin, target) * 2;
        RaycastHit hitInfo;

        Debug.DrawLine(rayOrigin, target, Color.blue);

        if (Physics.Raycast(rayOrigin, direction, out hitInfo, distance)) {
            //Debug.Log("ray hit surface");
        } else {
            //Debug.Log("ray did not hit surface");
        }
        _currentSurfaceNormal = hitInfo.normal;

    }

    /// <summary>
    /// Casts rays from left to right starting from 'startPoint' and ending at 'endPoint'.
    /// </summary>
    /// <returns>
    /// True if any ray hits something.
    /// </returns>
    private bool LeftToRightRaySweep(Vector3 startPoint, Vector3 endPoint, Vector3 direction, float rayLength, out RaycastHit hitInfo) {

        float y = startPoint.y;

        for (float x = startPoint.x; x <= endPoint.x; x += RAY_GAP) {

            var start = new Vector3(x, y);
            var end = new Vector3(start.x + direction.x * rayLength, start.y + direction.y * rayLength);

            Debug.DrawLine(start, end, Color.green, 0.25f);
            if (Physics.Raycast(start, direction, out hitInfo, rayLength)) {
                return true;
            }
        }
        hitInfo = new RaycastHit();
        return false;
    }

    /// <summary>
    /// Casts rays from right to left starting from 'startPoint' and ending at 'endPoint'.
    /// </summary>
    /// <returns>
    /// True if any ray hits something.
    /// </returns>
    private bool RightToLeftRaySweep(Vector3 startPoint, Vector3 endPoint, Vector3 direction, float rayLength, out RaycastHit hitInfo) {

        float y = startPoint.y;

        for (float x = startPoint.x; x >= endPoint.x; x -= RAY_GAP) {

            var start = new Vector3(x, y);
            var end = new Vector3(start.x + direction.x * rayLength, start.y + direction.y * rayLength);

            Debug.DrawLine(start, end, Color.red, 0.25f);
            if (Physics.Raycast(start, direction, out hitInfo, rayLength)) {
                return true;
            }
        }
        hitInfo = new RaycastHit();
        return false;
    }

    /// <summary>
    /// Casts rays from top to bottom starting from 'startPoint' and ending at 'endPoint'.
    /// </summary>
    /// <returns>
    /// True if any ray hits something.
    /// </returns>
    private bool TopToBottomRaySweep(Vector3 startPoint, Vector3 endPoint, Vector3 direction, float rayLength, out RaycastHit hitInfo) {

        float x = startPoint.x;

        for (float y = startPoint.y; y >= endPoint.y; y -= RAY_GAP) {

            var start = new Vector3(x, y);
            var end = new Vector3(start.x + direction.x * rayLength, start.y + direction.y * rayLength);

            Debug.DrawLine(start, end, Color.cyan, 0.25f);
            if (Physics.Raycast(start, direction, out hitInfo, rayLength)) {
                return true;
            }
        }
        hitInfo = new RaycastHit();
        return false;
    }

    /// <summary>
    /// Casts rays from bottom to top starting from 'startPoint' and ending at 'endPoint'.
    /// </summary>
    /// <returns>
    /// True if any ray hits something.
    /// </returns>
    private bool BottomToTopSweep(Vector3 startPoint, Vector3 endPoint, Vector3 direction, float rayLength, out RaycastHit hitInfo) {

        float x = startPoint.x;

        for (float y = startPoint.y; y <= endPoint.y; y += RAY_GAP) {

            var start = new Vector3(x, y);
            var end = new Vector3(start.x + direction.x * rayLength, start.y + direction.y * rayLength);

            Debug.DrawLine(start, end, Color.magenta, 0.25f);
            if (Physics.Raycast(start, direction, out hitInfo, rayLength)) {
                return true;
            }
        }
        hitInfo = new RaycastHit();
        return false;
    }

    protected void AddVelocity(Vector2 v) {
        velocity.x += v.x;
        velocity.y += v.y;
    }

    /// <summary>
    /// Applies acceleration of gravity to velocity if we are not currently on a platform.
    /// </summary>
    public virtual void ApplyGravity(float deltaTime) {

        Bounds b = this.collider.bounds;
        Vector3 center = b.center;
        float ex = b.extents.x;
        float ey = b.extents.y;
        var bottomLeft = new Vector3(center.x - ex + _skinThickness, center.y - ey + _skinThickness);
        var bottomRight = new Vector3(center.x + ex - _skinThickness, center.y - ey + _skinThickness);

        if (this.currentPlatform != null) {
            // We are currently on a platform (or maybe just were moments ago). For sure
            // we are not jumping right now, but we might be about to fall.
            // Are we still on that platform?  Cast rays to find out.
            GetCurrentPlatformNormal();
            Vector3 gravityDirection = _currentSurfaceNormal * -1;

            float gravityDistance = SceneController.GRAVITY * deltaTime * -1;
            RaycastHit platformHitInfo;

            if (this.isMovingLeft && LeftToRightRaySweep(bottomLeft, bottomRight, gravityDirection, gravityDistance, out platformHitInfo) ||
                RightToLeftRaySweep(bottomRight, bottomLeft, gravityDirection, gravityDistance, out platformHitInfo)) {
                // we are still on the platform, gravity shouldn't affect our velocity
                return;
            }
            else {
                // we are no longer on that platform
                this.currentPlatform = null;
            }
        }

        // add gravity to our current velocity
        AddVelocity(new Vector2(0, SceneController.GRAVITY * deltaTime));
    }
}
