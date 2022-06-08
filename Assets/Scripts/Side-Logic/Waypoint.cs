using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Side_Logic
{
	public class Waypoint : MonoBehaviour
	{
		public List<Waypoint> adyacent;
		public readonly HashSet<Item> nearbyItems = new HashSet<Item>();

		private void Start ()
		{
			//Make bidirectional
			foreach (var wp in adyacent.Where(wp => wp != null && wp.adyacent != null).Where(wp => !wp.adyacent.Contains(this)))
			{
				wp.adyacent.Add(this);
			}

			adyacent = adyacent.Where(x=>x!=null).Distinct().ToList();
		}

		//For debugging: Pause then inactivate
		private void Update () => nearbyItems.RemoveWhere(it => !it.isActiveAndEnabled);


		private void OnDrawGizmos()
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, 0.3f);
			Gizmos.color = Color.blue;
			foreach(var wp in adyacent)
			{
				Gizmos.DrawLine(transform.position, wp.transform.position);
			}
		}
	}
}
