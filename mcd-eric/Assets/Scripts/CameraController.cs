using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform player; // set in unity

    private Transform _camera;
    private Transform _bgCamera1;
    private Transform _bgCamera2;
    private Transform _bgCamera3;

    void Awake() {
        _camera = GameObject.FindWithTag("MainCamera").transform;
		/*
        _bgCamera1 = GameObject.FindWithTag("BGCamera1").transform;
        _bgCamera2 = GameObject.FindWithTag("BGCamera2").transform;
        _bgCamera3 = GameObject.FindWithTag("BGCamera3").transform;
        */
    }

    void Update() {
        var x = player.position.x;
        _camera.position = new Vector3(x, _camera.position.y, _camera.position.z);
		/*
        _bgCamera1.position = new Vector3(x * 0.65f, _bgCamera1.position.y, _bgCamera1.position.z);
        _bgCamera2.position = new Vector3(x * 0.9f, _bgCamera2.position.y, _bgCamera2.position.z);
        _bgCamera3.position = new Vector3(x * 0.85f, _bgCamera3.position.y, _bgCamera3.position.z);
        */
    }
}

