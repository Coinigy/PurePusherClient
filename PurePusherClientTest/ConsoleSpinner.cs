using System;
using System.Threading.Tasks;

namespace PurePusherClientTest
{
	public static class ConsoleSpinner
	{
		private static int _counter;
		private static string[] _sequence;
		private static bool _enabled;

		public static void SetSequence(int i = 0)
		{
			switch (i)
			{
				case 1:
					_sequence = new[] { ".", "o", "0", "o" };
					break;
				case 2:
					_sequence = new[] { "+", "x" };
					break;
				case 3:
					_sequence = new[] { "V", "<", "^", ">" };
					break;
				case 4:
					_sequence = new[] { ".   ", "..  ", "... ", "...." };
					break;
				case 0:
				default:
					_sequence = new[] { "/", "-", "\\", "|" };
					break;
			}
		}

		public static void Enable()
		{
			Console.CursorLeft = 0;
			if (_sequence == null)
				SetSequence();

			if (_enabled) return;
			_counter = 0;
			_enabled = true;
			Turn();
		}

		public static void Disable()
		{
			_enabled = false;
			_counter = 0;

			Task.Delay(160).Wait();
		}

		private static void Turn()
		{
			Task.Run(() =>
			{
				while (_enabled)
				{
					_counter++;

					if (_counter >= _sequence.Length)
						_counter = 0;

					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write(_sequence[_counter]);
					Console.ResetColor();
					Console.SetCursorPosition(Console.CursorLeft - _sequence[_counter].Length, Console.CursorTop);

					Task.Delay(150).Wait();
				}
			});
		}
	}
}
