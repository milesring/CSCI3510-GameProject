using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum RotationAxes{
//	MouseXAndY = 0,
//	MouseX = 1, 
//	MouseY = 2
//}
namespace Assets.Scripts.Player
{
    [Serializable]
    public class MouseLook
    {

        //public RotationAxes axes = RotationAxes.MouseXAndY;
        public float sensitivityHor = 9.0f;
        public float sensitivityVert = 9.0f;
        public bool clampVerticalRotation = true;
        public float minimumVert = -45.0f;
        public float maximumVert = 45.0f;
        public bool smooth;
        public float smoothTime = 5f;
		public GameObject rotationZone;
        //private float rotationX = 0;

		private CameraManager camManager;
        private Quaternion CharacterTargetRot;
        private Quaternion CameraTargetRot;

        // Use this for initialization
        public void Init(Transform character, Transform camera)
        {
			camManager = camera.GetComponent<CameraManager>();
            CharacterTargetRot = character.localRotation;
            CameraTargetRot = camera.localRotation;
            //Rigidbody body = GetComponent<Rigidbody>();
            //if (body != null)
            //{
            //    body.freezeRotation = true;
            //}
        }

        // Update is called once per frame
        public void LookRotation(Transform character, Transform camera)
        {
            //if (axes == RotationAxes.MouseX) {
            //	transform.Rotate (0, Input.GetAxis("Mouse X")*sensitivityHor, 0);
            //} else if (axes == RotationAxes.MouseY) {
            //	//vertical rotation 
            //	rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
            //	rotationX = Mathf.Clamp (rotationX, minimumVert, maximumVert);

            //	float rotationY = transform.localEulerAngles.y;

            //	transform.localEulerAngles = new Vector3 (rotationX, rotationY);
            //} else {
            //	//both horiz and vert rotation
            //	rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
            //	rotationX = Mathf.Clamp (rotationX, minimumVert, maximumVert);

            //	float delta = Input.GetAxis ("Mouse X") * sensitivityHor;
            //	float rotationY = transform.localEulerAngles.y + delta;
            //	transform.localEulerAngles = new Vector3 (rotationX, rotationY, 0);
            //}
            float yRot = Input.GetAxis("Mouse X") * sensitivityHor;
            //float xRot = Input.GetAxis("Mouse Y") * sensitivityVert;
			float xRot = Input.GetAxis("Mouse Y") / sensitivityVert * Mathf.Rad2Deg / 25;
            
			CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                CameraTargetRot = ClampRotationAroundXAxis(CameraTargetRot);

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = CharacterTargetRot;
				if (camManager.inFP) {
					camera.localRotation = CameraTargetRot;
				} else {
					float currentCamRotation = camera.transform.localRotation.eulerAngles.x - 360;
					if (currentCamRotation < -300f) {
						currentCamRotation = currentCamRotation + 360;
					}
					if (-xRot + currentCamRotation < maximumVert && currentCamRotation - xRot > minimumVert) {
						camera.transform.RotateAround (rotationZone.transform.position, rotationZone.transform.right, -xRot);
					}
					if (currentCamRotation < minimumVert) {
						currentCamRotation = minimumVert;
					}
					if (currentCamRotation > maximumVert) {
						currentCamRotation = maximumVert;
					}
				}
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, minimumVert, maximumVert);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}
