using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public static MenuManager Instance;

    [SerializeField]
    Menu[] menus;

    void Awake()
    {
        Instance = this;
    }

    public void OpenMenu(string menuName)
    {
        foreach(var menu in menus)
        {
            if(menu.menuName == menuName)
            {
                menu.Open();
            }
            else if(menu.isOpen)
            {
                CloseMenu(menu);
            }
        }
    }

    public void OpenMenu(Menu menu)
    {
        foreach(Menu m in menus)
        {
            if(m.isOpen)
            {
                CloseMenu(m);
            }
        }
        menu.Open();
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
