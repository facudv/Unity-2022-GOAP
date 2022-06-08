using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IA2;
using Managers;
using Misc;
using Side_Logic;
using Side_Logic.FSM;
using Side_Logic.GoapActionsFeedBuck;
using UnityEngine;

namespace GOAP
{
    public enum ActionEntity
    {
        Kill,
        PickUp,
        NextStep,
        FailedStep,
        Open,
        Success,
        JumpCliff,
        Heal,
        Steal,
        Buy
    }

    [RequireComponent(typeof(Entity))]
    public class Guy : MonoBehaviour
    {
        private EventFSM<ActionEntity> _fsm;
        private Item _target;
        private Entity _ent;
        private GameManager _gameManager;
    
        private IEnumerable<Tuple<ActionEntity, Item>> _plan;

        [Header("Jump")] [SerializeField] private float fResetMoveAfterJumpingCliff = 3.5f;
        private float _fResetMoveAfterJumpingCliff;
        
        private GameObject _fog;
    
        private void PerformHeal(Entity us, Item other)
        {
            if (other != _target) return;
            var healingFont = other.GetComponent<HealingFont>();
            if (healingFont)
            {
                _gameManager.UpdateLife(GoapListActionValues.iHealAmount);
                healingFont.FeedBuckHealing();
                _ent.Healing();
                _fsm.Feed(ActionEntity.NextStep);
            }
            else
                _fsm.Feed(ActionEntity.FailedStep);
        }

        private void PerformJumpCliff(Entity us, Item other)
        {
            if (other != _target) return;
            _ent.Jump();
            _gameManager.UpdateLife(-GoapListActionValues.iDamageAfterJump);
            StartCoroutine(StunnedAfterJumpCliff());
            _fsm.Feed(ActionEntity.NextStep);
        }

        private IEnumerator StunnedAfterJumpCliff()
        {
            var yieldInstruction =  new WaitForEndOfFrame();
            while (_fResetMoveAfterJumpingCliff > 0)
            {
                _ent.SetMove(false);
                _fResetMoveAfterJumpingCliff -= Time.deltaTime;
                yield return yieldInstruction;
            }
            _ent.SetMove(true);
        }
    
        private void PerformSteal(Entity us, Item other)
        {
            if (other != _target) return;
            var ganzua = _ent.items.FirstOrDefault(it => it.type == ItemType.Ganzua);
            var chestSteal = other.GetComponent<ChestSteal>();

            if (ganzua)
            {
                _gameManager.GameManagerInstance.UpdateMoney(GoapListActionValues.fMoneyGainStealChest);
                chestSteal.FeedBuckHouseSteal();
                Destroy(_ent.Removeitem(ganzua).gameObject);
                _fsm.Feed(ActionEntity.NextStep);
            }
            else
                _fsm.Feed(ActionEntity.FailedStep);
        }

        private void PerformBuy(Entity us, Item other)
        {
            if (other != _target) return;
            other.GiveItem(_ent);
            _fsm.Feed(ActionEntity.NextStep);
        }

        private void PerformAttack(Entity us, Item other)
        {
            Debug.Log("PerformAttack", other.gameObject);
            if (other != _target) return;
            var sword = _ent.items.FirstOrDefault(it => it.type == ItemType.Sword); //la espada no se rompe al atacar
            var explosive = _ent.items.FirstOrDefault(it => it.type == ItemType.Explosive);

            if (sword || explosive)
            {
                other.Kill();
                if (other.type == ItemType.CabainObjetive && explosive)
                {
                    var explosiveParticle = explosive.GetComponentInChildren<ExplosiveParticle>();
                    if (explosiveParticle)
                    {
                        explosiveParticle.gameObject.transform.parent = null;
                        Destroy(_ent.Removeitem(explosive).gameObject);
                        explosiveParticle.explosionParticle.Play();
                    }
                }
                if(sword)sword.GetComponentInChildren<ParticleSystem>().Play();
                _fsm.Feed(ActionEntity.NextStep);
            }
            else
                _fsm.Feed(ActionEntity.FailedStep);
        }

        private void PerformOpen(Entity us, Item other)
        {
            if (other != _target) return;

            var key = _ent.items.FirstOrDefault(it => it.type == ItemType.Key);
            var door = other.GetComponent<Door>();
            if (door && key)
            {
                door.Open();
                Destroy(_ent.Removeitem(key).gameObject);
                _fsm.Feed(ActionEntity.NextStep);
            }
            else
                _fsm.Feed(ActionEntity.FailedStep);
        }

