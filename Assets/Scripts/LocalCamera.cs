using UnityEngine;
using System.Collections;

public class LocalCamera : MonoBehaviour {
 
    [HideInInspector]
    public Transform Target;
    
	void LateUpdate () { 
	    if(Target != null) {
            transform.position = Target.position;
            transform.position += new Vector3(0, 20, -13);
            transform.LookAt(Target.position);
        }
	}
}
