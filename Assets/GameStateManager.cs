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

    float visualDistance,visualAngle,shootingDistance;
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
        if(agent.remainingDistance<1)
        {
            if(currentIndex>=GameController.Instance.CheckPoint.Count)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
               
            }
            agent.SetDestination(GameController.Instance.CheckPoint[currentIndex].transform.position);
        }
       // base.Update();
    }
    public override void Exit()
    {
        animator.ResetTrigger("isIdle");
        base.Exit();
    }

}



