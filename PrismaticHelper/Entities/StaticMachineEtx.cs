using System;
using System.Collections;
using System.Reflection;
using Celeste;
using Monocle;

namespace PrismaticHelper.Entities;

// based on Frost Helper: https://github.com/JaThePlayer/FrostHelper/blob/83636a0c3da701f50afca2851672c8590e151bc8/Code/FrostHelper/Helpers/StateMachineExt.cs#L3
internal static class StateMachineExt{
	/// <summary>
	/// Adds a state to a StateMachine
	/// </summary>
	/// <returns>The index of the new state</returns>
	public static int AddState(this StateMachine machine, Func<Entity, int> onUpdate, Func<Entity, IEnumerator> coroutine = null, Action<Entity> begin = null, Action<Entity> end = null){
		int nextIndex = Expand(machine);
		// And now we add the new functions
		machine.SetCallbacks(nextIndex, () => onUpdate(machine.Entity),
			() => coroutine?.Invoke(machine.Entity),
			() => begin?.Invoke(machine.Entity),
			() => end?.Invoke(machine.Entity));
		return nextIndex;
	}

	public static int AddState(this StateMachine machine, Func<Player, int> onUpdate, Func<Player, IEnumerator> coroutine = null, Action<Player> begin = null, Action<Player> end = null){
		int nextIndex = Expand(machine);
		// And now we add the new functions
		machine.SetCallbacks(nextIndex, () => onUpdate((Player)machine.Entity),
			() => coroutine?.Invoke((Player)machine.Entity),
			() => begin?.Invoke((Player)machine.Entity),
			() => end?.Invoke((Player)machine.Entity));
		return nextIndex;
	}

	public static int Expand(this StateMachine machine){
		Action[] begins = (Action[])StateMachine_begins.GetValue(machine);
		Func<int>[] updates = (Func<int>[])StateMachine_updates.GetValue(machine);
		Action[] ends = (Action[])StateMachine_ends.GetValue(machine);
		Func<IEnumerator>[] coroutines = (Func<IEnumerator>[])StateMachine_coroutines.GetValue(machine);
		int nextIndex = begins.Length;
		// Now let's expand the arrays
		Array.Resize(ref begins, begins.Length + 1);
		Array.Resize(ref updates, begins.Length + 1);
		Array.Resize(ref ends, begins.Length + 1);
		Array.Resize(ref coroutines, coroutines.Length + 1);
		// Store the resized arrays back into the machine
		StateMachine_begins.SetValue(machine, begins);
		StateMachine_updates.SetValue(machine, updates);
		StateMachine_ends.SetValue(machine, ends);
		StateMachine_coroutines.SetValue(machine, coroutines);

		return nextIndex;
	}

	private static FieldInfo StateMachine_begins = typeof(StateMachine).GetField("begins", BindingFlags.Instance | BindingFlags.NonPublic);
	private static FieldInfo StateMachine_updates = typeof(StateMachine).GetField("updates", BindingFlags.Instance | BindingFlags.NonPublic);
	private static FieldInfo StateMachine_ends = typeof(StateMachine).GetField("ends", BindingFlags.Instance | BindingFlags.NonPublic);
	private static FieldInfo StateMachine_coroutines = typeof(StateMachine).GetField("coroutines", BindingFlags.Instance | BindingFlags.NonPublic);
}