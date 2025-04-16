using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] Menu[] menus;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenMenu(string menuName)
    {
        foreach (var menu in menus)
        {
            if (menu.menuName == menuName)
            {
                OpenMenu(menu);
            }
            else if (menu.isOpen)
            {
                CloseMenu(menu);
            }
        }
    }

    public void OpenMenu(Menu menu)
    {
        foreach (var m in menus)
        {
            CloseMenu(m);
        }

        menu.Open();
        //Debug.Log($"Menu {menu.menuName} opened.");
    }

    public void CloseMenu(Menu menu)
    {
        if (!menu.isOpen) return;

        menu.Close();
        //Debug.Log($"Menu {menu.menuName} closed.");
    }


}


