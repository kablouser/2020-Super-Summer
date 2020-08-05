﻿using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

using static Fighter;

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
    /// <summary>
    /// amount of extra time to calculate target's position
    /// </summary>
    public const float predictIntoFuture = 0.15f;
    /// <summary>
    /// multiplier that changes the desired range
    /// </summary>
    public const float rangeVariation = 0.15f;

    public bool IsNavigating => agent.stoppingDistance < agent.remainingDistance;

    public EnemyComponents enemyComponents;

    public AbilityManual neutralMove = new AbilityManual() { choiceWeighting = 1f, engageRange = 4f, holdDuration = 1f, repeatable = false };    
        
    private Movement movement;
    private Equipment equipment;
    private Fighter fighter;
    private NavMeshAgent agent;

    private Coroutine currentAction;
    private bool currentActionFinished = false;

    //controls fields
    private float lookUp;
    [SerializeField]
    private Fighter currentTarget = null;

    private bool currentTargetAlive => currentTarget != null && currentTarget.enabled;

    private void Awake()
    {
        movement = enemyComponents.movement;
        equipment = enemyComponents.equipment;
        fighter = enemyComponents.fighter;
        agent = enemyComponents.agent;
        movement.SetMove(0, 0);

        agent.updatePosition = agent.updateRotation = false;
    }

    private void Start()
    {
        equipment.AutoEquip();
        currentAction = StartCoroutine(RandomAbilityRoutine());
        //currentActionFinished = false;
        //movement.SetMove(0, -0.3f);
        //fighter.UseAbility(AbilityIndex.L2, true, out _);
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
        List<AbilityManual> usableAbilities = new List<AbilityManual>(1 + fighter.currentAbilities.Length) { neutralMove };
        List<int> abilityIndexes = new List<int>(1 + fighter.currentAbilities.Length) { -1 };

        while (currentTargetAlive)
        {
            //pick a random ability
            float totalWeighting = neutralMove.choiceWeighting;

            int index = 0;
            foreach(Ability ability in fighter.currentAbilities)
            {
                if(ability != null)
                {
                    AbilityManual getManual = ability.abilityManual;
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

            float abilityStart = 0;
            bool usedAbility = false;

            int randomDirection;
            do
            {
                randomDirection = Random.Range(-1, 2);
            }
            while (randomDirection == rotateDirection);
            rotateDirection = randomDirection;

            float rangeVariance = 1 + Random.Range(-rangeVariation, rangeVariation);

            //use the random ability within the recommended range
            do
            {
                Vector3 targetPosition = currentTarget.transform.position +
                    currentTarget.characterComponents.rigidbodyComponent.velocity * predictIntoFuture;

                Vector3 difference = transform.position - targetPosition;

                Vector3 desiredDestination;
                if (rotateDirection == 0)
                    desiredDestination = targetPosition + difference.normalized * chosenAbility.engageRange * rangeVariance;
                else
                    desiredDestination = targetPosition +
                        Quaternion.Euler(0, rotateDirection * 10f, 0) * difference.normalized * chosenAbility.engageRange * rangeVariance;

                agent.nextPosition = transform.position;
                agent.destination = desiredDestination;

                //look at target
                Vector3 desiredVelocity = movement.bodyRotator.InverseTransformDirection(agent.desiredVelocity);
                movement.SetLook(ref lookUp, Quaternion.LookRotation(-difference).eulerAngles.y);
                movement.SetMove(desiredVelocity.x, desiredVelocity.z);

                if (usedAbility == false &&
                    difference.sqrMagnitude <=
                    chosenAbility.engageRange * chosenAbility.engageRange *
                    rangeVariance * rangeVariance &&
                    fighter.IsStaggered == false)
                {
                    fighter.TryStopLastAbility(out bool isProblem);
                    if (isProblem == false)
                    {
                        abilityStart = Time.time;
                        usedAbility = true;

                        if (chosenIndex != -1)
                        {
                            bool useProblem;
                            if (Random.value < 0.5f)
                                fighter.UseAbility(chosenIndex, true, out useProblem);
                            else
                                fighter.UseAbilityDontHold(chosenIndex, true, out useProblem);

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
                yield return CoroutineConstants.waitFixed;
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

        while (agent.pathPending) yield return CoroutineConstants.waitFixed;

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
            yield return CoroutineConstants.waitFixed;
        }
        while(IsNavigating);

        movement.SetMove(0, 0);
    }
}
