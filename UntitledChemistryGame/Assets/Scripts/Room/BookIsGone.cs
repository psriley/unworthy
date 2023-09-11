using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookIsGone : Trigger
{
    [SerializeField] private KeyCode _promptKeyText;
    public override KeyCode promptKeyText
    {
        get { return _promptKeyText; }
        set { _promptKeyText = value; }
    }
    [SerializeField] private EventDependency _eventDependency;
    public override EventDependency eventDependency
    {
        get { return _eventDependency; }
        set { _eventDependency = value; }
    }
    public bool noPrompt;
    public DialogueItem dialogue;
    public GameObject prerequisiteTrigger;

    private GameManager gm;
    private DialogueManager dm;
    private bool canTriggerDialogue;
    private PlayerController controller;

    protected override void Start()
    {
        base.Start();
        gm = FindObjectOfType<GameManager>();
        dm = gm.gameObject.GetComponent<DialogueManager>();
        //controller = gm.player.GetComponentInChildren<PlayerController>();
        prerequisiteTrigger.SetActive(false);
    }

    private void Update()
    {
        if (canTriggerDialogue && Input.GetKeyDown(promptKeyText))
        {
            dm.PlayDialogue(dialogue);
            //controller.enabled = false;
            base.HidePrompt();
            base.noPrompt = true;
            prerequisiteTrigger.SetActive(true);
        }
    }

    public override void TriggerEnterEvent()
    {
        canTriggerDialogue = true;
    }

    public override void TriggerExitEvent()
    {
        canTriggerDialogue = false;
    }
}