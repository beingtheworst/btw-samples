package com.beingtheworst.e004;

import java.util.ArrayList;
import java.util.List;

public class Program {

	// let's define our list of commands that the factory can carry out.
	public static class FactoryImplementation1 {
		// the methods below are linguistically equivalent to a command message
		// that could be sent to this factory. A command such as:
		// public class assignEmployeeToFactory
		// {
		// private String employeeName;
		//
		// public String getEmployeeName() {
		// return employeeName;
		// }
		//
		// public void setEmployeeName(String employeeName) {
		// this.employeeName = employeeName;
		// }
		// }

		// in this sample we will not create command messages to represent
		// and call these methods, we will just use the methods themselves to be
		// our
		// "commands" for convenience.

		public void assignEmployeeToFactory(String employeeName) {
		}

		public void transferShipmentToCargoBay(String shipmentName,
				CarPart... parts) {
		}

		public void unloadShipmentFromCargoBay(String employeeName) {
		}

		public void produceCar(String employeeName, String carModel) {
		}
	}

	// these factory methods could contain the following elements (which can be
	// really complex or can be optional):
	// * Checks (aka "guards") to see if an operation is allowed
	// * some work that might involve calculations, thinking, access to some
	// tooling
	// * Events that we write to the journal to mark the work as being done.
	// These elements are noted as comments inside of the methods below for now
	public static class FactoryImplementation2 {

		public void assignEmployeeToFactory(String employeeName) {
			// checkIfEmployeeCanBeAssignedToFactory(employeeName);
			// doPaperWork();
			// recordThatEmployeeAssignedToFactory(employeeName);
		}

		public void transferShipmentToCargoBay(String shipmentName,
				CarPart... parts) {
			// checkIfCargoBayHasFreeSpace(parts);
			// doRealWork("unloading supplies...");
			// doPaperWork("Signing the shipment acceptance form");
			// recordThatSuppliesAreAvailableInCargoBay()
		}

		public void UnloadShipmentFromCargoBay(String employeeName) {
			// doRealWork("passing supplies");
			// recordThatSuppliesWereUnloadedFromCargoBay()
		}

		public void ProduceCar(String employeeName, String carModel) {
			// checkIfWeHaveEnoughSpareParts
			// checkIfEmployeeIsAvailable
			// doRealWork
			// recordThatCarWasProduced
		}
	}

	// Now let's "unwrap" AssignEmployeeToFactory
	// we'll start by adding a list of employees

	public static class FactoryImplementation3 {
		// THE Factory Journal!
		// Where all things that happen inside of the factory are recorded
		public List<Event> journalOfFactoryEvents = new ArrayList<Event>();

		// internal "state" variables
		// these are the things that hold the data that represents
		// our current understanding of the state of the factory
		// they get their data from the methods that use them while the methods
		// react to events
		List<String> _ourListOfEmployeeNames = new ArrayList<String>();
		List<CarPart[]> _shipmentsWaitingToBeUnloaded = new ArrayList<CarPart[]>();

		public void assignEmployeeToFactory(String employeeName) {
			System.out.println(String.format(
					"?> Command: Assign employee %s to factory", employeeName));

			// Hey look, a business rule implementation!
			if (_ourListOfEmployeeNames.contains(employeeName)) {
				// yes, this is really weird check, but this factory has really
				// strict rules.
				// manager should've remembered that!
				System.out.println(String.format(
						":> the name of '%s' only one employee can have",
						employeeName));

				return;
			}

			// another check that needs to happen when assigning employees to
			// the factory
			// multiple options to prove this critical business rule:
			// John Bender:
			// http://en.wikipedia.org/wiki/John_Bender_(character)#Main_characters
			// Bender Bending Rodriguez:
			// http://en.wikipedia.org/wiki/Bender_(Futurama)
			if ("bender".equalsIgnoreCase(employeeName)) {
				System.out
						.println(":> Guys with the name 'bender' are trouble.");
				return;
			}

			doPaperWork("Assign employee to the factory");
			recordThat(new EmployeeAssignedToFactory(employeeName));
		}

		public void transferShipmentToCargoBay(String shipmentName,
				CarPart... parts) {
			System.out.println("?> Command: transfer shipment to cargo bay");
			if (_ourListOfEmployeeNames.isEmpty()) {
				System.out
						.println(":> There has to be somebody at factory in order to accept shipment");
				return;
			}

			if (_shipmentsWaitingToBeUnloaded.size() > 2) {
				System.out
						.println(":> More than two shipments can't fit into this cargo bay :(");
				return;
			}

			doRealWork("opening cargo bay doors");
			recordThat(new ShipmentTransferredToCargoBay(shipmentName, parts));

			int totalCountOfParts = 0;
			for (CarPart part : parts) {
				totalCountOfParts = part.quantity;
			}

			if (totalCountOfParts > 10) {
				recordThat(new CurseWordUttered(
						"Boltov tebe v korobky peredach",
						"awe in the face of the amount of parts delivered"));
			}
		}

		void doPaperWork(String workName) {
			System.out.println(String.format(" > Work:  papers... %s... ",
					workName));
			try {
				Thread.sleep(1000);
			} catch (InterruptedException e) {
			}
		}

		void doRealWork(String workName) {
			System.out.println(String.format(" > Work:  heavy stuff... %s...",
					workName));
			try {
				Thread.sleep(1000);
			} catch (InterruptedException e) {
			}
		}

