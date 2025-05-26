using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public bool isOpen;
    public string menuName;

    public void Open()
    {
        if (isOpen) return; 

        isOpen = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        if (!isOpen) return; 

        isOpen = false;
        gameObject.SetActive(false);

    }
}
