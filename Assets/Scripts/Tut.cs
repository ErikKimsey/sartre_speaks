using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tut : MonoBehaviour
{

    public Image m_AddComponentToGameObject;
    public TextMeshProUGUI m_TextElement;

    public Button m_DisplayHideTutButton;


    void Awake()
    {
        m_AddComponentToGameObject.gameObject.SetActive(false);
        m_TextElement.gameObject.SetActive(false);
    }

    public void DisplayTutImage()
    {
        bool show = !m_AddComponentToGameObject.gameObject.activeSelf;
        string buttonValue = show == true ? "Hide Tut" : "Show Tut";
        m_DisplayHideTutButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonValue;
        m_AddComponentToGameObject.gameObject.SetActive(show);
        m_TextElement.gameObject.SetActive(show);
    }

}