		// Remember that Factory Journal from above that is for writing
		// everything down? Here is where we record stuff in it.
		void recordThat(Event e) {
			journalOfFactoryEvents.add(e);

			// we also announce this event inside of the factory.
			// This way, all workers will immediately know
			// what is going on inside. In essence we are telling the compiler
			// to call one of the methods right below this"RecordThat" method.
			// The "dynamic" syntax below is a shortcut we are using so we don't
			// have to create a large if/else block for a bunch of specific
			// event types.
			// "Call this factory's instance of the AnnounceInsideFactory method
			// that has a method signature of:
			// announceInsideFactory(WhateverTheCurrentTypeIsOfThe-e-EventThatWasPassedIn)".

			if (e instanceof EmployeeAssignedToFactory) {
				announceInsideFactory((EmployeeAssignedToFactory) e);
			}
			if (e instanceof ShipmentTransferredToCargoBay) {
				announceInsideFactory((ShipmentTransferredToCargoBay) e);
			}
			if (e instanceof CurseWordUttered) {
				announceInsideFactory((CurseWordUttered) e);
			}

			// also print to console, just because we want to know
			System.out.println(String.format("!> Event: %s", e));
		}

		// announcements inside the factory that
		// get called by the dynamic code shortcut above.
		// As these methods change the content inside of the lists they call,
		// our understanding of the current state of the factory is updated.
		// It is important to note that the official state of the factory
		// that these methods change, only changes AFTER each event they react
		// to
		// has been RECORDED in the journal. If an event hasn't been recorded,
		// the state
		// of the factory WILL NOT CHANGE. State changes are ALWAYS reflected in
		// the
		// stream of events inside of the journal because these methods are not
		// executed until events have been logged to the journal.
		// This is a very powerful aspect of event sourcing (ES).
		// We should NEVER directly modify the state variables
		// (by calling the list directly for example), they are only ever
		// modifed
		// as side effects of events that have occured and have been logged.
		// Pretty much ensures a perfect audit log of what has happened.

		void announceInsideFactory(EmployeeAssignedToFactory e) {
			_ourListOfEmployeeNames.add(e.employeeName);
		}

		void announceInsideFactory(ShipmentTransferredToCargoBay e) {
			_shipmentsWaitingToBeUnloaded.add(e.carParts);
		}

		void announceInsideFactory(CurseWordUttered e) {

		}

	}

	// let's run this implementation3 of the factory
	// (right-click on this project in eclipse and choose
	// "Run as... Java Application"

	public static void main(String[] args) {
		System.out.println("A new day at the factory starts...\r\n");
		FactoryImplementation3 factory = new FactoryImplementation3();

		factory.transferShipmentToCargoBay("chassis",
				new CarPart[] { new CarPart("chassis", 4), });

		factory.assignEmployeeToFactory("yoda");
		factory.assignEmployeeToFactory("luke");
		// Hmm, a duplicate employee name, wonder if that will work?
		factory.assignEmployeeToFactory("yoda");
		// An employee named "bender", why is that ringing a bell?
		factory.assignEmployeeToFactory("bender");

		factory.transferShipmentToCargoBay("model T spare parts",
				new CarPart[] { new CarPart("wheels", 20),
						new CarPart("engine", 7),
						new CarPart("bits and pieces", 2) });

		System.out
				.println("\r\nIt's the end of the day. Let's read our journal of events once more:\r\n");
		System.out
				.println("\r\nWe should only see events below that were actually allowed to be recorded.\r\n");
		for (Event e : factory.journalOfFactoryEvents) {
			System.out.println(String.format("!> %s", e));
		}

		System.out
				.println("\r\nIt seems, this was an interesting day!  Two Yoda's there should be not!");
	}

	public static class EmployeeAssignedToFactory implements Event {
		private String employeeName;

		public EmployeeAssignedToFactory(String employeeName) {
			this.employeeName = employeeName;
		}

		public String toString() {
			return String.format("new worker joins our forces: '%s'",
					employeeName);
		}
	}

	public static class CurseWordUttered implements Event {
		private String theWord;
		private String meaning;

		public CurseWordUttered(String theWord, String meaning) {
			this.theWord = theWord;
			this.meaning = meaning;
		}

		public String toString() {
			return String.format(
					"'%s' was heard within the walls. It meant:\r\n    '%s'",
					theWord, meaning);
		}
	}

	public static class ShipmentTransferredToCargoBay implements Event {
		private String shipmentName;
		private CarPart[] carParts;

		public ShipmentTransferredToCargoBay(String shipmentName,
				CarPart... carParts) {
			this.shipmentName = shipmentName;
			this.carParts = carParts;
		}

		public String toString() {
			StringBuilder builder = new StringBuilder();
			builder.append(String.format(
					"Shipment '%s' transferred to cargo bay:\n", shipmentName));
			for (CarPart carPart : carParts) {
				builder.append(String.format("     %s %d pcs\n", carPart.name,
						carPart.quantity));
			}
			return builder.toString();
		}
	}

	public static interface Event {

	}

	public static class Factory {
		public void assignEmployeeToFactory(String name) {
		}

		public void shipSuppliesToCargoBay(String shipment, CarPart... parts) {
		}

		public void unloadSuppliesFromCargoBay(String employee) {
		}

		public void produceCar(String employee, String carModel) {
		}
	}

	public static class CarPart {
		public String name;
		public int quantity;

		public CarPart(String name, int quantity) {
			this.name = name;
			this.quantity = quantity;
		}
	}

}
