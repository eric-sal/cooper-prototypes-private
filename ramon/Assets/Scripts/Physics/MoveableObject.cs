using UnityEngine;
using System.Collections;

public class MoveableObject : MonoBehaviour {
 
    public Vector3 velocity;
    public PlatformCollisionHandler lastPlatform;
    protected AbstractCharacterController _characterController;
    protected AbstractCollisionHandler _collisionHandler;
    protected Transform _transform;
    protected float _colliderBoundsOffsetX;
    protected float _colliderBoundsOffsetY;
    protected float _skinThickness;

    void Awake() {
        _characterController = GetComponent<AbstractCharacterController>();
        _collisionHandler = GetComponent<AbstractCollisionHandler>();
        _transform = GetComponent<Transform>();
    }

    public virtual void Start() {
        Collider c = GetComponent<Collider>();
        _colliderBoundsOffsetX = c.bounds.extents.x;
        _colliderBoundsOffsetY = c.bounds.extents.y;
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

    public void Move(float deltaTime) {
        // TODO? Accept a function delegate to call before doing calculations

        // modifies vertical velocity
        ApplyGravity(deltaTime);

        // adjust velocity and fire events based on collisions (if any)
        CollisionCheck(deltaTime);

        // move the game object we are attached to
        float x = _transform.position.x + velocity.x * deltaTime;
        float y = _transform.position.y + velocity.y * deltaTime;
        _transform.position = new Vector3(x, y, 0);
    }

    /// <summary>
    /// Based on this object's current velocity, checks to see if this object would collide
    /// with something.  If so, then the object's velcoity and position are updated to
    /// prevent overlapping colliders.
    /// </summary>
    /// <param name='deltaTime'>
    /// The amount of time that has passed since the last collision check.
    /// </param>
    private void CollisionCheck(float deltaTime) {

        Vector3 rayOrigin = this.collider.bounds.center;
        float absoluteDistance;
        RaycastHit hitInfo;

        // cast horizontal rays
        float hVelocity = velocity.x;

        // if we're not moving horizontally, then don't cast any horizontal rays
        if (hVelocity != 0) {
            Vector3 hDirection = (hVelocity > 0) ? Vector3.right : Vector3.left;

            float hDistance = hVelocity * deltaTime;
            absoluteDistance = Mathf.Abs(hDistance) + _colliderBoundsOffsetX + _skinThickness;

            Vector3 yOffset = new Vector3(0, _colliderBoundsOffsetY - _skinThickness, 0);

            if (Physics.Raycast(rayOrigin, hDirection, out hitInfo, absoluteDistance) ||
                Physics.Raycast(rayOrigin + yOffset, hDirection, out hitInfo, absoluteDistance) ||
                Physics.Raycast(rayOrigin - yOffset, hDirection, out hitInfo, absoluteDistance)) {

                // a horizontal collision has occurred
                _collisionHandler.OnCollision(hitInfo.collider, hDirection, hitInfo.distance);

            } else {
                // we didn't have a horizontal collision, offset the vertical rays by the amount the player moved
                rayOrigin.x += hDistance;
            }
        }

        // cast veritcal rays
        float vVelocity = velocity.y;

        // even if we're not currently moving in the y direction, cast a ray in the direction of gravity (i.e. down)
        Vector3 vDirection = (vVelocity > 0) ? Vector3.up : Vector3.down;

        float vDistance = vVelocity * deltaTime;
        absoluteDistance = Mathf.Abs(vDistance) + _colliderBoundsOffsetY + _skinThickness;

        Vector3 xOffset = new Vector3(_colliderBoundsOffsetX - _skinThickness, 0, 0);

        if (Physics.Raycast(rayOrigin, vDirection, out hitInfo, absoluteDistance) ||
            Physics.Raycast(rayOrigin + xOffset, vDirection, out hitInfo, absoluteDistance) ||
            Physics.Raycast(rayOrigin - xOffset, vDirection, out hitInfo, absoluteDistance)) {

            // a vertical collision has occurred
            _collisionHandler.OnCollision(hitInfo.collider, vDirection, hitInfo.distance);

        } else {
            lastPlatform = null;
        }
    }
 
    protected void AddVelocity(Vector2 v) {
        velocity.x += v.x;
        velocity.y += v.y;
    }
 
    public virtual void ApplyGravity(float deltaTime) {
        AddVelocity(new Vector2(0, SceneController.GRAVITY * deltaTime));
    }
}
