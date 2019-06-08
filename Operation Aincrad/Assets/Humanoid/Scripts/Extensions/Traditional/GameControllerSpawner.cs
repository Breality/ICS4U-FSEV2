using System.Collections;
using UnityEngine;

namespace Passer {

    public class GameControllerSpawner : HumanoidSpawner {
        private int[] joystickAssignment;

        private const int maxJoysticks = 4;

        public void Awake() {
            spawnedHumanoids = new HumanoidControl[maxJoysticks];
            joystickAssignment = new int[maxJoysticks];
            for (int i = 0; i < maxJoysticks; i++)
                joystickAssignment[i] = -1;
        }

        public override void Start() {
            // enable component checkbox
        }

        public void OnEnable() {
            StartCoroutine(CheckJoysticks());
        }

        private IEnumerator CheckJoysticks() {
            while (enabled) {
                int joystickCount = JoystickCount();

                if (joystickCount > nHumanoids) {
                    int joystickID = FindNewJoystick();
                    if (joystickID != -1)
                        OnJoystickAppears(joystickID);

                } else if (joystickCount < nHumanoids) {

                    int joystickID = FindVanishedJoystick();
                    if (joystickID != -1)
                        OnJoystickDisappears(joystickID);
                }

                yield return new WaitForSeconds(1F);
            }
        }

        private int FindNewJoystick() {
            string[] joystickNames = Input.GetJoystickNames();
            for (int i = 0; i < maxJoysticks; i++) {
                if (joystickAssignment[i] == -1 && joystickNames[i] != null) {
                    return i;
                }
            }
            return -1;
        }

        private int FindVanishedJoystick() {
            string[] joystickNames = Input.GetJoystickNames();
            int n = joystickNames.Length < maxJoysticks ? joystickNames.Length : maxJoysticks;

            for (int i = 0; i < n; i++) {
                if (joystickAssignment[i] != -1 && string.IsNullOrEmpty(joystickNames[i])) {
                    return i;
                }
            }
            return -1;
        }

        private int JoystickCount() {
            string[] joystickNames = Input.GetJoystickNames();
            int joystickCount = 0;
            for (int i = 0; i < joystickNames.Length; i++) {
                if (joystickNames[i].Length > 0)
                    joystickCount++;
            }
            return joystickCount;
        }

        private void OnJoystickAppears(int joystickID) {
            Debug.Log("new joystick connected:");

            if (nHumanoids > maxJoysticks)
                return;

            HumanoidControl humanoid = SpawnHumanoid();
            if (humanoid == null)
                return;

            humanoid.gameControllerEnabled = true;
            HumanoidControl.SetControllerID(humanoid, joystickID);

            spawnedHumanoids[joystickID] = humanoid;
            joystickAssignment[joystickID] = joystickID;
            nHumanoids++;
        }

        private void OnJoystickDisappears(int joystickID) {
            Debug.Log("joystick disconnected");
            DestroyHumanoid(spawnedHumanoids[joystickID]);

            spawnedHumanoids[joystickID] = null;
            joystickAssignment[joystickID] = -1;
            nHumanoids--;
        }
    }
}