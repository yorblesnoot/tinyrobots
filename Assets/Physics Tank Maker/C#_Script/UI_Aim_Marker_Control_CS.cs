using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace ChobiAssets.PTM
{
	
	[DefaultExecutionOrder (+2)] // (Note.) This script is executed after the "Aiming_Control_CS", in order to move the marker smoothly.
    public class UI_Aim_Marker_Control_CS : MonoBehaviour
	{
        /*
		 * This script is attached to the "MainBody" of the tank.
		 * This script controls the 'Aim_Marker' in the scene.
         * This script works in combination with "Aiming_Control_CS" in the tank.
		*/

        // User options >>
        public string Aim_Marker_Name = "Aim_Marker";
		// << User options


		Aiming_Control_CS aimingScript;
		Image markerImage;
		Transform markerTransform;

		bool isSelected;


		void Start()
        {
            // Get the marker image in the scene.
            if (string.IsNullOrEmpty(Aim_Marker_Name))
            {
                return;
            }
            GameObject markerObject = GameObject.Find(Aim_Marker_Name);
            if (markerObject)
            {
                markerImage = markerObject.GetComponent<Image>();
            }
            else
            {
                // The marker cannot be found in the scene.
                Debug.LogWarning(Aim_Marker_Name + " cannot be found in the scene.");
                Destroy(this);
                return;
            }
            markerTransform = markerImage.transform;

            // Get the "Aiming_Control_CS" in the tank.
            aimingScript = GetComponent<Aiming_Control_CS>();
            if (aimingScript == null)
            {
                Debug.LogWarning("'Aiming_Control_CS' cannot be found in the MainBody.");
                Destroy(this);
            }
        }


        void Update()
        {
            if (isSelected == false)
            {
                return;
            }

            Marker_Control();
        }


        void Marker_Control()
        {
            // Check the aiming mode.
            if (aimingScript.Mode == 0)
            { // Not aiming now.

                // Disable the marker.
                markerImage.enabled = false;
                return;
            }

            // Enable the marker.
            markerImage.enabled = true;

            // Check locking on the target now.
            if (aimingScript.Target_Transform)
            { // Locking on the target now.

                // Make the marker red.
                markerImage.color = Color.red;

                // Set the marker on the target.
                var currentPosition = Camera.main.WorldToScreenPoint(aimingScript.Target_Position);
                if (currentPosition.z < 0.0f)
                { // Behind of the camera.

                    // Disable the marker.
                    markerImage.enabled = false;
                }
                else
                { // Front of the camera.
                    currentPosition.z = 128.0f;
                    markerTransform.position = currentPosition;
                }
            }
            else
            { // Not locking on now.

                // Make the marker white.
                markerImage.color = Color.white;

                // Check the player is using the gun camera now.
                if (aimingScript.reticleAimingFlag)
                { // Using the gun camera now.

                    // Set the marker at the center of the screen.
                    markerTransform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 128.0f);
                }
                else
                { // Not using the gun camera now.

                    // Set the marker at the center of the screen with offset.
                    markerTransform.position = new Vector3(Screen.width * 0.5f, Screen.height * (0.5f + General_Settings_CS.Aiming_Offset), 128.0f);
                }
            }
        }


        void Selected(bool isSelected)
        { // Called from "ID_Settings_CS".
            if (isSelected)
            {
                this.isSelected = true;
            }
            else
            {
                if (this.isSelected)
                { // This tank is selected until now.
                    this.isSelected = false;
                    markerImage.enabled = false;
                }
            }
        }


        void MainBody_Destroyed_Linkage()
        { // Called from "Damage_Control_Center_CS".

            // Turn off the marker.
            if (isSelected)
            {
                markerImage.enabled = false;
            }

            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}
