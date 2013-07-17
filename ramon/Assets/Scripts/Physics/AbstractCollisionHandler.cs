using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// This is the base class from which all other collision handlers inherit.
/// </summary>
public abstract class AbstractCollisionHandler : MonoBehaviour {

    // to cache the reflected type name
    public string typeName;
    
    // so we can reuse the same block of memory when dispatching the collision handlers
    private System.Object[] _args;

    // so we can dispatch to the correct overloads based on the run-time types of the collision handlers
    private static Dictionary<string, Dictionary<string, MethodInfo>> _methodInfoTable;

    public virtual void Awake() {
        this.typeName = this.GetType().Name;
        _args = new System.Object[4];
        BuildMethodInfoTable();
    }


    /// <summary>
    /// Create a table mapping sublcasses of AbstractCollisionHandler to the correct HandleCollision method.
    /// Inspiration: http://www.arcadianvisions.com/downloads/MultipleDispatch/multiDispatch.html
    /// </summary>
    private void BuildMethodInfoTable() {

        if (_methodInfoTable != null) {
            // another instance already built the table earlier
            return;
        }

        _methodInfoTable = new Dictionary<string, Dictionary<string, MethodInfo>>();
        Type baseType = System.Type.GetType("AbstractCollisionHandler");

        foreach (Type t in Assembly.GetCallingAssembly().GetTypes()) {

            // skip any Type that does not inherit from baseType
            if (!t.IsSubclassOf(baseType)) {
                continue;
            }

            // create an entry in our table for this type
            string thisName = t.Name;
            _methodInfoTable.Add(thisName, new Dictionary<string, MethodInfo>());

            foreach (MethodInfo mi in t.GetMethods()) {
                if (mi.Name == "HandleCollision") {
                    // add this method to our table
                    ParameterInfo[] pars = mi.GetParameters();
                    string otherName = pars[0].ParameterType.Name;
                    _methodInfoTable[thisName].Add(otherName, mi);
                }
            }
        }
    }

    /// <summary>
    /// Calls the appropriate overload of HandleCollision for this.gameObject and collidedWith.gameObject
    /// </summary>
    public void OnCollision(Collider collidedWith, Vector3 fromDirection, float distance, Vector3 surfaceNormal) {
        
        var other = collidedWith.gameObject.GetComponent<AbstractCollisionHandler>();
        
        if (other == null) {
            this.HandleCollision(collidedWith, fromDirection, distance, surfaceNormal);
        
        } else {
            string thisName = this.typeName;
            string otherName = other.typeName;

            // dispatch other handler to our own overload
            MethodInfo mi = _methodInfoTable[thisName][otherName];
            _args[0] = other;
            _args[1] = fromDirection;
            _args[2] = distance;
            _args[3] = surfaceNormal;
            mi.Invoke(this, _args);

            // dispatch ourselves to the other handler's overload
            mi = _methodInfoTable[otherName][thisName];
            _args[0] = this;
            _args[1] = fromDirection * -1;
            _args[3] = surfaceNormal * -1; // this is wrong
            mi.Invoke(other, _args);
        }
    }

    /// <summary>
    /// Handles collision with objects that we don't have special behavior for; like the ground or other
    /// non-interactive things in the environment.  A subclass of AbstractCollisionHandler must, at a
    /// minimum, provide an implementation for this method.  All other overloads of the HandleCollision
    /// method will funnel into this function unless overridden with different behavior.
    /// </summary>
    public abstract void HandleCollision(Collider collidedWith, Vector3 fromDirection, float distance, Vector3 surfaceNormal);

    /// <summary>
    /// The behavior to use for unknown colliders.  Unless overridden this will pass through to
    /// HandleCollision(other.collider, fromDirection, distance).
    /// </summary>
    public virtual void DefaultHandleCollision(AbstractCollisionHandler other, Vector3 fromDirection, float distance, Vector3 surfaceNormal) {
        HandleCollision(other.collider, fromDirection, distance, surfaceNormal);
    }

    /*
    We want a virtual overload of HandleCollision for every concrete subtype of CollisionHandler.
    In AbstractCollisionHandler, every method should call the DefaultHandleCollision method.  Subclasses
    can then override HandleCollision for specific cases or can also override DefaultHandleCollision if necessary.
    Note, we don't need to include overloads for abstract subclasses as there will never be instances of
    those classes and the dispatching is done based on the run-time type of the collision handlers, not
    the compile-time type.
    */
    public virtual void HandleCollision(CharacterCollisionHandler other, Vector3 fromDirection, float distance, Vector3 surfaceNormal) {
        DefaultHandleCollision(other, fromDirection, distance, surfaceNormal);
    }

    public virtual void HandleCollision(PlatformCollisionHandler other, Vector3 fromDirection, float distance, Vector3 surfaceNormal) {
        DefaultHandleCollision(other, fromDirection, distance, surfaceNormal);
    }
}
