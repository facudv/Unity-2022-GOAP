using UnityEngine;

namespace Side_Logic
{
	public enum ItemType
	{
		Invalid,
		Key,
		Door,
		Entity,
		Mace,
		PastaFrola,
		Cliff,
		HealingFont,
		MonsterProtector,
	
		//objetive items
		ObjetiveToKill,
		CabainObjetive,

		//initial items
		Ganzua,

		//world state items
		Sword, 
		Explosive,
		FogLight,
		ChestSteal,

	}

	public class Item : MonoBehaviour
	{
		public ItemType type;
		private Waypoint _wp;
		private bool _insideInventory;

		public void OnInventoryAdd()
		{
			Destroy(GetComponent<Rigidbody>());
			_insideInventory = true;
			if(_wp)
				_wp.nearbyItems.Remove(this);
		}

		public void OnInventoryRemove()
		{
			gameObject.AddComponent<Rigidbody>();
			_insideInventory = false;
		}

		private void Start ()
		{
			_wp = Navigation.instance.NearestTo(transform.position);
			_wp.nearbyItems.Add(this);
		}

		public void Kill()
		{
			var ent = GetComponent<Entity>();
			if(ent != null)
			{
				foreach(var it in ent.RemoveAllitems())
					it.transform.parent = null;
			}
			Destroy(gameObject);
		}

		public void GiveItem(Entity other)
		{
			var ent = GetComponent<Entity>();
			if (ent == null) return;
			
			foreach (var item in ent.RemoveAllitems())
			{
				var transIt = item.transform;
				transIt.parent = null;
				transIt.position = other.transform.position + Vector3.up * 5;

			}
		}

		private void OnDestroy() => _wp.nearbyItems.Remove(this);
		
		private void Update ()
		{
			if (_insideInventory) return;
			
			_wp.nearbyItems.Remove(this);
			_wp = Navigation.instance.NearestTo(transform.position);
			_wp.nearbyItems.Add(this);
		}
	}
}