using System;
using System.Net.WebSockets;
using PurePusher;

namespace PurePusherClientTest
{
	class Program
	{
		private static PurePusherClient _client;
		private static bool _init = true;

		static void Main(string[] args)
		{
			// to make the output a bit nicer we are going to do some console manipulation
			// this stuff is by no means any sort of requirement but should make it ewasy to understand what is happeneing
			Console.CursorVisible = false;
			ConsoleSpinner.SetSequence(4);
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Starting Connection!");
			Console.ResetColor();
			ConsoleSpinner.Enable();

			// intantiate an insatance of the pusher client by passing the application key and the options we want to use (if any)
			// one of the more interesting options is the ability to change the serializer being used, in this instance we will use our own
			// by implimenting the ISerializer interface and setting the Serializer option
			_client = new PurePusherClient("de504dc5763aeef9ff52", new PurePusherClientOptions { DebugMode = false, Serializer = new MySerializer()});

			// lets hook up to some events
			_client.ConnectionStateChanged += _client_ConnectionStateChanged;

			// try and connect
			if (_client.Connect())
			{
			//	ConsoleSpinner.Disable();
			//	Console.SetCursorPosition(0, Console.CursorTop - 1);
			//	Console.WriteLine("Connected!        ");
			//	ConsoleSpinner.Enable();

				// we have a connection so lets subscribe to a channel
				var ethusdChannel = _client.Subscribe("live_trades_ethusd");
				// bind to the new channel and do something with the data
				ethusdChannel.Bind("trade", data => { OnTrade(data, "USD/ETH"); });

				Console.WriteLine("Waiting for data!");
			}
			else
			{
				// something is wrong we did not get a connection
				ConsoleSpinner.Disable();
				Console.WriteLine("Failed to connect!");
			}

			Console.ReadLine();
		}

		private static void _client_ConnectionStateChanged(object sender, WebSocketState state)
		{
			ConsoleSpinner.Disable();
			var posTop = Console.CursorTop;
			var posLeft = Console.CursorLeft;

			Console.CursorLeft = 0;
			Console.CursorTop = 0;
			Console.ForegroundColor = state == WebSocketState.Open ? ConsoleColor.Green : ConsoleColor.Red;
			Console.WriteLine("Connection " + state + "!                   ");

			Console.ResetColor();
			Console.SetCursorPosition(posLeft, posTop);

			ConsoleSpinner.Enable();
		}

		// this method will be called when the channel we subscribed to gets data (line 25)
		public static void OnTrade(dynamic tradeData, string market = "")
		{
			// in here lets do someething with the data, in this case we are going to just show some text in the console
			ConsoleSpinner.Disable();
			if (_init)
			{
				Console.CursorTop = 1;
				_init = false;
			}

			Console.WriteLine(DateTime.Now.ToLongTimeString() + " Market: " + market + " Price: " + tradeData["price_str"]);
			ConsoleSpinner.Enable();
		}
	}
}
