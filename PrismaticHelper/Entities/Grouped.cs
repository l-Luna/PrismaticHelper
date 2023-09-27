using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace PrismaticHelper.Entities;

public class Grouped<E> : Component
	where E : Entity
{

	public E Leader;
	public List<E> Group = new();

	public Predicate<E> IsSimilar;
	
	protected E self;
	
	public Grouped() : base(false, false){}

	public override void Added(Entity entity){
		base.Added(entity);
		self = (E)entity;
	}

	public override void EntityAwake(){
		base.EntityAwake();

		if(Leader == null){
			Leader = self;
			CheckForSame(self);
		}
	}

	private void CheckForSame(E at){
		foreach(E other in Scene.Tracker.GetEntities<E>().OfType<E>()){
			if(other.Get<Grouped<E>>() is { Leader: null } ungrouped && (IsSimilar?.Invoke(other) ?? true)){
				if(other.CollideRect(new Rectangle((int)at.X - 1, (int)at.Y, (int)at.Width + 2, (int)at.Height))
				   || other.CollideRect(new Rectangle((int)at.X, (int)at.Y - 1, (int)at.Width, (int)at.Height + 2))){
					ungrouped.Leader = self;
					ungrouped.Group = Group;
					Group.Add(other);
					CheckForSame(other);
				}
			}
		}
	}
	
	public Rectangle Covering => Group.Select(x => x.Collider.Bounds).Aggregate(Union);

	private static Rectangle Union(Rectangle l, Rectangle r){
		int x = Math.Min(l.X, r.X), y = Math.Min(l.Y, r.Y);
		return new Rectangle(x, y, Math.Max(l.Right, r.Right) - x, Math.Max(l.Bottom, r.Bottom) - y);
	}
}