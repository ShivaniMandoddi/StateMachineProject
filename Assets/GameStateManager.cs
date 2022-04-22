using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class GameStateManager : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;
    public Transform player;
    State currentState;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentState = new Idle(this.gameObject, agent, animator, player);
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Process();
    }
}
public class State
{
    public enum STATE
    {
        ATTACK, PATROL, RUN, IDLE, DEATH
    }
    public enum EVENTS
    {
        ENTER, UPDATE, EXIT
    }
    public STATE stateName;
    public EVENTS eventStage;

    public GameObject nPC;
    public NavMeshAgent agent;
    public Animator animator;
    public Transform playerPosition;
    public State nextState;

    public float visualDistance=10f,visualAngle=30f,shootingDistance=5f;
    public State(GameObject _npc,NavMeshAgent _agent,Animator _animator,Transform _playerPosition)
    {
        this.nPC = _npc;
        this.playerPosition = _playerPosition;
        this.agent = _agent;
        this.animator = _animator;
        eventStage = EVENTS.ENTER;
    }
    public virtual void Enter()
    {
        eventStage = EVENTS.UPDATE;
    }
    public virtual void Update()
    {
        eventStage = EVENTS.UPDATE;
    }
    public virtual void Exit()
    {
        eventStage = EVENTS.EXIT;
    }
    public State Process()
    {
        if (eventStage == EVENTS.ENTER)
        {
            Enter();
        }
        if (eventStage == EVENTS.UPDATE)
        {
            Update();
        }
        if (eventStage == EVENTS.EXIT)
        {
            Exit();
            return nextState;
        }
        return this;
    }
    public bool CanSeePlayer()
    {
        Vector3 direction = playerPosition.position - nPC.transform.position;
        float angle = Vector3.Angle(direction, nPC.transform.forward);
        if(direction.magnitude<visualDistance &&  angle<visualAngle)
        {
            return true;
        }
        return false;
    }
    public bool EnemyCanAttackPlayer()
    {
        Vector3 direction = playerPosition.position - nPC.transform.position;
        if(direction.magnitude<shootingDistance)
        {
            return true;
        }
        return false;
    }
}
public class Idle : State
{
    public Idle(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition): base(_npc,_agent,_animator,_playerPosition)
    {
        stateName = STATE.IDLE;
    }
    public override void Enter()
    {
        animator.SetTrigger("isIdle");
        base.Enter();
        
    }
    public override void Update()
    {
        if(CanSeePlayer())
        {
            nextState= new Chase(nPC, agent, animator, playerPosition);
            eventStage = EVENTS.EXIT;
        }
        if(Random.Range(0,100)<5)
        {
            nextState = new Patrol(nPC,agent,animator,playerPosition);
            eventStage = EVENTS.EXIT;
        }
       // base.Update();
    }
    public override void Exit()
    {
        animator.ResetTrigger("isIdle");
        base.Exit();
    }


}
public class Patrol : State
{
    int currentIndex = -1;
    public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.PATROL;
        agent.speed = 2;
        agent.isStopped = false;
    }
    public override void Enter()
    {
        currentIndex = 0;
        animator.SetTrigger("isWalking");
        base.Enter();

    }
    public override void Update()
    {
        if (CanSeePlayer())
        {
            nextState = new Chase(nPC, agent, animator, playerPosition);
            eventStage = EVENTS.EXIT;
        }
        if (agent.remainingDistance<1)
        {
            if(currentIndex>=GameController.Instance.CheckPoint.Count-1)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
               
            }
            agent.SetDestination(GameController.Instance.CheckPoint[currentIndex].transform.position);
        }
       //base.Update();
    }
    public override void Exit()
    {
        animator.ResetTrigger("isWalking");
        base.Exit();
    }

}
public class Chase: State
{
    public Chase(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.RUN;
        agent.speed = 5f;
        agent.isStopped = false;
    }
    public override void Enter()
    {
        animator.SetTrigger("isRunning");
        base.Enter();

    }
    public override void Update()
    {
        agent.SetDestination(playerPosition.position);
        if(agent.hasPath)
        {
            if(EnemyCanAttackPlayer())
            {
                nextState = new Attack(nPC, agent, animator, playerPosition);
                eventStage = EVENTS.EXIT;
            }
            else if(!CanSeePlayer())
            {
                nextState = new Patrol(nPC, agent, animator, playerPosition);
                eventStage = EVENTS.EXIT;
            }
        }
        //base.Update();
    }
    public override void Exit()
    {
        animator.ResetTrigger("isRunning");
        base.Exit();
    }

}
public class Attack: State
{
    float rotationSpeed = 5f;
    public Attack(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.ATTACK;
    }
    public override void Enter()
    {
        animator.SetTrigger("isShooting");
        agent.isStopped = true;
        base.Enter();
    }
    public override void Update()
    {
        Vector3 direction = playerPosition.position - nPC.transform.position;
        float angle = Vector3.Angle(direction, nPC.transform.forward);
        direction.y = 0;
        nPC.transform.rotation = Quaternion.Slerp(nPC.transform.rotation, Quaternion.LookRotation(direction), rotationSpeed*Time.deltaTime);
        if(!EnemyCanAttackPlayer())
        {
            nextState= new Idle(nPC, agent, animator, playerPosition);
            eventStage = EVENTS.EXIT;
        }
        if(!CanSeePlayer())
        {
            nextState = new Death(nPC, agent, animator, playerPosition);
            eventStage = EVENTS.EXIT;
        }
    }
    public override void Exit()
    {
        animator.ResetTrigger("isShooting");
        
        base.Exit();
    }

}
public class Death: State
{
    public Death(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _playerPosition) : base(_npc, _agent, _animator, _playerPosition)
    {
        stateName = STATE.DEATH;
    }
    public override void Enter()
    {
        animator.SetTrigger("isSleeping");
        //agent.isStopped = true;
        base.Enter();
    }
    public override void Update()
    {
        // Future Update
    }
    public override void Exit()
    {
        animator.ResetTrigger("isSleeping");
       
        base.Exit();
    }

}
// Adding Audio Source 
// bullet used object poling method
//

