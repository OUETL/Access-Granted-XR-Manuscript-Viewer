using UnityEngine;

/*
 * This class simply activates/deactivates various movement controllers to allow the user
 * to control specific aspects of the scene.
 */

namespace OU.OVAL
{
    public class MovementSelector : MonoBehaviour
    {
        [Tooltip("Player, model, and light movement controller objects")]
        public GameObject playerMovementController, modelMovementController, lightMovementController;

        [Tooltip("Currently active target")]
        public Core.Constants.MovementTarget activeTarget = Core.Constants.MovementTarget.Player;

        private GameObject activeGameObject = null;
        private GameObject storeActiveGameObject = null;

        // Use this for initialization
        void Start()
        {
            if (playerMovementController == null) Debug.LogWarning("No GameObject assigned to player!");
            if (modelMovementController == null) Debug.LogWarning("No GameObject assigned to model!");
            if (lightMovementController == null) Debug.LogWarning("No GameObject assigned to light!");

            Activate(activeTarget);
        }

        void Halt()
        {
            storeActiveGameObject = activeGameObject;
        }
        void Resume()
        {
            activeGameObject = storeActiveGameObject;
        }

        public void Activate(Core.Constants.MovementTarget who)
        {
            activeGameObject = null;

            playerMovementController.SetActive(false);
            modelMovementController.SetActive(false);
            lightMovementController.SetActive(false);

            switch (who)
            {
                case Core.Constants.MovementTarget.Model:
                    activeGameObject = modelMovementController;
                    break;

                case Core.Constants.MovementTarget.Light:
                    activeGameObject = lightMovementController;
                    break;

                default:
                    activeGameObject = playerMovementController;
                    break;
            }

            activeTarget = who;
            activeGameObject.SetActive(true);
        }

        public void ResetPosition()
        {
/*            BaseMovementController b = activeGameObject?.GetComponent<BaseMovementController>();
            if (b) b.ResetPosition();*/
        }
    }

}
