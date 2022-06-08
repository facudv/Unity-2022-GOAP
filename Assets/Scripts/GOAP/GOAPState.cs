using System.Collections.Generic;
using System.Linq;
using Managers;
using Side_Logic;

namespace GOAP
{
    public class GoapState
    {
        public WorldState worldState;


        public GoapAction generatingAction;
        public int step = 0;

        #region CONSTRUCTOR
        public GoapState(GoapAction gen = null)
        {
            generatingAction = gen;
            worldState = new WorldState()
            {
                values = new Dictionary<string, bool>()//must initialize
            };
        }

        public GoapState(GoapState source, GoapAction gen = null)
        {
            worldState = source.worldState.Clone();
            generatingAction = gen;
        }
        #endregion


        public override bool Equals(object obj)
        {
            var result =
                obj is GoapState other
                && other.generatingAction == generatingAction  
                && other.worldState.values.Count == worldState.values.Count
                && other.worldState.values.All(kv => kv.In(worldState.values));
            return result;
        }

        public override int GetHashCode() => worldState.values.Count == 0 ? 0 : 31 * worldState.values.Count + 31 * 31 * worldState.values.First().GetHashCode();

        public override string ToString()
        {
            var str = "";
            foreach (var kv in worldState.values.OrderBy(x => x.Key))
            {
                str += (string.Format("{0:12} : {1}\n", kv.Key, kv.Value));
            }
            return ("--->" + (generatingAction != null ? generatingAction.Name : "NULL") + "\n" + str);
        }
    }
    
    public struct WorldState
    {
        public int playerHp;
        public WeaponType enumWeaponType;
        public float money;
        public bool hasFog;

        public Dictionary<string, bool> values;

    
        //Create a clone to don't have old references
        public WorldState Clone()
        {
            return new WorldState()
            {
                playerHp = this.playerHp,
                enumWeaponType = this.enumWeaponType,
                money = this.money,
                hasFog = this.hasFog,

                values = this.values.ToDictionary(kv => kv.Key, kv => kv.Value)
            };
        }
    }
}