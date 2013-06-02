using UnityEngine;
using System.Collections;

public class CharacterCollisionHandler : AbstractCollisionHandler {
    protected CharacterState _character;
    protected Transform _transform;
    protected float _colliderBoundsOffsetX;
    protected float _colliderBoundsOffsetY;

    public override void Awake() {
        base.Awake();
        _character = GetComponent<CharacterState>();
        _transform = this.transform;
    }

    /// <summary>
    /// Stop the character's movement in the direction the collision came from.
    /// </summary>
    public override void HandleCollision(Collider collidedWith, Vector3 fromDirection, float distance, Vector3 normal) {
        _colliderBoundsOffsetX = this.collider.bounds.extents.x;
        _colliderBoundsOffsetY = this.collider.bounds.extents.y;

        // Get the angle of the slow


        // a collision in the direction we are moving means we should stop moving
        if (_character.isMovingRight && fromDirection == Vector3.right ||
            _character.isMovingLeft && fromDirection == Vector3.left) {
            float hDistance = (distance - _colliderBoundsOffsetX) * fromDirection.x;
            float vDistance = 0;

            if (normal.y == 0) {
                // If the surface we collied with is a vertical wall,
                // then stop our forward progress.
                _character.velocity.x = 0;
            } else {
                // Otherwise, this is a sloped surface.

                // Determine the angle of the sloped surface from the normal,
                // then use the angle to find the slope of the surface.
                float angle = Vector3.Angle(fromDirection, normal) - 90;
                float radians = angle * Mathf.Deg2Rad;
                float slope = Mathf.Tan(radians) * fromDirection.x;

                // hDistance should be the distance we *want* to travel,
                // not the distance between our collider and the object
                // we collided with.
                hDistance = _character.velocity.x * Time.deltaTime;

                // Since we know how far we want to travel horizontally,
                // we can multiply that value (x) by the slope (m) to
                // find our vertical distance offset.
                // Equation for slope : m = y / x
                vDistance = slope * hDistance;

                // We actually want this to be 0, but that stops the animation.
                // Better solution: adjust conditions for stopping the walk animation.
                _character.velocity.x = 0.01f;
            }

            _transform.position = new Vector3(_transform.position.x + hDistance, _transform.position.y + vDistance, 0);

        } else if (_character.isMovingUp && fromDirection == Vector3.up ||
            _character.isMovingDown && fromDirection == Vector3.down) {

            // We need there to be a constant downward gravitational force in order to
            // smoothly walk down slopes, so we never set the Y velocity to 0. However,
            // this screws up our collision detection, and mario will get partially
            // stuck in the ground after landing.
            //_character.velocity.y = 0;

            float vDistance = (distance - _colliderBoundsOffsetY) * fromDirection.y;

            if (fromDirection == Vector3.down) {
                _character.isGrounded = true;
                _character.isJumping = false;
            }
         
            _transform.position = new Vector3(_transform.position.x, _transform.position.y + vDistance, 0);
        }
    }
}
