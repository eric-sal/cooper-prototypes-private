using UnityEngine;
using System.Collections;

public class CharacterCollisionHandler : AbstractCollisionHandler {
    protected CharacterState _character;
    protected MoveableObject _moveable;
    protected Transform _transform;
    protected float _skinThickness;

    public override void Awake() {
        base.Awake();
        _character = GetComponent<CharacterState>();
        _moveable = GetComponent<MoveableObject>();
        _transform = this.gameObject.transform;
        _skinThickness = 0.01f;
    }

    /// <summary>
    /// Stop the character's movement in the direction the collision came from.
    /// </summary>
    public override void HandleCollision(Collider collidedWith, Vector3 fromDirection, float distance, Vector3 surfaceNormal) {
        // a collision in the direction we are moving means we should stop moving
        if (_moveable.isMovingRight && fromDirection == Vector3.right ||
            _moveable.isMovingLeft && fromDirection == Vector3.left) {

            float hDistance = distance - _skinThickness;
         
            _moveable.velocity.x = 0;
            if (fromDirection == Vector3.left) {
                hDistance *= -1;
            }
         
            _transform.position = new Vector3(_transform.position.x + hDistance, _transform.position.y, 0);

        } else if (_moveable.isMovingUp && fromDirection == Vector3.up ||
            _moveable.isMovingDown && fromDirection == Vector3.down) {

            _moveable.velocity.y = 0;
            float vDistance = distance - _skinThickness;

            if (fromDirection == Vector3.down) {
                _character.isJumping = false;
                vDistance *= -1;
            }
         
            _transform.position = new Vector3(_transform.position.x, _transform.position.y + vDistance, 0);
        }
    }

    public override void HandleCollision(PlatformCollisionHandler platform, Vector3 fromDirection, float distance, Vector3 surfaceNormal) {
        if (fromDirection == Vector3.down) {
            _moveable.currentPlatform = platform;
        }
        HandleCollision(platform.gameObject.collider, fromDirection, distance, surfaceNormal);
    }
}
