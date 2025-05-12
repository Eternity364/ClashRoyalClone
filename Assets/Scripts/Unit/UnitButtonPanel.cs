using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units {
    public class UnitButtonPanel : MonoBehaviour
    {
        
        [SerializeField]
        Unit[] units = new Unit[4];

        private UnitButton[] buttons;

        
        void Start()
        {
            buttons = GetComponentsInChildren<UnitButton>();
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i < units.Length)
                {
                    buttons[i].SetValue(units[i]);
                }
            }
        }
    }
}
