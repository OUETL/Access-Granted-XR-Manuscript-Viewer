using System.Collections.Generic;
using UnityEngine;
using TextAction = System.Collections.Generic.KeyValuePair<string, System.Action>;
using MovementTarget = OU.OVAL.Core.Constants.MovementTarget;
using UnityEngine.UIElements;
//using UnityEngine.UI;

namespace OU.OVAL
{
    public class MovePanel : MonoBehaviour
    {
        [Tooltip("Handler for changing which element in the scene the user is moving")]
        public MovementSelector movementSelector;

        //public Dropdown moveDropdown;
        public Button positionResetButton;
        public Slider motionScaleSlider;
      //  public Text motionScaleText;

        [Tooltip("Light indicator object")]
        public GameObject lightIndicator;

        List<TextAction> movementHandlers = new List<TextAction>();
        MovementTarget saveMovementTarget;

/*        void SetOptions(Dropdown d, List<TextAction> l)
        {
            var o = new List<Dropdown.OptionData>();
            foreach (var ta in l) o.Add(new Dropdown.OptionData(ta.Key));
            d.ClearOptions();
            d.AddOptions(o);
        }*/
        void SetActiveTarget( MovementTarget what )
        {
            if (lightIndicator) lightIndicator.SetActive(what == MovementTarget.Light);
            if (movementSelector) movementSelector.Activate(what);
        }

        private void OnEnable()
        {
            SetActiveTarget(saveMovementTarget);
        }
        private void OnDisable()
        {
            // Store current movement target, then enable player movement.
            // This avoids the player geting "stuck" if joining a room as
            // non-master client while not in player move mode, as will be
            // unable to then change mode as control panel hidden.
            if (movementSelector)
            {
                saveMovementTarget = movementSelector.activeTarget;
                SetActiveTarget(MovementTarget.Player);
            }
        }

        void SetupUI()
        {
            // Set up motion selector dropbox
/*            if (moveDropdown)
            {
                movementHandlers = new List<TextAction>()
                {
                    new TextAction("Move Yourself", delegate () { SetActiveTarget(MovementTarget.Player); }),
                    new TextAction("Move Model",  delegate ()   { SetActiveTarget(MovementTarget.Model); }),
                    new TextAction("Move Light",  delegate ()   { SetActiveTarget(MovementTarget.Light); }),
                };
                SetOptions(moveDropdown, movementHandlers);
            }*/
        }

        void Start()
        {
            // Set up motion selector dropbox
/*            if (moveDropdown == null) Debug.LogWarning("No move dropdown specified!");
            else moveDropdown.onValueChanged.AddListener(OnMovementSelect);*/

            // Set up move reset button
            if (positionResetButton == null) Debug.LogWarning("No position reset button specified!");
            else positionResetButton.clicked += OnPositionResetButton;
            //else positionResetButton.onClick.AddListener(OnPositionResetButton);

                // Check movement selector
            if (movementSelector == null) Debug.LogWarning("No movement selector specified!");
            else saveMovementTarget = movementSelector.activeTarget;

            var motionScaleFactor = Core.Common.Instance.config.motionScaleFactor;

            // Check motion scale factor slider
            if (motionScaleSlider == null) Debug.LogWarning("No motion scale slider specified!");
            else
            {
/*                motionScaleSlider.onValueChanged.AddListener( delegate {
                    var val = motionScaleSlider.value;
                    Core.Common.Instance.config.motionScaleFactor = val;
                    if (motionScaleText != null) motionScaleText.text = $"{val:F2}x";
                });*/
                // Should invoke the handler above to set the text.
                motionScaleSlider.value = motionScaleFactor;
            }

            // Check motion scale factor text
           // if (motionScaleText == null) Debug.LogWarning("No motion scale text specified!");

            SetupUI();
        }

        void OnMovementSelect(int choice)
        {
            movementHandlers[choice].Value();
        }

        void OnPositionResetButton()
        {
            movementSelector?.ResetPosition();
        }
    }
}
