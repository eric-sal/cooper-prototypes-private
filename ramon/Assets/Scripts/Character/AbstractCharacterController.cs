using UnityEngine;
using System.Collections;

/// <summary>
/// Controls character behavior either through player input or AI.
/// Associated GameObject should have a Character-derived script, a
/// kindred-sprite Sprite-derived script, and a MoveableObject script.
///
/// You cannot assign this to a game object directly.  You must inherit
/// from this class.
///
/// Sub-classes should modify character state in the 'Act' function.
/// </summary>
public abstract class AbstractCharacterController : MonoBehaviour {

    protected CharacterState _character;
    protected Sprite _sprite;
    protected MoveableObject _moveable;
    protected float _jumpTolerance;

    public virtual void Awake() {
        _character = GetComponent<CharacterState>();
        _sprite = GetComponent<Sprite>();
        _moveable = GetComponent<MoveableObject>();
    }

    public virtual void Start() {
        _jumpTolerance = 30.0f;
    }

    /// <summary>
    /// This is to be defined by subclasses.  This method is called from
    /// FixedUpdate before any physics calculations have been performed.
    /// Here would be where player inputs would get recorded and the
    /// character state would get modified.  For enemies, the AI would
    /// alter the state instead.
    /// </summary>
    protected abstract void Act();

    public virtual void FixedUpdate() {
        // Let player or AI modify character state first
        // TODO? Convert to delegate and pass to _moveable.Move
        Act();
     
        float dt = Time.deltaTime;
        _moveable.Move(dt);

        if (_moveable.currentPlatform != null) {
            _character.isWalking = _moveable.velocity.x != 0;
        }
    }

    public virtual void Jump(float multiplier = 1.0f) {
        if (_character.isJumping) {
            return;
        }

        if (Mathf.Abs(_moveable.velocity.y) <= _jumpTolerance) {
            _character.isJumping = true;
            _moveable.currentPlatform = null;
            _moveable.velocity.y += _character.jumpSpeed * multiplier;
        }
    }
}
