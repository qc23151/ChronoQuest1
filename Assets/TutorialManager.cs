using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    // setting the order of tutorial steps
    public enum TutorialStep
    {
        Movement,
        Attack,
        Rewind, 
        Complete
    }

    public TutorialStep currentStep; 
    
    public GameObject rewindHint;
    public GameObject attackHint;
    public GameObject movementHint;

    public Transform player;
    public Transform enemy; 
    public float attackDistance = 10f;

    bool moveCompleted = false;
    bool attackCompleted = false;
    bool rewindCompleted = false;

    void Start()
    {
        DisableHints();
        SetStep(TutorialStep.Movement);         // set movement hint as first step in tutorial
    }

    void Update()
    {
        CheckAttackDistance();
        
        if (moveCompleted && attackCompleted && rewindCompleted)
        {
            SetStep(TutorialStep.Complete);
            Debug.Log("Tutorial Complete!");
            DisableHints();
        }
    }

    void CheckAttackDistance()
    {
        float distance = Vector2.Distance(player.position, enemy.position);

        if (distance <= attackDistance)
        {
            attackHint.SetActive(true);
        }
    }

    public void OnPlayerMoved()
    {
        if (currentStep == TutorialStep.Movement && !moveCompleted)
        {
            moveCompleted = true; 
            movementHint.SetActive(false);
            Debug.Log("Player movement tutorial complete");

        }
    }

    public void OnPlayerAttack()
    {
        if (currentStep == TutorialStep.Attack && !attackCompleted)
        {
            attackCompleted = true;
            attackHint.SetActive(false);
            Debug.Log("Player attack tutorial complete");
        }
    }

    public void OnPlayerRewind()
    {
        if (currentStep == TutorialStep.Rewind && !rewindCompleted)
        {
            rewindCompleted = true;
            rewindHint.SetActive(false);
            Debug.Log("Player rewind tutorial complete");
        }
    }

    // setting the current tutorial step and showing corresponding hint
    void SetStep(TutorialStep step)
    {
        currentStep = step;

        movementHint.SetActive(step == TutorialStep.Movement);
        attackHint.SetActive(step == TutorialStep.Attack);
        rewindHint.SetActive(step == TutorialStep.Rewind);
    }

    void DisableHints()
    {
        rewindHint.SetActive(false);
        attackHint.SetActive(false);
        movementHint.SetActive(false);
    }
}
