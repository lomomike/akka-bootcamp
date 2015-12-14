using System;
using System.IO;
using Akka.Actor;


namespace WinTail
{
	public class FileValidatorActor : UntypedActor
	{
		private readonly IActorRef _consoleWriterActor;
		
		public FileValidatorActor (IActorRef consoleWriterActor)
		{
			if (consoleWriterActor == null)
				throw new ArgumentNullException ("consoleWriterActor");

			_consoleWriterActor = consoleWriterActor;
		}

		protected override void OnReceive (object message)
		{
			var msg = message as string;
			if (string.IsNullOrEmpty (msg)) 
			{
				_consoleWriterActor.Tell(new Messages.NullInputError(("Imput was blank. Please try again.\n")));

				Sender.Tell(new Messages.ContinueProcessing ());
			}
			else
			{
				var valid = IsFileUri(msg);
				if (valid)
				{
					// send success to console writer
					_consoleWriterActor.Tell(new Messages.InputSuccess(
						string.Format("Starting processing for {0}", msg)));

					Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor")
						   .Tell (new TailCoordinatorActor.StartTail (msg, _consoleWriterActor));
				}
				else
				{
					// signal that input was bad
					_consoleWriterActor.Tell(new Messages.ValidationError(
						string.Format("{0} is not an existing URI on disk.", msg)
					));

					Sender.Tell (new Messages.ContinueProcessing ());
				}
			}

			Sender.Tell(new Messages.ContinueProcessing());
		}

		private static bool IsValid(string msg)
		{
			var valid = msg.Length % 2 == 0;
			return valid;
		}

		private static bool IsFileUri(string path)
		{
			return File.Exists (path);
		}
	}
}

