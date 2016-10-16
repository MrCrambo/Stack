using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Stack : MonoBehaviour {

	public Text scoreText;
	public Color32[] gameColors = new Color32[4];
	public Material stackMaterial;
	public GameObject endPanel;

	private const float BOUND_SIZE = 3.5f;
	private const float STACK_MOVING_SPEED = 5.0f;
	private const float ERROR_MARGIN = 0.1f;

	private GameObject[] theStack;
	private Vector2 stackBound = new Vector2 (BOUND_SIZE, BOUND_SIZE);

	private int stackIndex = 0;
	private int scoreCount = 0;
	private int combo = 0;

	private float tileTransition = 0.0f;
	private float tileSpeed = 2.5f;
	private float secondaryPosition;

	private Vector3 desiredPosition;
	private Vector3 lastTilePosition;

	private bool isMoveOnX = true;
	private bool isDead = false;

	private void Start () {
		theStack = new GameObject[transform.childCount];
		for (int i = 0; i < transform.childCount; i++) {
			theStack [i] = transform.GetChild (i).gameObject; 
			ColorMesh (theStack [i].GetComponent<MeshFilter> ().mesh);
		}

		stackIndex = transform.childCount - 1;
	}

	private void Update () {

		if (isDead)
			return;

		if (Input.GetMouseButtonDown (0)) {
			if (PlaceIt ()) {
				SpawnTile ();
				scoreCount++;
				scoreText.text = scoreCount.ToString ();
			} else {
				EndGame ();
			}
		
		}

		MoveTile ();

		transform.position = Vector3.Lerp (transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime);
	}

	private void CreateRubble(Vector3 pos, Vector3 sca){
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cube);

		go.transform.localPosition = pos;
		go.transform.localScale = sca;
		go.AddComponent<Rigidbody> ();

		go.GetComponent<MeshRenderer> ().material = stackMaterial;
		ColorMesh (go.GetComponent<MeshFilter> ().mesh);
	}

	private void MoveTile(){

		tileTransition += Time.deltaTime * tileSpeed;
		if (isMoveOnX)
			theStack [stackIndex].transform.localPosition = new Vector3 (Mathf.Sin(tileTransition) * BOUND_SIZE, scoreCount, secondaryPosition);
		else
			theStack [stackIndex].transform.localPosition = new Vector3 (secondaryPosition, scoreCount, Mathf.Sin(tileTransition) * BOUND_SIZE);
	}

	private void SpawnTile(){
		lastTilePosition = theStack [stackIndex].transform.localPosition;

		stackIndex--;
		if (stackIndex < 0) {
			stackIndex = transform.childCount - 1;
		}

		desiredPosition = (Vector3.down) * scoreCount;

		theStack [stackIndex].transform.localPosition = new Vector3 (0, scoreCount, 0);
		theStack [stackIndex].transform.localScale = new Vector3 (stackBound.x, 1, stackBound.y);

		ColorMesh (theStack[stackIndex].GetComponent<MeshFilter>().mesh);
	}

	private bool PlaceIt(){

		Transform t = theStack [stackIndex].transform;

		if (isMoveOnX) {
			float deltaX = lastTilePosition.x - t.position.x;
			if (Mathf.Abs (deltaX) > ERROR_MARGIN) {
				// cut the tile

				combo = 0;
				stackBound.x -= Mathf.Abs (deltaX);
				if (stackBound.x <= 0)
					return false;

				float middle = lastTilePosition.x + t.localPosition.x / 2;
				t.localScale = new Vector3 (stackBound.x, 1, stackBound.y);
				CreateRubble (
					new Vector3((t.position.x > 0)
						?t.position.x + (t.localScale.x / 2)
						:t.position.x - (t.localScale.x / 2)
						, t.position.y, t.position.z), 
					new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z));
				t.localPosition = new Vector3 (middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
			} else {
				combo++;

				if (combo > 3) {
					stackBound.x += 0.25f;
					float middle = lastTilePosition.x + t.localPosition.x / 2;
					t.localScale = new Vector3 (stackBound.x, 1, stackBound.y);
					t.localPosition = new Vector3 (middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
				}
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, lastTilePosition.z);

			}
		} else {
			float deltaZ = lastTilePosition.z - t.position.z;
			if (Mathf.Abs (deltaZ) > ERROR_MARGIN) {
				// cut the tile

				combo = 0;
				stackBound.y -= Mathf.Abs (deltaZ);
				if (stackBound.y <= 0)
					return false;

				float middle = lastTilePosition.z + t.localPosition.z / 2;
				t.localScale = new Vector3 (stackBound.x, 1, stackBound.y);
				CreateRubble (
					new Vector3(t.position.x, t.position.y,
						(t.position.z > 0)
						?t.position.z + (t.localScale.z / 2)
						:t.position.z - (t.localScale.z / 2)), 
					new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ)));
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
			} else {
				combo++;

				if (combo > 3) {
					stackBound.x += 0.25f;
					float middle = lastTilePosition.z + t.localPosition.z / 2;
					t.localScale = new Vector3 (stackBound.x, 1, stackBound.y);
					t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
				}

				t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);

			}
		}

		secondaryPosition = (isMoveOnX) ? t.localPosition.x : t.localPosition.z;

		isMoveOnX = !isMoveOnX;

		return true;
	}

	private void EndGame(){
		if (PlayerPrefs.GetInt ("score") < scoreCount) {
			PlayerPrefs.SetInt ("score", scoreCount);
		}
		isDead = true;
		endPanel.SetActive (true);
		theStack [stackIndex].AddComponent<Rigidbody> ();
	}

	public void OnButtonClick(string sceneName){
		SceneManager.LoadScene (sceneName);
	}

	private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t){
		if (t < 0.33f)
			return Color.Lerp (a, b, t / 0.33f);
		else if (t < 0.66f)
			return Color.Lerp (b, c, (t - 0.33f) / 0.33f);
		else
			return Color.Lerp (c, d, (t - 0.66f) / 0.66f);
	}

	private void ColorMesh(Mesh mesh){
		Vector3[] vertices = mesh.vertices;
		Color32[] colors = new Color32[vertices.Length];
		float f = Mathf.Sin (scoreCount * 0.25f);

		for (int i = 0; i < vertices.Length; i++) {
			colors [i] = Lerp4 (gameColors [0], gameColors [1], gameColors [2], gameColors [3], f);
		}

		mesh.colors32 = colors;
	}
}
