using UnityEngine;
using TMPro;
using System.Collections;

public class HermitDialogue : MonoBehaviour
{
    [Header("Player References")]
    public PlayerMovement playerMovement; 
    public PlayerCombat playerCombat;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public GameObject shopPanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;
    public GameObject interactPromptObject;
    public GameObject goldHudObject;

    [Header("Typewriter Effect")]
    public float typeSpeed = 0.03f;

    [Header("Intro Dialogue")]
    [TextArea] public string[] introDialogue = {
        "Ah, a visitor! It has been some time since anyone has wandered this far up the path...",
        "I am what remains of the mining operation that once thrived below. They called me the Hermit.",
        "The mine goes deeper than anyone charted. Strange things took root down there after the collapse.",
        "The miners who went back never returned. I stayed up here. Safer that way.",
        "If you are heading down I can offer you supplies. Come back whenever you need something."
    };

    [Header("Shop Greetings")]
    [TextArea] public string[] shopGreetings = {
        "Back again? What do you need?",
        "The cave claims another run, does it? Stock up while you can.",
        "I heard something howling from the shaft last night. Take care down there.",
        "Potions are fresh. Torches too. Take what you need.",
        "You look like you have seen better days. Let me help with that."
    };

    private Transform player;
    private bool playerInRange = false;
    private bool introComplete = false;
    private bool isDialogueOpen = false;
    private bool isTyping = false;
    private bool skipTyping = false;
    private int currentLineIndex = 0;

    void Start()
    {
        // find player transform for distance checks
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("No GameObject tagged Player found!");

        // clear everything at start
        dialoguePanel.SetActive(false);
        shopPanel.SetActive(false);
        promptText.text = "";

        if (interactPromptObject != null)
            interactPromptObject.SetActive(false);

        introComplete = PlayerPrefs.GetInt("HermitIntroComplete", 0) == 1;

        Debug.Log($"Intro complete: {introComplete}");
    }

    void Update()
    {
        if (interactPromptObject != null)
            interactPromptObject.SetActive(playerInRange && !isDialogueOpen && !shopPanel.activeSelf);

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (playerInRange && !isDialogueOpen && !shopPanel.activeSelf)
                OpenDialogue();
            else if (isDialogueOpen)
            {
                if (isTyping)
                    skipTyping = true;
                else
                    AdvanceDialogue();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // check the collider and its game object still exist before accessing them
        if (other == null || other.gameObject == null) return;
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (isDialogueOpen) CloseDialogue();
            if (shopPanel != null && shopPanel.activeSelf) CloseShop();
        }
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;
    }

    void OpenDialogue()
    {
        isDialogueOpen = true;
        currentLineIndex = 0;

        speakerNameText.text = "";
        dialogueText.text = "";
        promptText.text = "";

        dialoguePanel.SetActive(true);
        speakerNameText.text = "Hermit";

        // disable player control while talking
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            playerMovement.body.linearVelocity = Vector2.zero;
            playerMovement.animator.SetBool("isRunning", false);
            playerMovement.animator.SetBool("isDashing", false);
        }
        if (playerCombat != null) playerCombat.enabled = false;

        if (!introComplete)
        {
            ShowLine(introDialogue[currentLineIndex]);
        }
        else
        {
            ShowLine(shopGreetings[Random.Range(0, shopGreetings.Length)]);
        }
    }

    void ShowLine(string line)
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        skipTyping = false;
        dialogueText.text = "";
        promptText.text = "";

        Debug.Log($"Typing line: {line}");

        foreach (char c in line)
        {
            if (skipTyping)
            {
                dialogueText.text = line;
                break;
            }
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        skipTyping = false;
        promptText.text = "Press F to continue";
    }

    void AdvanceDialogue()
    {
        if (!introComplete)
        {
            currentLineIndex++;

            if (currentLineIndex < introDialogue.Length)
                ShowLine(introDialogue[currentLineIndex]);
            else
            {
                introComplete = true;
                PlayerPrefs.SetInt("HermitIntroComplete", 1);
                CloseDialogueOpenShop();
            }
        }
        else
        {
            CloseDialogueOpenShop();
        }
    }

    void CloseDialogue()
    {
        StopAllCoroutines();
        isDialogueOpen = false;
        dialoguePanel.SetActive(false);
        speakerNameText.text = "";
        dialogueText.text = "";
        promptText.text = "";
    }

    void CloseDialogueOpenShop()
    {
        CloseDialogue();
        OpenShop();
    }

    void OpenShop()
    {
        shopPanel.SetActive(true);
        if (goldHudObject != null)
            goldHudObject.SetActive(false);
        GetComponent<HermitShop>()?.RefreshUI();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        promptText.text = "";
        if (goldHudObject != null)
            goldHudObject.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;
    }
}