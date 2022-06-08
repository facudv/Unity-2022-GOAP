using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Side_Logic;
using UnityEngine;

namespace GOAP
{
    public class Planner : MonoBehaviour
    {
        private readonly List<Tuple<Vector3, Vector3>> _debugRayList = new List<Tuple<Vector3, Vector3>>();
        [SerializeField] private GameManager gameManager;

        #region WORLD STATES VARIABLES
    
        private GameObject _fog;
        [SerializeField] private GameObject[] swordOrExplosive;
        [SerializeField] private Transform spawnTransWeapon;
    
        #endregion
    
        private void Start()
        {
            _fog = GameObject.Find("fog");
            SetInitialsWorldStates();
            StartCoroutine(Plan());
        }
    
        private void SetInitialsWorldStates()
        {
            GameObject.Instantiate(gameManager.GetPlayerWeaponInWorld() == WeaponType.Sword ? swordOrExplosive[0] : swordOrExplosive[1]
                , spawnTransWeapon.position
                , Quaternion.identity);
        
            _fog.SetActive(gameManager.GameManagerInstance.GetFog());
        }

        private void Check(Dictionary<string, bool> state, ItemType type)
        {

            var items = Navigation.instance.AllItems();
            var inventories = Navigation.instance.AllInventories();
            var floorItems = items.Except(inventories);
            var item = floorItems.FirstOrDefault(x => x.type == type);
            var here = transform.position;
            state["accessible" + type.ToString()] = item != null && Navigation.instance.Reachable(here, item.transform.position, _debugRayList);

            var inv = inventories.Any(x => x.type == type);
            state["otherHas" + type.ToString()] = inv;
            
            state["dead" + type.ToString()] = false;
        }

        private IEnumerator Plan()
        {
            yield return new WaitForSeconds(0.2f);
            var observedState = new Dictionary<string, bool>();

            var nav = Navigation.instance;//To get items
            // var floorItems = nav.AllItems();
            // var inventory = nav.AllInventories();
            var everything = nav.AllItems().Union(nav.AllInventories());// .Union() two collections excluding duplicates

            //Check bool to all items. Generates model world in observedState
            Check(observedState, ItemType.Key);
            Check(observedState, ItemType.Door);
            Check(observedState, ItemType.Entity);
            Check(observedState, ItemType.CabainObjetive);
            Check(observedState, ItemType.Cliff);
            Check(observedState, ItemType.FogLight);
            Check(observedState, ItemType.HealingFont);
            Check(observedState, ItemType.ChestSteal);
            Check(observedState, ItemType.MonsterProtector);
            Check(observedState, ItemType.ObjetiveToKill);
            Check(observedState, ItemType.Ganzua);
            Check(observedState, ItemType.Sword);
            Check(observedState, ItemType.Explosive);

            var actions = CreatePossibleActionsList();
        
            var initial = new GoapState
            {
                worldState = new WorldState()
                {
                    playerHp = gameManager.GameManagerInstance.GetPlayerLife(),
                    enumWeaponType = gameManager.GameManagerInstance.GetPlayerWeaponInWorld(),
                    money = gameManager.GameManagerInstance.GetPlayerMoney(),
                    hasFog = gameManager.GameManagerInstance.GetFog(),
                    values = new Dictionary<string, bool>()
                }
            };

            initial.worldState.values = observedState;

            //EXTRA, used for open chest steal
            initial.worldState.values["has" + ItemType.Ganzua.ToString()] = true;
            
            foreach (var item in initial.worldState.values)
            {
                Debug.Log(item.Key + " ---> " + item.Value);
            }

            //GOAL CONDITION
            GoapState goal = new GoapState();
            goal.worldState.values["dead" + ItemType.ObjetiveToKill.ToString()] = true;

            Func<GoapState, float> heuristc = (curr) =>
            {
                int count = 0;            

                string key = "dead" + ItemType.ObjetiveToKill.ToString();
                if (!curr.worldState.values.ContainsKey(key) || !curr.worldState.values[key])
                    count++;

                return count;
            };


            Func<GoapState, bool> objective = (curr) =>
            {
                string key = "dead" + ItemType.ObjetiveToKill.ToString();
                return curr.worldState.values.ContainsKey(key) && curr.worldState.values["dead" + ItemType.ObjetiveToKill.ToString()];
            };


            var actDict = new Dictionary<string, ActionEntity>() 
            {
                { "Kill"      , ActionEntity.Kill }
                , { "Pickup"    , ActionEntity.PickUp }
                , { "Open"      , ActionEntity.Open }
                , { "JumpCliff" , ActionEntity.JumpCliff }
                , { "Heal"      , ActionEntity.Heal }
                , { "Steal"     , ActionEntity.Steal }
                , { "Buy"       , ActionEntity.Buy }
            };


            var plan = Goap.Execute(initial, null, objective, heuristc, actions);

            if (plan == null)
                Debug.Log("Couldn't plan");
            else
            {
                GetComponent<Guy>().ExecutePlan(
                    plan
                        .Select(a =>
                        {
                            Item i2 = everything.FirstOrDefault(i => i.type == a.item);
                            if (actDict.ContainsKey(a.Name) && i2 != null)
                            {
                                return Tuple.Create(actDict[a.Name], i2);
                            }
                            else
                            {
                                return null;
                            }
                        }).Where(a => a != null)
                        .ToList(), 
                    gameManager
                );
            }
        }
    
