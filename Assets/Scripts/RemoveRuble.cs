using UnityEngine;
using System.Collections;

public class RemoveRuble : MonoBehaviour {

	private void OnCollisionEnter(Collision collision){
		Destroy (collision.gameObject);
	}
}