        private void PerformPickUp(Entity us, Item other)
        {
            if (other != _target) return;
        
            _ent.AddItem(other);
            _fsm.Feed(ActionEntity.NextStep);
        }
        

        private void Awake()
        {
            _fog = GameObject.Find("fog");
            _ent = GetComponent<Entity>();
            _fResetMoveAfterJumpingCliff = fResetMoveAfterJumpingCliff;
        
            var any = new State<ActionEntity>("any");

        
            var idle = new State<ActionEntity>("idle");
            var bridgeStep = new State<ActionEntity>("planStep");
            var failStep = new State<ActionEntity>("failStep");
            var kill = new State<ActionEntity>("kill");
            var pickup = new State<ActionEntity>("pickup");
            var open = new State<ActionEntity>("open");
            var success = new State<ActionEntity>("success");
            var jumpCliff = new State<ActionEntity>("jumpCliff");
            var heal = new State<ActionEntity>("heal");
            var steal = new State<ActionEntity>("steal");
            var buy = new State<ActionEntity>("buy");




            kill.OnEnter += a =>
            {
                _ent.GoTo(_target.transform.position);
                _ent.OnHitItem += PerformAttack;
            };

            kill.OnExit += a => _ent.OnHitItem -= PerformAttack;

            failStep.OnEnter += a => { _ent.Stop(); Debug.Log("Plan failed"); };

            pickup.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformPickUp; };
            pickup.OnExit += a => _ent.OnHitItem -= PerformPickUp;

            open.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformOpen; };
            open.OnExit += a => _ent.OnHitItem -= PerformOpen;

            steal.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformSteal; };
            steal.OnExit += a => _ent.OnHitItem -= PerformSteal;

            buy.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformBuy; };
            buy.OnExit += a => _ent.OnHitItem -= PerformBuy;

            jumpCliff.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformJumpCliff; };
            jumpCliff.OnExit += a => _ent.OnHitItem -= PerformJumpCliff;

            heal.OnEnter += a => { _ent.GoTo(_target.transform.position); _ent.OnHitItem += PerformHeal; };
            heal.OnExit += a => _ent.OnHitItem -= PerformHeal;


            bridgeStep.OnEnter += a =>
            {
                var step = _plan.FirstOrDefault();
                if (step != null)
                {
                    _plan = _plan.Skip(1);
                    var oldTarget = _target;
                    _target = step.Item2;
                    if (!_fsm.Feed(step.Item1))
                        _target = oldTarget;
                    _gameManager.GameManagerInstance.UpdateActualState(_fsm.Current);
                }
                else
                {
                    _fsm.Feed(ActionEntity.Success);
                    _gameManager.GameManagerInstance.Success();
                }
            };

            success.OnEnter += a => { Debug.Log("Success"); };
            success.OnUpdate += () => { _ent.SuccesFeedBuck(); };

            StateConfigurer.Create(any)
                .SetTransition(ActionEntity.NextStep, bridgeStep)
                .SetTransition(ActionEntity.FailedStep, idle)
                .Done();

            StateConfigurer.Create(bridgeStep)
                .SetTransition(ActionEntity.Kill, kill)
                .SetTransition(ActionEntity.PickUp, pickup)
                .SetTransition(ActionEntity.Open, open)
                .SetTransition(ActionEntity.Success, success)
                .SetTransition(ActionEntity.JumpCliff, jumpCliff)
                .SetTransition(ActionEntity.Heal, heal)
                .SetTransition(ActionEntity.Steal, steal)
                .SetTransition(ActionEntity.Buy, buy)

                .Done();

            _fsm = new EventFSM<ActionEntity>(idle, any);
        }

        public void ExecutePlan(List<Tuple<ActionEntity, Item>> plan,GameManager gameManger)
        {
            _gameManager = gameManger;
            _plan = plan;
            _fsm.Feed(ActionEntity.NextStep);
        }

        private void Update() => _fsm.Update();

        private void OnCollisionEnter(Collision collision)
        {
            var item = collision.gameObject.GetComponent<Item>();
            if (!item) return;
            StartCoroutine(WaitForNextStep());
            if(item.type == ItemType.FogLight)
                _fog.SetActive(false);
        } 

        private IEnumerator WaitForNextStep()
        {
            _ent.SetMove(false);
            yield return new WaitForSeconds(1f);
            _ent.SetMove(true);
        }
    }
}