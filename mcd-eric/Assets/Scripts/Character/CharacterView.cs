using UnityEngine;
using System.Collections;

// TODO: This is a straight conversion from js without much modification, need to tweak for more robust inheritance
public class CharacterView : MonoBehaviour {

    public enum Animations { StandRight, SlideRight, WalkRight, JumpRight, StandLeft, SlideLeft, WalkLeft, JumpLeft, PlayerDeath };

    protected CharacterState _character;
    protected AnimatedSprite _sprite;
    protected Animations _currentAnimation;

    public void Awake() {
        _character = this.GetComponent<CharacterState>();
        _sprite = this.GetComponent<AnimatedSprite>();
    }

    void Start() {
        _currentAnimation = Animations.StandRight;
    }
    
    void Update() {
        if (_character.health == 0) {
            // player is dead
            if (_currentAnimation != Animations.PlayerDeath) {
                _currentAnimation = Animations.PlayerDeath;
                _sprite.ShowFrame(0);

                //var currentPosition : Vector3 = transform.position;
                //var apex : Vector3 = currentPosition + (Vector3.up * 112);

                 // Calling Player.Respawn() oncomplete probably isn't the best bet in the long run.
                 // We not only want to respawn the player, but reset the level.
                 // Plan: Add a method to SceneController called Reset, and add a Reset method to
                 // every object that can/needs to be reset after player death.
                 //iTween.MoveTo(gameObject, {'path': [apex, currentPosition], 'easetype': 'linear', 'time': 1, 'oncomplete': 'Respawn' });
            }
        } else {
            if (_character.facing == Vector2.right) {
                if (_character.isWalking) {
                    if (_currentAnimation != Animations.WalkRight) {
                        _currentAnimation = Animations.WalkRight;
                        _sprite.ShowFrame(9);
                        _sprite.Play("WalkRight");
                    }
                } else if (_character.isJumping) {
                    if (_currentAnimation != Animations.JumpRight) {
                        _currentAnimation = Animations.JumpRight;
                        _sprite.ShowFrame(2);
                    }
                } else {
                    if (_currentAnimation != Animations.StandRight) {
                        _currentAnimation = Animations.StandRight;
                        _sprite.ShowFrame(5);
                    }
                }
            } else {
                // player is facing left
                if (_character.isWalking) {
                    if (_currentAnimation != Animations.WalkLeft) {
                        _currentAnimation = Animations.WalkLeft;
                        _sprite.ShowFrame(6);
                        _sprite.Play("WalkLeft");
                    }
                } else if (_character.isJumping) {
                    if (_currentAnimation != Animations.JumpLeft) {
                        _currentAnimation = Animations.JumpLeft;
                        _sprite.ShowFrame(1);
                    }
                } else {
                    if (_currentAnimation != Animations.StandLeft) {
                        _currentAnimation = Animations.StandLeft;
                        _sprite.ShowFrame(4);
                    }
                }
            }
        }
    }
}

