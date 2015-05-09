/* Make the gameObject follow another gameObject.
 */
using UnityEngine;
using System.Collections;

public class ObjectLabel : MonoBehaviour
{
	public Transform targetGameObject;
	public Vector3 offset;

	void Update()
	{
		transform.position = targetGameObject.position + offset;
	}
}
