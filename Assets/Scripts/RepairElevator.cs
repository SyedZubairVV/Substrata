using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class ElevatorRepairTilemap : MonoBehaviour
{
	[Header("UI & Animation")]
	public CanvasGroup fadeGroup;
	public GameObject repairPrompt;
	public GameObject ridePrompt;

	[Header("Tile Settings")]
	public Tilemap tilemap;
	public Vector3Int brokenTopLeft;      // Coordinate of the broken elevator top-left
	public Vector3Int fixedTemplateTopLeft; // Coordinate of your "Fixed" hidden version top-left
	public Vector2Int fixedDimensions;         // How many tiles wide/tall (e.g. x:3, y:4)
	public Vector3Int topPosTopLeft;
	public Vector3Int topPosTemplateTopLeft;
	public Vector2Int topPosDimensions;

	[Header("Player Object")]
	public GameObject player;
	public int newX;
	public int newY;

	private bool playerInRange;
	private bool isFixed;
	private bool elevatorUp;
	void Start()
	{
		if (player == null)
		{
			player = GameObject.FindGameObjectWithTag("Player");
		}
	}

	void Update()
	{
		// Detect "F" key when player is standing in the trigger
		if (playerInRange && !isFixed && Input.GetKeyDown(KeyCode.F))
		{
			StartCoroutine(RepairSequence());
		}
		else if (playerInRange && isFixed && !elevatorUp && Input.GetKeyDown(KeyCode.F))
		{
			StartCoroutine(RideSequence());
		}
	}

	IEnumerator RepairSequence()
	{
		isFixed = true;
		repairPrompt.SetActive(false);

		// 1. Fade to Black
		float elapsed = 0;
		while (elapsed < 0.5f)
		{
			fadeGroup.alpha = Mathf.Lerp(0, 1, elapsed / 0.5f);
			elapsed += Time.deltaTime;
			yield return null;
		}

		// 2. THE SWAP (Copy tiles from Template location to Broken location)
		for (int x = 0; x < fixedDimensions.x; x++)
		{
			for (int y = 0; y < fixedDimensions.y; y++)
			{
				Vector3Int sourcePos = new Vector3Int(fixedTemplateTopLeft.x + x, fixedTemplateTopLeft.y - y, 0);
				Vector3Int targetPos = new Vector3Int(brokenTopLeft.x + x, brokenTopLeft.y - y, 0);

				// 1. Get the tile asset
				TileBase templateTile = tilemap.GetTile(sourcePos);

				// 2. Get the transform matrix (this carries the flip/rotation info)
				Matrix4x4 sourceMatrix = tilemap.GetTransformMatrix(sourcePos);

				// 3. Apply the tile
				tilemap.SetTile(targetPos, templateTile);

				// 4. Apply the matrix so the new tile isn't flipped like the old one was
				tilemap.SetTransformMatrix(targetPos, sourceMatrix);
			}
		}

		ridePrompt.SetActive(true);
		yield return new WaitForSeconds(1f); // Drama pause

		// 3. Fade Back In
		elapsed = 0;
		while (elapsed < 0.5f)
		{
			fadeGroup.alpha = Mathf.Lerp(1, 0, elapsed / 0.5f);
			elapsed += Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator RideSequence()
	{
		elevatorUp = true;
		ridePrompt.SetActive(false);

		// 1. Fade to Black
		float elapsed = 0;
		while (elapsed < 0.5f)
		{
			fadeGroup.alpha = Mathf.Lerp(0, 1, elapsed / 0.5f);
			elapsed += Time.deltaTime;
			yield return null;
		}

		// 2. THE SWAP
		for (int x = 0; x < topPosDimensions.x; x++)
		{
			for (int y = 0; y < topPosDimensions.y; y++)
			{
				Vector3Int sourcePos = new Vector3Int(topPosTemplateTopLeft.x + x, topPosTemplateTopLeft.y - y, 0);
				Vector3Int targetPos = new Vector3Int(topPosTopLeft.x + x, topPosTopLeft.y - y, 0);

				// 1. Get the tile asset
				TileBase templateTile = tilemap.GetTile(sourcePos);

				// 2. Get the transform matrix (this carries the flip/rotation info)
				Matrix4x4 sourceMatrix = tilemap.GetTransformMatrix(sourcePos);

				// 3. Apply the tile
				tilemap.SetTile(targetPos, templateTile);

				// 4. Apply the matrix so the new tile isn't flipped like the old one was
				tilemap.SetTransformMatrix(targetPos, sourceMatrix);
			}
		}

		//player.GetComponent<Rigidbody2D>().position = new Vector2(newX, newY);
		if (player == null)
		{
			player = GameObject.FindGameObjectWithTag("Player");
		}

		if (player != null)
		{
			player.transform.position = new Vector2(newX, newY);
		}
		
		yield return new WaitForSeconds(1f); // Drama pause

		// 3. Fade Back In
		elapsed = 0;
		while (elapsed < 0.5f)
		{
			fadeGroup.alpha = Mathf.Lerp(1, 0, elapsed / 0.5f);
			elapsed += Time.deltaTime;
			yield return null;
		}
	}

	// Trigger detection for the "Press F" popup
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && !isFixed)
		{
			playerInRange = true;
			repairPrompt.SetActive(true);
		}
		else if (other.CompareTag("Player") && isFixed)
		{
			playerInRange = true;
			ridePrompt.SetActive(true);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			playerInRange = false;
			repairPrompt.SetActive(false);
			ridePrompt.SetActive(false);
		}
	}
}
