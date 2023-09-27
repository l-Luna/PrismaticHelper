using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using static PrismaticHelper.Entities.NinePatch.TilePart;

namespace PrismaticHelper.Entities;

public static class NinePatch{

	public enum TilePart{
		Left, Right, Top, Bottom, Inner, TopLeft, TopRight, BottomLeft, BottomRight, InnerUL, InnerUR, InnerDL, InnerDR
	}

	public class TileSpec{
		public Dictionary<TilePart, List<Vector2>> PartSprites = new();

		public static readonly TileSpec Basic, CassetteLike, Extended;

		public Vector2 GetPartSprite(TilePart part){
			return PartSprites.TryGetValue(part, out var sprites) && sprites.Count > 0
				? Calc.Random.Choose(sprites)
				: part != Inner ? GetPartSprite(Inner) : new(0, 0);
		}

		public Subsprite GetPartSubsprite(Sprite master, TilePart part){
			var sprite = GetPartSprite(part);
			return new Subsprite(master, new Rectangle((int)sprite.X, (int)sprite.Y, 8, 8));
		}

		public TileSpec Copy(){
			TileSpec copy = new();
			foreach(var kv in PartSprites){
				List<Vector2> listCopy = new List<Vector2>();
				copy.PartSprites[kv.Key] = listCopy;
				listCopy.AddRange(kv.Value);
			}
			return copy;
		}

		static TileSpec(){
			Basic = new(){
				PartSprites = {
					[TopLeft] = new() { new(0, 0) },
					[Top] = new() { new(8, 0) },
					[TopRight] = new() { new(16, 0) },
					[Right] = new() { new(16, 8) },
					[BottomRight] = new() { new(16, 16) },
					[Bottom] = new() { new(8, 16) },
					[BottomLeft] = new() { new(0, 16) },
					[Left] = new() { new(0, 8) },
					[Inner] = new() { new(8, 8) }
				}
			};

			CassetteLike = Basic.Copy();
			CassetteLike.PartSprites[InnerUR] = new(){ new(24, 0) };
			CassetteLike.PartSprites[InnerUL] = new(){ new(24, 8) };
			CassetteLike.PartSprites[InnerDR] = new(){ new(24, 16) };
			CassetteLike.PartSprites[InnerDL] = new(){ new(24, 24) };
			
			Extended = new(){
				PartSprites = {
					[TopLeft] = new() { new(8, 8), new(8, 0), new(0, 8) },
					[TopRight] = new() { new(48, 8), new(48, 0), new(56, 8) },
					[BottomLeft] = new() { new(8, 48), new(0, 48), new(8, 56) },
					[BottomRight] = new() { new(48, 48), new(56, 48), new(48, 56) },
					[InnerUL] = new() { new(0, 0) },
					[InnerUR] = new() { new(56, 0) },
					[InnerDL] = new() { new(0, 56) },
					[InnerDR] = new() { new(56, 56) },
					[Top] = new(){ new(16, 8), new(24, 8), new(32, 8), new(40, 8) },
					[Bottom] = new(){ new(16, 48), new(24, 48), new(32, 48), new(40, 48) },
					[Left] = new(){ new(8, 16), new(8, 24), new(8, 32), new(8, 40) },
					[Right] = new(){ new(48, 16), new(48, 24), new(48, 32), new(48, 40) },
					[Inner] = (
						from x in Enumerable.Range(0, 4)
						from y in Enumerable.Range(0, 4)
						select new Vector2(16 + x * 8, 16 + y * 8)
					).ToList()
				}
			};
		}
	}

	public static void CreateConnectedNinepatch<T>(T e, Sprite master, TileSpec spec, Predicate<T> isSimilar = default)
		where T : Solid
	{
		master.Visible = false;
		for(float x = e.Left; x < (double)e.Right; x += 8f){
			for(float y = e.Top; y < (double)e.Bottom; y += 8f){
				bool left = CheckForSame(x - 8, y, e, isSimilar);
				bool right = CheckForSame(x + 8, y, e, isSimilar);
				bool up = CheckForSame(x, y - 8, e, isSimilar);
				bool down = CheckForSame(x, y + 8, e, isSimilar);
				if(left && right && up && down){
					if(!CheckForSame(x + 8, y - 8, e, isSimilar)) // inner corner, up-right showing
						AddSubsprite(x, y, spec, InnerUR, e, master);
					else if(!CheckForSame(x - 8, y - 8, e, isSimilar)) // inner corner, up-left showing
						AddSubsprite(x, y, spec, InnerUL, e, master);
					else if(!CheckForSame(x + 8, y + 8, e, isSimilar)) // inner corner, down-right showing
						AddSubsprite(x, y, spec, InnerDR, e, master);
					else if(!CheckForSame(x - 8, y + 8, e, isSimilar)) // inner corner, down-left showing
						AddSubsprite(x, y, spec, InnerDL, e, master);
					else
						AddSubsprite(x, y, spec, Inner, e, master); // centre
				}else if(left && right && !up && down) // top of block
					AddSubsprite(x, y, spec, Top, e, master);
				else if(left && right && up && !down) // bottom of block
					AddSubsprite(x, y, spec, Bottom, e, master);
				else if(left && !right && up && down) // right of block
					AddSubsprite(x, y, spec, Right, e, master);
				else if(!left && right && up && down) // left of block
					AddSubsprite(x, y, spec, Left, e, master);
				else if(left && !right && !up && down) // up-right corner
					AddSubsprite(x, y, spec, TopRight, e, master);
				else if(!left && right && !up && down) // up-left corner
					AddSubsprite(x, y, spec, TopLeft, e, master);
				else if(left && !right && up && !down) // down-right corner
					AddSubsprite(x, y, spec, BottomRight, e, master);
				else if(!left && right && up && !down) // down-left corner
					AddSubsprite(x, y, spec, BottomLeft, e, master);
			}
		}
	}

	public static bool CheckForSame<T>(float x, float y, T r, Predicate<T> isSimilar)
		where T : Solid
	{
		return r.Scene.Tracker.Entities[r.GetType()]
			.Cast<T>()
			.Where(e => isSimilar?.Invoke(e) ?? true)
			.Any(entity => entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)));
	}

	public static void AddSubsprite(float x, float y, TileSpec spec, TilePart part, Entity e, Sprite master){
		var subsprite = spec.GetPartSubsprite(master, part);
		subsprite.Position = new(x - e.X, y - e.Y);
		e.Add(subsprite);
	}
}