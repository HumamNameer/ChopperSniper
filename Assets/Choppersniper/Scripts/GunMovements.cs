using UnityEngine;
using System.Collections;

public class GunMovements : MonoBehaviour {


	public float moveAmount = 1.0f;
	public float moveSpeed = 2.0f;

	public GameObject gunModel;
	public GameObject cameraModel;

	public float moveOnX = 0.0f;
	public float MoveonY = 0.0f;

	public Vector3 defaultPos;
	public Vector3 newGunPos;

	public Quaternion defaultRot;
	public Quaternion newGunRot;

	public bool OnOff = false;

	public float recoilAmount = 0.0f;
	public float amountToRecoil = 0.0f;

	public float recoilMultiplier = 1.0f;

	public float recoilMax = 0.0f;
	public float recoilMin = 0.0f;
	public float rotationMultiplier = 10.0f;

	void Awake(){
		newGunRot = defaultRot;
		defaultPos = transform.localPosition;
		OnOff = true;

	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if(Input.GetMouseButtonDown(1)){
			OnOff = false ;
		}

		if(Input.GetMouseButtonUp(2)){

			OnOff = true;
		}

		if(Input.GetMouseButton(0)){
			recoilAmount += amountToRecoil;

		}

		if(recoilAmount >= recoilMax){
			recoilAmount = recoilMax;
		}

		if(recoilAmount <= recoilMin){

			recoilAmount = recoilMin;
		}
		if(!Input.GetMouseButtonDown(0)){
			recoilAmount -= (amountToRecoil / 2 );
		}
	}

	void LateUpdate() {

		if(OnOff == true){

			moveOnX = Input.GetAxis("Mouse X") * Time.deltaTime*moveAmount;
			MoveonY = Input.GetAxis("Mouse Y") * Time.deltaTime * moveAmount;

			newGunPos = new Vector3(defaultPos.x+ moveOnX, defaultPos.y +MoveonY,defaultPos.z - (recoilAmount * recoilMultiplier ));
			gunModel.transform.localPosition = Vector3.Lerp(gunModel.transform.localPosition , newGunPos , moveSpeed*Time.deltaTime);
		}

		newGunRot = Quaternion.Euler(defaultRot.x, defaultRot.y, defaultRot.z - (recoilAmount * recoilMultiplier) * recoilMultiplier);
		gunModel.transform.localRotation = newGunRot;

		}


}