        private List<GoapAction> CreatePossibleActionsList()
        {
            return new List<GoapAction>()
            {

                new GoapAction("JumpCliff")
                    .SetCost(1f)
                    .SetItem(ItemType.Cliff)
                    .Pre((gS) =>
                        gS.worldState.values.ContainsKey("accessible"+ ItemType.Cliff.ToString()) &&
                            gS.worldState.values["accessible"+ ItemType.Cliff.ToString()] &&
                            gS.worldState.playerHp > GoapListActionValues.iDamageAfterJump)
                    
                        .Effect((gS) =>
                        {
                            gS.worldState.values["accessible"+ ItemType.Cliff.ToString()] = false;
                            gS.worldState.values["accessible"+ ItemType.HealingFont.ToString()] = true;
                            gS.worldState.values["accessible"+ ItemType.Explosive.ToString()] = true;
                            gS.worldState.values["accessible"+ ItemType.Sword.ToString()] = true;
                            gS.worldState.playerHp = -GoapListActionValues.iDamageAfterJump;
                            return gS;
                        }
                    )


                ,new GoapAction("Heal")
                    .SetCost(2f)
                    .SetItem(ItemType.HealingFont)
                    .Pre((gS) => gS.worldState.values.ContainsKey("accessible"+ ItemType.HealingFont.ToString()) &&
                                 gS.worldState.values["accessible"+ ItemType.HealingFont.ToString()] &&
                                 gS.worldState.playerHp <= GoapListActionValues.iHealAmount / 2)
                    .Effect((gS) =>
                        {
                            gS.worldState.playerHp = GoapListActionValues.iHealAmount;
                            gS.worldState.values["accessible"+ ItemType.ChestSteal.ToString()] = true;
                            gS.worldState.values["has"+ ItemType.Ganzua.ToString()] = true;
                            return gS;
                        }
                    )


                ,new GoapAction("Steal")
                    .SetCost(2f)
                    .SetItem(ItemType.ChestSteal)
                    .Pre((gS) => gS.worldState.values.ContainsKey("accessible"+ ItemType.ChestSteal.ToString()) &&
                                 gS.worldState.values.ContainsKey("has"+ ItemType.Ganzua.ToString()) &&
                                 gS.worldState.values["accessible"+ ItemType.ChestSteal.ToString()] &&
                                 gS.worldState.money < GoapListActionValues.fMoneyCanStealChest)
                  
                    .Effect((gS) =>
                        {
                            gS.worldState.values["accessible"+ ItemType.ChestSteal.ToString()] = false;
                            gS.worldState.values["accessible"+ ItemType.Entity.ToString()] = true;
                            gS.worldState.values["dead"+ ItemType.Ganzua.ToString()] = true;
                            gS.worldState.money  += GoapListActionValues.fMoneyGainStealChest;
                            return gS;
                        }
                    )

                ,new GoapAction("Buy")
                    .SetCost(3f)
                    .SetItem(ItemType.Entity)
                    .Pre((gS) => gS.worldState.values.ContainsKey("accessible"+ ItemType.Entity.ToString()) &&
                                 gS.worldState.values.ContainsKey("otherHas"+ ItemType.FogLight.ToString()) &&
                                 gS.worldState.values["accessible"+ ItemType.Entity.ToString()] &&
                                 gS.worldState.values["otherHas"+ ItemType.FogLight.ToString()] &&
                                 gS.worldState.money >= GoapListActionValues.fMoneyCanStealChest &&
                                 gS.worldState.hasFog)
                  
                    .Effect((gS) =>
                        {
                            gS.worldState.values["accessible"+ ItemType.Entity.ToString()] = false;
                            gS.worldState.values["accessible"+ ItemType.FogLight.ToString()] = true;
                            gS.worldState.values["otherHas"+ ItemType.FogLight.ToString()] = false;
                            gS.worldState.money  -= GoapListActionValues.fMoneyGainStealChest;
                            return gS;
                          
                        }
                    )

                ,new GoapAction("Pickup")
                    .SetCost(1f)
                    .SetItem(ItemType.FogLight)
                    .Pre((gS) => gS.worldState.values.ContainsKey("otherHas" + ItemType.FogLight.ToString()) &&
                                 gS.worldState.values.ContainsKey("accessible" + ItemType.FogLight.ToString()) &&
                                 !gS.worldState.values["otherHas" + ItemType.FogLight.ToString()] &&
                                 gS.worldState.values["accessible" + ItemType.FogLight.ToString()] &&

                                 gS.worldState.hasFog)
                    .Effect((gS) =>
                        {
                            gS.worldState.hasFog = false;
                            gS.worldState.values["accessible"+ ItemType.FogLight.ToString()] = false;
                            gS.worldState.values["has"+ ItemType.FogLight.ToString()] = true;
                            return gS;
                        }
                    )

                ,new GoapAction("Pickup")
                    .SetCost(3f)
                    .SetItem(ItemType.Explosive)
                    .Pre((gS) => 
                        gS.worldState.values.ContainsKey("accessible"+ ItemType.Explosive.ToString()) &&
                        gS.worldState.values["accessible"+ ItemType.Explosive.ToString()] &&
                        gS.worldState.hasFog == false &&
                        gS.worldState.enumWeaponType == WeaponType.Explosive)
                    .Effect((gS) =>
                        {
                            gS.worldState.values["accessible"+ ItemType.Explosive.ToString()] = false;
                            gS.worldState.values["has"+ ItemType.Explosive.ToString()] = true;
                            return gS;
                        }
                    )
                  
                ,new GoapAction("Pickup")
                    .SetCost(3f)
                    .SetItem(ItemType.Sword)
                    .Pre((gS) => 
                        gS.worldState.values.ContainsKey("accessible"+ ItemType.Sword.ToString()) &&
                        gS.worldState.values["accessible"+ ItemType.Sword.ToString()] &&
                        gS.worldState.hasFog == false &&
                        gS.worldState.enumWeaponType == WeaponType.Sword)
                    .Effect((gS) =>
                        {
                            gS.worldState.values["accessible"+ ItemType.Sword.ToString()] = false;
                            gS.worldState.values["has"+ ItemType.Sword.ToString()] = true;
                            return gS;
                        }
                    )
                
                ,new GoapAction("Kill")
                    .SetCost(5f)
                    .SetItem(ItemType.MonsterProtector)
                    .Pre((gS) => 
                        gS.worldState.values.ContainsKey("dead"+ ItemType.MonsterProtector.ToString()) &&
                        gS.worldState.values.ContainsKey("has"+ ItemType.Sword.ToString()) &&
                        !gS.worldState.values["dead"+ ItemType.MonsterProtector.ToString()] &&
                        gS.worldState.values["has"+ ItemType.Sword.ToString()] &&

                        gS.worldState.hasFog == false &&
                        gS.worldState.enumWeaponType == WeaponType.Sword &&
                        gS.worldState.playerHp > 50)
                    .Effect((gS) =>
                        {
                            gS.worldState.values["accessible" + ItemType.Key.ToString()] = true;
                            gS.worldState.values["dead"+ ItemType.MonsterProtector.ToString()] = true;
                            return gS;
                        }
                    )
                  
                  
                ,new GoapAction("Kill")
                    .SetCost(7f)
                    .SetItem(ItemType.CabainObjetive)
                    .Pre((gS) => 
                        gS.worldState.values.ContainsKey("has"+ ItemType.Explosive.ToString()) &&
                        gS.worldState.values["has"+ ItemType.Explosive.ToString()] &&

                        gS.worldState.hasFog == false &&
                        gS.worldState.enumWeaponType == WeaponType.Explosive)
                    .Effect((gS) =>
                        {
                            gS.worldState.values["has"+ ItemType.Explosive.ToString()] = false;
                            gS.worldState.values["dead"+ ItemType.ObjetiveToKill.ToString()] = true;
                            return gS;
                        }
                    )


                ,new GoapAction("Pickup")
                    .SetCost(1f)
                    .SetItem(ItemType.Key)
                    .Pre((gS) => 
                        gS.worldState.values.ContainsKey("dead"+ ItemType.MonsterProtector.ToString()) &&
                        gS.worldState.values.ContainsKey("accessible"+ ItemType.Key.ToString()) &&
                        gS.worldState.values["dead"+ ItemType.MonsterProtector.ToString()] &&
                        gS.worldState.values["accessible"+ ItemType.Key.ToString()])
                    .Effect((gS) =>
                        {
                            gS.worldState.values["otherHas"+ ItemType.Key.ToString()] = false;
                            gS.worldState.values["has"+ ItemType.Key.ToString()] = true;
                            return gS;
                        }
                    )


                ,new GoapAction("Open")
                    .SetCost(2f)
                    .SetItem(ItemType.Door)
                    .Pre((gS) => 
                        gS.worldState.values.ContainsKey("dead"+ ItemType.MonsterProtector.ToString()) &&
                        gS.worldState.values.ContainsKey("has"+ ItemType.Key.ToString()) &&
                        gS.worldState.values["dead"+ ItemType.MonsterProtector.ToString()] &&
                        gS.worldState.values["has"+ ItemType.Key.ToString()])
                    .Effect((gS) =>
                        {
                            gS.worldState.values["has"+ ItemType.Key.ToString()] = false;
                            gS.worldState.values["dead"+ ItemType.Key.ToString()] = true;
                            gS.worldState.values["accessible"+ ItemType.ObjetiveToKill.ToString()] = true;
                            return gS;
                        }
                    )


                ,new GoapAction("Kill")
                    .SetCost(6f)
                    .SetItem(ItemType.ObjetiveToKill)
                    .Pre((gS) => 
                        gS.worldState.values.ContainsKey("accessible"+ItemType.ObjetiveToKill.ToString()) &&
                        gS.worldState.values.ContainsKey("has"+ ItemType.Sword.ToString()) &&
                        gS.worldState.values["accessible"+ ItemType.ObjetiveToKill.ToString()] &&
                        gS.worldState.values["has"+ ItemType.Sword.ToString()])
                    .Effect((gS) =>
                        {
                            //gS.worldState.values["has"+ ItemType.Key.ToString()] = false;
                            gS.worldState.values["dead"+ ItemType.ObjetiveToKill.ToString()] = true;
                            gS.worldState.values["accessible"+ ItemType.ObjetiveToKill.ToString()] = false;
                            return gS;
                        }
                    )
            };
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            foreach (var t in _debugRayList)
            {
                Gizmos.DrawRay(t.Item1, (t.Item2 - t.Item1).normalized);
                Gizmos.DrawCube(t.Item2 + Vector3.up, Vector3.one * 0.2f);
            }
        }
    }
}
