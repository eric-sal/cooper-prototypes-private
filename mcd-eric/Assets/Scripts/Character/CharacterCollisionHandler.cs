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
        _colliderBoundsOffsetX = this.collider.bounds.extents.x;
        _colliderBoundsOffsetY = this.collider.bounds.extents.y;
    }

    /// <summary>
    /// Stop the character's movement in the direction the collision came from.
    /// </summary>
    public override void HandleCollision(Collider collidedWith, Vector3 fromDirection, float distance) {
        // a collision in the direction we are moving means we should stop moving
        if (_character.isMovingRight && fromDirection == Vector3.right ||
            _character.isMovingLeft && fromDirection == Vector3.left) {
            float hDistance = distance - _colliderBoundsOffsetX;
         
            _character.velocity.x = 0;
            if (fromDirection == Vector3.left) {
                hDistance *= -1;
            }
         
            _transform.position = new Vector3(_transform.position.x + hDistance, _transform.position.y, 0);

        } else if (_character.isMovingUp && fromDirection == Vector3.up ||
            _character.isMovingDown && fromDirection == Vector3.down) {

            _character.velocity.y = 0;
            float vDistance = distance - _colliderBoundsOffsetY;

            if (fromDirection == Vector3.down) {
                _character.isGrounded = true;
                _character.isJumping = false;
                vDistance *= -1;
            }
         
            _transform.position = new Vector3(_transform.position.x, _transform.position.y + vDistance, 0);
        }
    }
}
