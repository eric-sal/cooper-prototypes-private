using UnityEngine;
using System.Collections;

/// <summary>
/// The Character state is modified by a class that inherits from AbstractCharacterController.
/// </summary>
public class CharacterState : MonoBehaviour {

    public Vector2 facing;
    public float maxWalkSpeed;
    public float jumpSpeed;
    public int health;
    public bool isWalking;
    public bool isJumping;
    public int coinCount;
}
