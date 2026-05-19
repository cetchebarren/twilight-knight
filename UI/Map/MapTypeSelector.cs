using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class MapTypeSelector : MonoBehaviour
    {
        public enum ButtonType
        {
            Local,
            World
        }

        public ButtonType mapType;

        public Image thisButtonImage;

        // Start is called before the first frame update
        void Start()
        {
            SetDefaultColor();
        }

        public void SetDefaultColor()
        {
            switch (mapType)
            {
                case ButtonType.Local:
                    SetColor_Selected();
                    break;
                case ButtonType.World:
                    SetColor_Unselected();
                    break;
                default:
                    break;
            }
        }

        public void SetColor_Selected()
        {
            thisButtonImage.color = Color.white;
        }

        public void SetColor_Unselected()
        {
            thisButtonImage.color = Color.grey;
        }

    }
}
