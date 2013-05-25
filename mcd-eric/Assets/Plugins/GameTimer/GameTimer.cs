// Copyright (c) 2012 Eric Salczynski and Ramón Rocha
// This program is released under the MIT License.
// http://opensource.org/licenses/MIT

using UnityEngine;
using System;
using System.Collections;

// This is ticking off seconds slower than realtime. Why?
public class GameTimer : MonoBehaviour {

    /* *** Member Variables *** */

    public double startTime = 0;
    public bool countDown = true;   // count up or down
    public float timeScale = 1;     // Adjust the speed at which the timer counts
 
    private bool _paused = false;
    private int _step = 1;
    private double _currentTime;
    private double _previousTime;

    /* *** "Contstructors" (Start, Awake) *** */

    public void Awake() {
        _currentTime = startTime;
        _previousTime = _currentTime;
     
        if (countDown) {
            _step = -1;
        }
    }

    /* *** Properties *** */

    public double currentTime {
        get { return _currentTime; }
    }

    public bool paused {
        get { return _paused; }
    }

    public int hours {
        get { return (int)Math.Floor(currentTime / 3600); }
    }

    public int minutes {
        get { return (int)Math.Floor((currentTime - hours * 3600) / 60); }
    }

    public int seconds {
        get { return (int)Math.Floor(currentTime - hours * 3600 - minutes * 60); }
    }

    public int milliseconds {
        get { return (int)Math.Floor((currentTime - (double)Math.Truncate(currentTime)) * 100); }
    }

    public double deltaTime {
        get { return Math.Abs(_previousTime - _currentTime); }
    }

    /* *** MonoBehaviour/Overrideable Methods *** */

    public void FixedUpdate() {
        if (!paused) {
            _previousTime = _currentTime;
            _currentTime += Time.deltaTime * timeScale * _step;
        }
    }

    /* *** Public Methods *** */

    public void Pause() {
        _paused = true;
    }

    public void Unpause() {
        _paused = false;
    }

    public override string ToString() {
        return String.Format("{0:00}:{1:00}:{2:00}.{3:00}", hours, minutes, seconds, milliseconds);
    }
}
