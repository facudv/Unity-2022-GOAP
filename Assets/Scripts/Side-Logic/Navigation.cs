using System;
using System.Collections.Generic;
using System.Linq;
using Graph;
using UnityEngine;

namespace Side_Logic
{
	public class Navigation : MonoBehaviour
	{
		public static Navigation instance;
		private readonly List<Waypoint> _waypoints = new List<Waypoint>();

		private void Start ()
		{
			instance = this;

			foreach(Transform trans in transform)
			{
				var wp = trans.GetComponent<Waypoint>();
				if(wp != null)
					_waypoints.Add(wp);
			}
		}

		public bool Reachable(Vector3 from, Vector3 to, List<Tuple<Vector3, Vector3>> debugRayList)
		{
			var srcWp = NearestTo(from);
			var dstWp = NearestTo(to);

			var wp = srcWp;

			if(srcWp != dstWp) 
			{
				var path = AStarNormal<Waypoint>.Run(
					srcWp
					, dstWp
					, (wa, wb) => Vector3.Distance(wa.transform.position, wb.transform.position)
					, w => w == dstWp
					, w =>
						w.adyacent
							.Where(a => a.nearbyItems.All(it => it.type == ItemType.Cliff))
							//.Where(a => a.nearbyItems.All(it => it.type != ItemType.Door)) //OLD EXAMPLE
							.Select(a => new AStarNormal<Waypoint>.Arc(a, Vector3.Distance(a.transform.position, w.transform.position)))
				);
				if(path == null)
					return false;

				wp = path.Last();
			}
			Debug.Log("Reachable from " + wp.name);
			if(debugRayList != null) debugRayList.Add(Tuple.Create(wp.transform.position, to));

			var transWp = wp.transform;
			var posWp = transWp.position;
			var delta = (to - posWp);
			var distance = delta.magnitude;

			return !Physics.Raycast(posWp, delta/distance, distance, LayerMask.GetMask(new []{"Blocking"}));
		}

		public IEnumerable<Item> AllInventories() 
		{
			return AllItems()
				.Select(item => item.GetComponent<Entity>())
				.Where(entity => entity != null)
				.Aggregate(FList.Create<Item>(), (a, entity) => a + entity.items);
		}

		public IEnumerable<Item> AllItems() => All().Aggregate(FList.Create<Item>(), (a, wp) => a += wp.nearbyItems);
		private IEnumerable<Waypoint> All() => _waypoints;

		public Waypoint Random() => _waypoints[UnityEngine.Random.Range(0, _waypoints.Count)];

		public Waypoint NearestTo(Vector3 pos) 
		{
			return All()
				.OrderBy(wp => 
				{
					var dir = wp.transform.position - pos;
					dir.y = 0;
					return dir.sqrMagnitude;
				})
				.First();
		}
	}
}
