using UnityEngine;
using System.Collections;

public class PlayerCharacterController : AbstractCharacterController {

    public bool isPlayerInputEnabled;
    private float _horizontalInput;

    protected override void Act() {

        _character.velocity.x = _character.maxWalkSpeed * _horizontalInput;

        if (_horizontalInput > 0) {
            _character.facing = Vector2.right;
        } else if (_horizontalInput < 0) {
            _character.facing = -Vector2.right;
        }

        return;
    }

    public void Update() {

        if (this.isPlayerInputEnabled) {

            _horizontalInput = Input.GetAxis("Horizontal"); // -1.0 to 1.0

            // this will do nothing if the player is already in the air
            // either because they are already jumping or if they are falling
            if (!_character.isJumping && Input.GetButtonDown("Jump")) {
                this.Jump();
            }
        }
    }
}
