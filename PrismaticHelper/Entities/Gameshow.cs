using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Celeste;
using Celeste.Mod.Entities;

using Microsoft.Xna.Framework;

using Monocle;

namespace PrismaticHelper.Entities {

	[CustomEntity("PrismaticHelper/Gameshow")]
	public class Gameshow : Entity {

		#region Question Types

		public abstract class Question {

			public abstract string Text();

			public abstract List<Choice> AvailableChoices();
		}

		public class FixedQuestion : Question {

			public string Label;
			public List<Choice> Choices;

			public override List<Choice> AvailableChoices() => Choices;
			public override string Text() => Label;
		}

		public class MathsQuestion : Question {

			public enum Type {
				Addition, Multiplication, Division, Rearranging, /*SimultaniousSolving,*/ Differentiation, Integration
			}

			public Type QuestionType;
			public List<float> Constants;
			public int NumChoices = 5;

			public MathsQuestion(Type type, int choices = 5) {
				NumChoices = choices;
				QuestionType = type;
				Constants = new();
				for(int i = 0; i < 10; i++)
					Constants.Add((Calc.Random.Next(30) + 10) / Calc.Random.Choose(2f, 1f, 4f));
			}

			public override string Text() => QuestionType switch {
				// 7 + 12
				Type.Addition => $"{Constants[0]} + {Constants[1]}",
				// 4 * 34
				Type.Multiplication => $"{Constants[0]} * {Constants[1]}",
				// 627 / 3
				Type.Division => $"{Constants[0]} / {Constants[1]}",
				// 19x + 14y = 87
				Type.Rearranging => $"{Constants[0]}x + {Constants[1]}y = {Constants[2]}",
				// y, where
				// 33x + 22y = 12
				// 22x + 33y = 87
				//Type.SimultaniousSolving => $"y, where\n{Constants[0]}x + {Constants[1]}y = {Constants[3]}\n{Constants[4]}x + {Constants[5]}y = {Constants[6]}",
				// the derivitave of (3x^4 + 8x^7 - x^-2) at 3
				Type.Differentiation => $"the derivitave of ({Constants[0]}^{Constants[1]} + {Constants[2]}^{Constants[3]} + {Constants[4]}^{Constants[5]}) at {Constants[6]}",
				// the antiderivitive of (3x^4 + 8x^7 - x^-2) from 0 to 3
				Type.Integration => $"the antiderivitave of ({Constants[0]}^{Constants[1]} + {Constants[2]}^{Constants[3]} + {Constants[4]}^{Constants[5]}) from 0 to {Constants[6]}",
				_ => throw new NotImplementedException()
			};

			public override List<Choice> AvailableChoices() {
				var ret = new List<Choice>();
				var c = Constants;
				static float diff(float a, float b, float c) => (float)(a * b * Math.Pow(c, b - 1));
				static float integ(float a, float b, float c) => (float)(a / (b + 1) * Math.Pow(c, b + 1));
				while(ret.Count < NumChoices) {
					bool correct = ret.Count == 0;
					int boundedOffset = Calc.Random.Choose(1, -1) * (1 + Calc.Random.Next((int)Calc.Random.Choose(Constants)));
					ret.Add(QuestionType switch {
						Type.Addition => correct ? new Choice(0, c[0] + c[1]) : new Choice(1, c[0] + c[1] + boundedOffset),
						Type.Multiplication => correct ? new Choice(0, c[0] * c[1]) : new Choice(1, c[0] * c[1] + boundedOffset),
						Type.Division => correct ? new Choice(0, c[0] / c[1]) : new Choice(1, c[0] / c[1] + boundedOffset),
						// ax + by = c, by = c - ax, y = (c - ax) / b
						Type.Rearranging => correct ? new Choice(0, $"{c[2] / c[1]} - {c[0] / c[1]}x") : new Choice(1, $"{c[2] / (c[1] + boundedOffset / 2)} - {c[0] / (c[1] / boundedOffset / 3)}x"),
						// d/dx (ax^b + cx^d + ex^f) = abg^(b-1) + cdg^(d-1) + efg^(f-1)
						Type.Differentiation => correct ? new Choice(0, diff(c[0], c[1], c[6]) + diff(c[2], c[3], c[6]) + diff(c[4], c[5], c[6])) : new Choice(1, diff(c[0], c[1], c[6]) + diff(c[2], c[3], c[6]) + diff(c[4], c[5], c[6]) + boundedOffset),
						// ∫[0,g](ax^b + cx^d + ex^f)dx = (a/(b+1))g^(b+1) + (c/(d+1))g^(d+1) + (e/(f+1))g^(f+1)
						Type.Integration => correct ? new Choice(0, integ(c[0], c[1], c[6]) + integ(c[2], c[3], c[6]) + integ(c[4], c[5], c[6])) : new Choice(1, integ(c[0], c[1], c[6]) + integ(c[2], c[3], c[6]) + integ(c[4], c[5], c[6]) + boundedOffset),
						_ => null
					});
				}
				return ret;
			}
		}

		public class Choice {

			// 0 = correct, 1 = incorrect, 2 = unknown
			public int Type;

			public string Text;

			public Choice(int type, string text) {
				Type = type;
				Text = text;
			}

			public Choice(int type, object text) {
				Type = type;
				Text = text.ToString();
			}
		}

		#endregion

		#region Gameshow question/answer entities

		public class GameshowQuestion : Entity {

		}

		public class GameshowChoice : Entity {

			private readonly Choice choice;

			public GameshowChoice(Choice choice) {
				this.choice = choice;
			}

			public override void Render() {
				base.Render();

			}
		}

		public class GameshowLabel : Entity {

		}

		#endregion

		#region Gameshow entity

		EntityID ID;

		public List<Question> Questions = new();
		public int CurQuestion = 0;

		public Gameshow(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
			ID = id;
			
		}

		#endregion
	}
}