using UnityEngine;
using TMPro;

public class ControlsPopup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public GameObject interactPromptObject;

    [Header("Content")]
    public string popupTitle = "Controls";
    [TextArea(10, 20)] public string controlsText =
        "Move  -  A / D\n" +
        "Jump  -  Space\n" +
        "Dash  -  Shift\n" +
        "Attack  -  Left Click, 5 Hit Combo\n" +
        "Place Torch  -  E\n" +
        "Use Potion  -  Q\n" +
        "Interact  -  T";

    private bool playerInRange = false;
    private bool isOpen = false;

    void Start()
    {
        popupPanel.SetActive(false);

        if (interactPromptObject != null)
            interactPromptObject.SetActive(false);

        if (titleText == null) Debug.LogError("ControlsPopup: titleText is not assigned!");
        if (bodyText == null) Debug.LogError("ControlsPopup: bodyText is not assigned!");
        if (popupPanel == null) Debug.LogError("ControlsPopup: popupPanel is not assigned!");
    }

    void Update()
    {
        if (interactPromptObject != null)
            interactPromptObject.SetActive(playerInRange && !isOpen);

        if (Input.GetKeyDown(KeyCode.F) && playerInRange)
        {
            if (!isOpen)
                OpenPopup();
            else
                ClosePopup();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || other.gameObject == null) return;
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (isOpen) ClosePopup();
        }
    }

    void OpenPopup()
    {
        isOpen = true;
        popupPanel.SetActive(true);
        titleText.text = popupTitle;
        bodyText.text = controlsText;
    }

    void ClosePopup()
    {
        isOpen = false;
        popupPanel.SetActive(false);
    }
}