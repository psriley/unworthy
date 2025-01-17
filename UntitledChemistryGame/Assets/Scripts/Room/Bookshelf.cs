using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bookshelf : Trigger
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

    public GameObject book;
    public GameObject prerequisiteTrigger;
    public DialogueItem dialogue;
    public Quest questToComplete;

    private GameManager gm;
    private DialogueManager dm;
    private bool canPlaceBook;

    protected override void Start()
    {
        base.Start();
        gm = FindObjectOfType<GameManager>();
        dm = gm.gameObject.GetComponent<DialogueManager>();
        canPlaceBook = false;
        book.SetActive(false);
        prerequisiteTrigger.SetActive(false);
    }

    private void Update()
    {
        if (canPlaceBook && Input.GetKeyDown(promptKeyText))
        {
            dm.PlayDialogue(dialogue);
            book.SetActive(true);
            base.HidePrompt();
            base.noPrompt = true;
            prerequisiteTrigger.SetActive(true);
            // make it so that the player can't open the book
            gm.RemoveBook();
            gm.qm.CompleteQuest(questToComplete);
        }
    }

    public override void TriggerEnterEvent()
    {
        canPlaceBook = true;
    }

    public override void TriggerExitEvent()
    {
        canPlaceBook = false;
    }
}
