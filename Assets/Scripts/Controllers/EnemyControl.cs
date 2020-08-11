using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyControl : MonoBehaviour, CharacterSheet.IAttackListener
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

    public enum AITeam { defaultTeam }

    public bool IsNavigating => agent.stoppingDistance < agent.remainingDistance;

    public EnemyComponents enemyComponents;
    public AITeam team;
    public AbilityManual neutralMove = new AbilityManual() { choiceWeighting = 1f, engageRange = 4f, holdDuration = 1f, repeatable = false };
    public ResourceBars resourceBars;

    private Movement movement;
    private Equipment equipment;
    private Fighter fighter;
    private NavMeshAgent agent;

    private Coroutine currentAction;
    private bool currentActionFinished = false;

    //controls fields
    [SerializeField]
    private Fighter currentTarget = null;

    private bool currentTargetAlive => currentTarget != null && currentTarget.enabled;

    /// <summary>
    /// This function allows the enemy to aggro
    /// </summary>
    /// 

    public void OnAttacked(int damage, Vector3 contactPoint, CharacterComponents character, out CharacterSheet.DefenceFeedback feedback)
    {
        feedback = CharacterSheet.DefenceFeedback.NoDefence;
        SenseNewThreat(character);        
    }

    public void SenseNewThreat(CharacterComponents character)
    {
        if (character == null) return;

        if (character is EnemyComponents enemyAI)
        {
            if (team == enemyAI.enemyControl.team)
                //same team, friendly fire, dont retaliate...
                //or maybe do retaliate depending on the team type and ai
                return;
        }

        if (currentTargetAlive)
        {
            //is the attacker closer?
            if ((transform.position - character.transform.position).sqrMagnitude <
                (transform.position - currentTarget.transform.position).sqrMagnitude)
                currentTarget = character.fighter;
        }
        else
        {
            currentTarget = character.fighter;
        }
    }

    private void Awake()
    {
        movement = enemyComponents.movement;
        equipment = enemyComponents.equipment;
        fighter = enemyComponents.fighter;
        agent = enemyComponents.agent;
        movement.SetMove(0, 0);

        agent.updatePosition = agent.updateRotation = false;

        resourceBars.Setup(enemyComponents.characterSheet);

        movement.OnCollisionEvent += MovementCollision;
    }

    private void Start()
    {
        equipment.AutoEquip();
        currentAction = StartCoroutine(RandomAbilityRoutine());
    }

    private void OnEnable()
    {
        enemyComponents.characterSheet.AddAttackListener(this);
    }

    private void OnDisable()
    {
        movement.SetMove(0, 0);
        if (currentAction != null) StopCoroutine(currentAction);
        currentActionFinished = true;

        enemyComponents.characterSheet.RemoveAttackListener(this);
        resourceBars.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        movement.OnCollisionEvent -= MovementCollision;
    }

    private void FixedUpdate()
    {
        if (currentActionFinished && currentTargetAlive)
        {
            if (currentAction != null) StopCoroutine(currentAction);
            currentAction = StartCoroutine(RandomAbilityRoutine());
        }

        if(currentTargetAlive)
        {
            resourceBars.gameObject.SetActive(true);
            resourceBars.UpdateVisuals();
        }
        else
        {
            resourceBars.gameObject.SetActive(false);
        }
    }

    private void MovementCollision(Collision collision)
    {
        var rigidbody = collision.rigidbody;
        if (rigidbody == null) return;

        CharacterComponents character = rigidbody.GetComponent<CharacterComponents>();
        if (character == null) return;

        SenseNewThreat(character);
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
            foreach (Ability ability in fighter.currentAbilities)
            {
                if (ability != null)
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
                desiredDestination.y = targetPosition.y;

                agent.nextPosition = transform.position;
                if (NavMesh.SamplePosition(desiredDestination, out NavMeshHit hit, 2 * agent.height, NavMesh.AllAreas))
                    agent.destination = hit.position;
                else
                    agent.destination = desiredDestination;

                //look at target
                Vector3 desiredVelocity = movement.bodyRotator.InverseTransformDirection(agent.desiredVelocity);
                movement.SetMove(desiredVelocity.x, desiredVelocity.z);
                LookInDirection(-difference);

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
        if (isDirect)
            movement.SetMove(0, 1);

        while (agent.pathPending) yield return CoroutineConstants.waitFixed;

        do
        {
            if (isDirect)
                LookInDirection(agent.desiredVelocity);
            else
            {
                Vector3 desiredVelocity = agent.desiredVelocity;
                movement.SetMove(desiredVelocity.x, desiredVelocity.z);
            }
            agent.nextPosition = transform.position;
            yield return CoroutineConstants.waitFixed;
        }
        while (IsNavigating);

        movement.SetMove(0, 0);
    }

    private void LookInDirection(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Vector3 euler = Quaternion.LookRotation(direction).eulerAngles;        
        if(180 < euler.x)
            euler.x -= 360;
        movement.SetLook(ref euler.x, euler.y);
    }
}
