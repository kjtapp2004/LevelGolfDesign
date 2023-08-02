using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

	private GameObject playerCam;
	private GameObject playerStart;
	private GameObject playerGoal;

	private GameObject camPos;
	private GameObject ballPos;

	public GameObject ball;
	public GameObject pointer;
	private Rigidbody ballRB;

	public Vector3 shotAngleVectorDefault = new Vector3 (-2.0f, 0.15f, 0.0f);
	private Vector3 shotAngleVector;
	private float shotAngle = 0.0f;
	private float mouseMoveCumulativeTotal;
	private float shotForce;

	public float minStopSpeed;
	public float forceRangeMin;
	public float forceRangeMax;
	public float forcePingPongSpeed;
	public float angleMultiplier;

	public bool isShooting = false;
	public bool afterShot = false;
	private int pingPongDir = 1;
	public bool disableInput = false;
	private int shotCount = 0;

	//----- will get dropped if prefab removed from scene

	public Slider powerSlider;
	public Text shotCountText;
	public Text parText;

	public GameObject goalObject;
	public GameObject zKillObject;

	//---------------------------------------------------

	[SerializeField] private GameObject b;
	private GameObject p;

	private Vector3 lastPos;

	public enum gameState {waiting,aiming,shooting};
	public gameState currentState = gameState.waiting;

	void Start ()
	{
		// should auto assign UI elements too at some point
		// slider
		// shot count text
		// par text

		playerCam = GameObject.FindGameObjectsWithTag("MainCamera")[0];
		playerStart = GameObject.FindGameObjectsWithTag("Start")[0];
		playerGoal = GameObject.FindGameObjectsWithTag("Goal")[0];

		// MYLES GETTING REF
		_mainCamera = Camera.main;
		_rayDetector = GameObject.FindGameObjectWithTag("RayDetector").GetComponent<Collider>();
		// END OF MYLES GETTING REF

		ballPos = playerStart.transform.GetChild (1).gameObject;		/// this seems super fragile, should probably fix this
		camPos = playerStart.transform.GetChild (2).gameObject;

		playerCam.transform.position = camPos.transform.position;
		playerCam.transform.rotation = camPos.transform.rotation;

		// UNCOMMENT FOR ORIGINAL
		//b = Instantiate (ball, ballPos.transform.position, ballPos.transform.rotation) as GameObject;
		p = Instantiate (pointer, b.transform.position, b.transform.rotation) as GameObject;

		p.SetActive (false);

		ballRB = b.GetComponent<Rigidbody> ();

		playerCam.GetComponent<LookAtObject> ().target = b;

		shotForce = forceRangeMin;
		shotCountText.text = "0";
		shotAngleVector = shotAngleVectorDefault;

		parText.text = goalObject.GetComponent<Hole> ().par.ToString();
	}

	void FixedUpdate()
	{
		// MYLES FIXED UPDATE - TURN OFF IF WANT ORIGINAL
		MylesFixedUpdate();
	}

	void Update ()
	{
		// MYLES UPDATE - TURN OFF IF WANT ORIGINAL
		MylesUpdate();

		// ORIGINAL UPDATE - TURN ON IF WANT ORIGINAL
		//OriginalUpdate();
	}

	//-------------------------------------------------

	private void OriginalUpdate()
	{
		shotAngleVector = Quaternion.AngleAxis(shotAngle, Vector3.up) * shotAngleVector;        // calculate this properly next

		if (currentState == gameState.waiting)
		{
			if (disableInput == false)
			{
				if (Input.GetMouseButtonDown(0))
				{
					StartShot();
					currentState = gameState.aiming;
				}
			}
		}
		else if (currentState == gameState.aiming)
		{
			DoShot();
			if (Input.GetMouseButtonUp(0))
			{
				EndShot();
				currentState = gameState.shooting;
			}
		}
		else if (currentState == gameState.shooting)
		{
			CheckBall();
		}

		CheckHeight();
	}

	private void StartShot()
	{
		isShooting = true;
		disableInput = true;
		p.SetActive (true);
	}

	/// <summary>
	/// Myles Math for Ball Aiming
	/// </summary>
	[Header("MYLES MATH VARIABLES")]
	[SerializeField] private SOFloat _maxForce;
	[SerializeField] private SOFloat _forceModifier;
	[SerializeField] private SOFloat _minStopSpeed;
	[SerializeField] private SOFloat _gravity;
	[SerializeField] private LayerMask _rayLayer;
	[SerializeField] private Collider _rayDetector;
	[SerializeField] private LineRenderer _lineRenderer;
	[SerializeField] private GameObject _areaAffector;
	private Camera _mainCamera;
	private float _force;
	private bool _hasStopped = true;
	private bool _showAffector = true;
	private Vector3 _startPos, _endPos;
	private Vector3 _direction;
	private void MylesFixedUpdate()
	{
		if (ballRB == null) return;
		Physics.gravity = new Vector3(0, _gravity.Value, 0);
		if (isShooting)
		{
			isShooting = false;
			_direction = _startPos - _endPos;
			ballRB.AddForce(_direction * _force, ForceMode.Impulse);
			_force = 0;
			_startPos = _endPos = Vector3.zero;
		}

		if (ballRB.velocity == Vector3.zero)
		{
			_showAffector = true;
			_hasStopped = true;
		}
		else
		{
			_showAffector = false;
			_hasStopped = false;
		}
	}

	private void MylesUpdate()
	{
		if (_showAffector)
			_areaAffector.SetActive(true);
		else
			_areaAffector.SetActive(false);

		// ORIGINAL
		CheckBall();
		CheckHeight();

		if (!_hasStopped) return;
		if (Input.GetMouseButtonDown(0) && !isShooting)
		{
			_rayDetector.enabled = true;
			_startPos = ClickedPoint();

			_lineRenderer.gameObject.SetActive(true);
			_lineRenderer.SetPosition(0, b.transform.localPosition);
		}

		if (Input.GetMouseButton(0))
		{			
			_endPos = ClickedPoint();
			if (_endPos != Vector3.zero)
			{
				_areaAffector.SetActive(false);
				_force = Mathf.Clamp(Vector3.Distance(_endPos, _startPos) * _forceModifier.Value, 0, _maxForce.Value);
				_lineRenderer.gameObject.SetActive(true);
				_lineRenderer.SetPosition(1, transform.InverseTransformPoint(new Vector3(_endPos.x, _endPos.y, _endPos.z)));
			}
			else
			{
				_lineRenderer.gameObject.SetActive(false);
			}


        }

		if (Input.GetMouseButtonUp(0))
		{
			_endPos = ClickedPoint();
			if (_endPos != Vector3.zero)
			{
				_lineRenderer.gameObject.SetActive(false);
				_rayDetector.enabled = false;
				lastPos = b.transform.position;
				isShooting = true;
				shotCount++;
				shotCountText.text = shotCount.ToString();
			}


        }
	}

	private Vector3 ClickedPoint()
	{
		Vector3 pos = Vector3.zero;
		var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, _rayLayer))
		{
			pos = hit.point;
		}
		return pos;
	}

	private void DoShot2()
	{
		if (!isShooting) return;

	}
	/// <summary>
	/// END OF Myles Math for Ball Aiming
	/// </summary>

	private void DoShot()
	{
		if (isShooting == true)
			shotForce += forcePingPongSpeed * pingPongDir;

		if (shotForce >= forceRangeMax)
			pingPongDir = -1;

		if (shotForce <= forceRangeMin)
			pingPongDir = 1;

		powerSlider.value = (shotForce - forceRangeMin) / (forceRangeMax - forceRangeMin);

		mouseMoveCumulativeTotal = (Input.GetAxis ("Mouse X"));
		shotAngle = mouseMoveCumulativeTotal * angleMultiplier;

		p.transform.position = b.transform.position;
		p.transform.rotation = Quaternion.LookRotation (shotAngleVector, Vector3.up);

	}

	private void EndShot()
	{
		lastPos = b.transform.position;
		p.SetActive (false);
		isShooting = false;
		afterShot = true;
		ballRB.AddForce (shotAngleVector * shotForce);	// this is where the ball gets hit
		shotForce = forceRangeMin;
		powerSlider.value = 0.0f;
		shotCount++;
		shotCountText.text = shotCount.ToString();
	}

	//-------------------------------------------------

	private void CheckBall()
	{
		if (ballRB.velocity.magnitude <= _minStopSpeed.Value)
		{
			ballRB.velocity = Vector3.zero;
			ballRB.angularVelocity = Vector3.zero;

			if (afterShot == true)
			{
				disableInput = false;
				afterShot = false;
			}
			
			shotAngleVector = shotAngleVectorDefault;
			currentState = gameState.waiting;
		}
	}

	private void CheckHeight()
	{
		if (b.transform.position.y <= zKillObject.transform.position.y)
		{
			b.transform.position = lastPos;
			ballRB.velocity = Vector3.zero;
			ballRB.angularVelocity = Vector3.zero;
		}
	}
}
