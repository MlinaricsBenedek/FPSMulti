using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public bool isOpen;
    public string menuName;

    public void Open()
    {
        if (isOpen) return; // M?r nyitva van

        isOpen = true;
        gameObject.SetActive(true);
        //Debug.Log($"Menu {menuName} is now open.");
    }

    public void Close()
    {
        if (!isOpen) return; // M?r z?rva van

        isOpen = false;
        gameObject.SetActive(false);
        //Debug.Log($"Menu {menuName} is now closed.");
    }
}
