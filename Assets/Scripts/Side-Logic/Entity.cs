using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Graph;
using UnityEngine;

namespace Side_Logic
{
	public sealed class Entity : MonoBehaviour
	{
		#region VARIABLES
		
		[SerializeField] private TextMesh lblNumber, lblId;
		[SerializeField] private  Transform body, inventory;
		[SerializeField] private string initialId;
		[SerializeField] private Color initialColor;
		[SerializeField] private bool canMove;
	

		public event Action<Entity>				OnHitFloor = delegate {};
		public event Action<Entity, Transform>	OnHitWall = delegate {};
		public event Action<Entity, Item>		OnHitItem = delegate {};
		public event Action<Entity, Waypoint, bool>	OnReachDestination = delegate {};

		[SerializeField] private List<Item> initialItems;
	
		[SerializeField] private Vector3 vel;
		[SerializeField] private float speed = 2f;
	
		private List<Item> _items;
		private bool _onFloor;
		private string _label;
		private int _number;
		private Color _color;


		private Waypoint _gizmoRealTarget;
		private IEnumerable<Waypoint> _gizmoPath;

		#region GETTERS & SETTERS
		public IEnumerable<Item> items => _items;

		private string label
		{
			get => _label;
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					_label = null;
					lblId.text = "";
				}
				else
				{
					_label = value;
					lblId.text = "\u2190" + value;
				}
			}
		}

		private int number
		{
			get => _number;
			set
			{
				_number = value;
				lblNumber.text = value.ToString();
			}
		}

		private Color color
		{
			get => _color;
			set
			{
				_color = value;
				Paint(value);
			}
		}
		#endregion

		#endregion
		

		private void Awake()
		{
			_items = new List<Item>();
			vel = Vector3.zero;
			_onFloor = false;
			label = initialId;
			number = 99;
		}

		private void Start()
		{
			color = initialColor;
			canMove = true;
			foreach (var it in initialItems)
				AddItem(Instantiate(it));
		}
		#region Functionality

		public void SetMove(bool bMove) => canMove = bMove;
		
		public void SuccesFeedBuck() => lblId.text = "success";
		
		public void Healing() => StartCoroutine(HealingFeedBuck());

		IEnumerator HealingFeedBuck()
		{
			yield return new WaitForEndOfFrame();
			Debug.Log("Healing Corr ex");
		}

		#endregion
    

		#region MOVEMENT & COLLISION
		private void FixedUpdate()
		{
			if(canMove) transform.Translate(vel * (Time.fixedDeltaTime * speed));
		}
    
		public void Jump()
		{
			if (!_onFloor) return;
        
			_onFloor = false;
			GetComponent<Rigidbody>().AddForce(Vector3.up * 3 + Vector3.forward * 3, ForceMode.Impulse);

		}

		private void OnCollisionEnter(Collision col)
		{
			if (col.collider.CompareTag("Floor"))
			{
				_onFloor = true;
				OnHitFloor(this);
			}
			else if (col.collider.CompareTag("Wall"))
				OnHitWall(this, col.collider.transform);
			else
			{
				var item = col.collider.GetComponentInParent<Item>();
				if (item && item.transform.parent != inventory)
					OnHitItem(this, item);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			var e = other.GetComponent<Entity>();
			if (e != null && e != this)
			{
				Debug.Log(e.name + " hit " + name);
			}
		}
		#endregion

		#region ITEM MANAGEMENT
		public void AddItem(Item item) 
		{
			_items.Add(item);
			item.OnInventoryAdd();
			item.transform.parent = inventory;
			RefreshItemPositions();
		}

		public Item Removeitem(Item item) 
		{
			_items.Remove(item);
			item.OnInventoryRemove();
			item.transform.parent = null;
			RefreshItemPositions();
			var transItem = item.transform;
			var pos = transItem.position;
			pos += pos + Vector3.forward;
			transItem.position = pos;
			return item;
		}

		public IEnumerable<Item> RemoveAllitems()
		{
			var ret = _items;
			foreach(var item in items) 
			{
				item.OnInventoryRemove();
			}
			_items = new List<Item>();
			RefreshItemPositions();
			return ret;
		}

		private void RefreshItemPositions()
		{
			const float dist = 1.25f;
			for (int i = 0; i < _items.Count; i++)
			{
				var phi = (i + 0.5f) * Mathf.PI / (_items.Count);
				_items[i].transform.localPosition = new Vector3(-Mathf.Cos(phi) * dist, Mathf.Sin(phi) * dist, 0f);
			}
		}
		#endregion

		private Vector3 FloorPos(MonoBehaviour b) => FloorPos(b.transform.position);

		private Vector3 FloorPos(Vector3 v) => new Vector3(v.x, 0f, v.z);

		private Coroutine _navCR;
		
		public void GoTo(Vector3 destination) => _navCR = StartCoroutine(Navigate(destination));

		public void Stop() 
		{
			if(_navCR != null) StopCoroutine(_navCR);
			vel = Vector3.zero;
		}


		private IEnumerator Navigate(Vector3 destination)
		{
			var srcWp = Navigation.instance.NearestTo(transform.position);
			var dstWp = Navigation.instance.NearestTo(destination);
		
			_gizmoRealTarget = dstWp;
			var reachedDst = srcWp;

			if(srcWp != dstWp)
			{
				var path = _gizmoPath = AStarNormal<Waypoint>.Run(
					srcWp
					, dstWp
					, (wa, wb) => Vector3.Distance(wa.transform.position, wb.transform.position)
					, w => w == dstWp
					, w =>
						//w.nearbyItems.Any(it => it.type == ItemType.Door)
						//? null
						//:
						w.adyacent
							//.Where(a => a.nearbyItems.All(it => it.type != ItemType.Door))
							.Select(a => new AStarNormal<Waypoint>.Arc(a, Vector3.Distance(a.transform.position, w.transform.position)))
				);
				if(path != null) 
				{
					foreach(var next in path.Select(FloorPos)) 
					{

						while ((next - FloorPos(this)).sqrMagnitude >= 0.05f) 
						{
							vel = (next - FloorPos(this)).normalized;
							yield return null;
						}
					}
				}
				reachedDst = path.Last();
			}

			if(reachedDst == dstWp) 
			{
				vel = (FloorPos(destination) - FloorPos(this)).normalized;
				yield return new WaitUntil(() => (FloorPos(destination) - FloorPos(this)).sqrMagnitude < 0.05f);
			}
		
			vel = Vector3.zero;
			OnReachDestination(this, reachedDst, reachedDst == dstWp);
		}

		private void Paint(Color color) 
		{
			foreach(Transform trans in body)
				trans.GetComponent<Renderer>().material.color = color;
			lblNumber.color = new Color(1f-color.r, 1f-color.g, 1f-color.b);
		}

		private void OnDrawGizmos()
		{
			if (_gizmoPath == null)
				return;

			Gizmos.color = color;
			var points = _gizmoPath.Select(FloorPos);
			Vector3 last = points.First();
			foreach (var p in points.Skip(1))
			{
				Gizmos.DrawLine(p + Vector3.up, last + Vector3.up);
				last = p;
			}
			if (_gizmoRealTarget != null)
				Gizmos.DrawCube(_gizmoRealTarget.transform.position + Vector3.up * 1f, Vector3.one * 0.3f);
		}
	}
}
