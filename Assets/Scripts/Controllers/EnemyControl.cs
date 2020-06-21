using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

using static Fighter;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Equipment))]
[RequireComponent(typeof(Fighter))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyControl : MonoBehaviour
{
    [System.Serializable]
    public struct AbilityManual
    {
        public float choiceWeighting;
        public float engageRange;
        public float holdDuration;
        public bool repeatable;
    }

    public const float navDistance = 10f;

    public bool IsNavigating => agent.stoppingDistance < agent.remainingDistance;

    public AbilityManual neutralMove = new AbilityManual() { choiceWeighting = 1f, engageRange = 4f, holdDuration = 1f, repeatable = false };

    private readonly WaitForFixedUpdate wait = new WaitForFixedUpdate();

    private Movement movement;
    private Equipment equipment;
    private Fighter fighter;
    private NavMeshAgent agent;
    
    private AbilityContainer[] currentAbilities;

    private Coroutine currentAction;
    private bool currentActionFinished = false;

    //controls fields
    private float lookUp;
    [SerializeField]
    private Fighter currentTarget;

    private bool currentTargetAlive => currentTarget != null && currentTarget.enabled;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, agent.desiredVelocity);
    }

    private void Awake()
    {
        movement = GetComponent<Movement>();
        equipment = GetComponent<Equipment>();
        fighter = GetComponent<Fighter>();
        agent = GetComponent<NavMeshAgent>();
        movement.SetMove(0, 0);

        agent.updatePosition = agent.updateRotation = false;
    }

    private void Start()
    {
        equipment.AutoEquip();
        currentAbilities = fighter.currentAbilities;
        currentAction = StartCoroutine(RandomAbilityRoutine());
    }

    private void OnDisable()
    {
        movement.SetMove(0, 0);
        if (currentAction != null) StopCoroutine(currentAction);
        currentActionFinished = true;
    }

    private void FixedUpdate()
    {
        if (currentActionFinished)
        {
            if (currentAction != null) StopCoroutine(currentAction);
            currentAction = StartCoroutine(RandomAbilityRoutine());
        }
    }

    private IEnumerator RandomAbilityRoutine()
    {
        int maxIts = 999;
        currentActionFinished = false;

        int chosenIndex = -1;
        int rotateDirection = -2;
        List<AbilityManual> usableAbilities = new List<AbilityManual>(1 + currentAbilities.Length) { neutralMove };
        List<int> abilityIndexes = new List<int>(1 + currentAbilities.Length) { -1 };

        while (currentTargetAlive)
        {
            //pick a random ability
            float totalWeighting = neutralMove.choiceWeighting;

            int index = 0;
            foreach(AbilityContainer container in currentAbilities)
            {
                if(container.ability)
                {
                    AbilityManual getManual = container.ability.abilityManual;
                    totalWeighting += getManual.choiceWeighting;
                    usableAbilities.Add(getManual);
                    abilityIndexes.Add(index);
                }
                index++;
            }

            AbilityManual chosenAbility;
            int findIndex = -2;
            do
            {
                index = 0;
                chosenAbility = neutralMove;
                float chooseAbility = Random.Range(0, totalWeighting);

                totalWeighting = 0;
                foreach (AbilityManual manual in usableAbilities)
                {
                    float nextBound = totalWeighting + manual.choiceWeighting;
                    if (totalWeighting <= chooseAbility && chooseAbility <= nextBound)
                    {
                        chosenAbility = manual;
                        findIndex = abilityIndexes[index];
                        break;
                    }
                    totalWeighting = nextBound;
                    index++;
                }
                maxIts--;
                if (maxIts < 0)
                    break;
            }
            while (chosenAbility.repeatable == false && findIndex == chosenIndex);
            chosenIndex = findIndex;

            usableAbilities.RemoveRange(1, usableAbilities.Count - 1);
            abilityIndexes.RemoveRange(1, abilityIndexes.Count - 1);

            //Ability chosenAbility = null;
            //if (Random.value < .8f)
            //{
            //    int tries = 0;
            //    do
            //    {
            //        int randomIndex = Random.Range(0, currentAbilities.Length);
            //        if (randomIndex == chosenIndex)
            //            continue;

            //        chosenIndex = randomIndex;
            //        chosenAbility = currentAbilities[chosenIndex].ability;
            //        if (chosenAbility != null)
            //            break;
            //        else
            //            tries++;
            //    }
            //    while (tries < currentAbilities.Length);
            //}
            //else
            //    chosenAbility = null;

            //float engageRange;
            //float holdDuration;

            //if (chosenAbility == null)
            //{
            //    engageRange = 4f;
            //    holdDuration = 1f;
            //    chosenIndex = -1;
            //}
            //else
            //{
            //    engageRange = chosenAbility.aiEngageRange;
            //    holdDuration = chosenAbility.aiHoldDuration;
            //}

            float abilityStart = 0;
            bool usedAbility = false;

            int randomDirection;
            do
            {
                randomDirection = Random.Range(-1, 2);
            }
            while (randomDirection == rotateDirection);
            rotateDirection = randomDirection;
            //use the random ability within the recommended range
            do
            {
                Vector3 targetPosition = currentTarget.transform.position;
                Vector3 difference = transform.position - targetPosition;

                Vector3 desiredDestination;
                if (rotateDirection == 0)
                    desiredDestination = targetPosition + difference.normalized * chosenAbility.engageRange;
                else
                    desiredDestination = targetPosition + Quaternion.Euler(0, rotateDirection * 10f, 0) * difference.normalized * chosenAbility.engageRange;

                agent.nextPosition = transform.position;
                agent.destination = desiredDestination;

                //look at target
                Vector3 desiredVelocity = movement.model.InverseTransformDirection(agent.desiredVelocity);
                movement.SetLook(ref lookUp, Quaternion.LookRotation(-difference).eulerAngles.y);
                movement.SetMove(desiredVelocity.x, desiredVelocity.z);

                if (usedAbility == false &&
                    difference.sqrMagnitude <= chosenAbility.engageRange * chosenAbility.engageRange &&
                    fighter.IsStaggered == false)
                {
                    fighter.TryStopLastAbility(out bool isProblem);
                    if (isProblem == false)
                    {
                        abilityStart = Time.time;
                        usedAbility = true;

                        if (chosenIndex != -1)
                        {
                            fighter.UseAbility(chosenIndex, true, out bool useProblem);
                            if (useProblem)
                                break;
                        }
                    }
                }
                else if (usedAbility && abilityStart + chosenAbility.holdDuration < Time.time)
                {
                    if (chosenIndex != -1)
                        fighter.UseAbility(chosenIndex, false, out _);

                    break;
                }
                yield return wait;
            }
            while (currentTargetAlive);
        }

        movement.SetMove(0, 0);
        currentActionFinished = true;
    }

    private IEnumerator MoveRoutine(Vector3 destination, bool isDirect)
    {
        agent.nextPosition = transform.position;
        agent.destination = destination;
        if(isDirect)
            movement.SetMove(0, 1);

        while (agent.pathPending) yield return wait;

        do
        {
            if (isDirect)
                movement.SetLook(ref lookUp, Quaternion.LookRotation(agent.desiredVelocity).eulerAngles.y);
            else
            {
                Vector3 desiredVelocity = agent.desiredVelocity;
                movement.SetMove(desiredVelocity.x, desiredVelocity.z);
            }
            agent.nextPosition = transform.position;
            yield return wait;
        }
        while(IsNavigating);

        movement.SetMove(0, 0);
    }
}
