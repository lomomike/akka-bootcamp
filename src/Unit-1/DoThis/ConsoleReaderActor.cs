using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for reading FROM the console. 
    /// Also responsible for calling <see cref="ActorSystem.Shutdown"/>.
    /// </summary>
    class ConsoleReaderActor : UntypedActor
    {
        public const string ExitCommand = "exit";
		public const string StartCommand = "start";

        private IActorRef _validationActor;

        public ConsoleReaderActor(IActorRef validationActor)
        {
			if (validationActor == null)
				throw new ArgumentNullException ("validationActor");
			_validationActor = validationActor;
        }

        protected override void OnReceive(object message)
        {
			if (message.Equals (StartCommand)) {
				DoPrintInstructions ();
			} 

			GetAndValidateInput ();          
        }

		#region Internal methods
		private void DoPrintInstructions()
		{
			Console.WriteLine("Please provide the URI of log file on disc.\n");
		}

		/// <summary>
		/// Reads input from console, validates it, then signals appropriate response
		/// (continue processing, error, success, etc.).
		/// </summary>
		private void GetAndValidateInput()
		{
			var message = Console.ReadLine();

			if (string.Equals (message, ExitCommand, StringComparison.InvariantCultureIgnoreCase)) {
				Context.System.Shutdown ();
				return;
			}

			_validationActor.Tell (message);
		}

		#endregion

    }
}