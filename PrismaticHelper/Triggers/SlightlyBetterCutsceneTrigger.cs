using Celeste;
using Celeste.Mod.Entities;

using Microsoft.Xna.Framework;

using PrismaticHelper.Entities;

namespace PrismaticHelper.Triggers {

	[CustomEntity("PrismaticHelper/SlightlyBetterCutsceneTrigger")]
	internal class SlightlyBetterCutsceneTrigger : Trigger {

		public bool Triggered = false;
		public bool OnlyOnce;
		public int DeathCount;

		public string Cutscene;

		public EntityID ID;

		public SlightlyBetterCutsceneTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
			ID = id;
			OnlyOnce = data.Bool("onlyOnce");
			Cutscene = data.Attr("cutscene");
			DeathCount = data.Int("deathCount", -1);
		}

		public override void OnEnter(Player player) {
			base.OnEnter(player);

			if(Triggered)
				return;

			Scene.Add(new DialogCutscene(Cutscene, player, false));
			if(OnlyOnce)
				(Scene as Level).Session.DoNotLoad.Add(ID);

			Triggered = true;
		}
	}
}
