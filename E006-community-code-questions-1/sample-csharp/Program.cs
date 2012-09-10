//  New BSD License
//  Author:
//    Jaapjan Tinbergen jaapjan@tinbergen.net
//
//  Copyright (c) 2012, Jaapjan Tinbergen
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the distribution.
//     * Neither the name of the author nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.Collections.Generic;

namespace eventquestions1
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            License lic = new License();
            lic.ChangeCustomerName("Erick Eberhart");
            lic.ChangeNumberOfSeats(5);
            foreach (var @event in lic.GetUncommitedEvents())
            {
                Console.WriteLine(@event.ToString());
            }

            Console.ReadKey();
        }
    }

    class License
    {
        private Guid identity;
        private string name;
        private int seats;
        private IList<IEvent> uncommitedevents;

        public License()
        {
            this.uncommitedevents = new List<IEvent>();
            this.identity = Guid.NewGuid();
            this.name = "New license";
            this.seats = 1;
            // Question: Set initial state here ... or generate events to set state? 
            // Especially in the case of the identity which many events would need to identify the aggregate root?
            // Generate the identity event first in that case?
        }

        public void ChangeCustomerName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                throw new InvalidOperationException("A valid name must be specified.");
            }

            // Question: These events have changed in their name. Is this right?
            ApplyNewEvent(new CustomerNameHasChanged(this.identity, newName));
        }

        // Question: These commands contain change in their name. 
        // What -do- you name a command when a customer wants less seats for a reason?
        // (Maybe they have less seats required because they fired people .. or another reason outside the domain/one you do not know?)
        public void ChangeNumberOfSeats(int newSeatCount)
        {
            if (newSeatCount < 0)
            {
                throw new InvalidOperationException("A license without seats is not allowed.");
            }

            ApplyNewEvent(new SeatCountForLicenseChanged(this.identity, newSeatCount));
        }

        public IEnumerable<IEvent> GetUncommitedEvents()
        {
            return this.uncommitedevents;
        }

        private void ApplyNewEvent(IEvent @event)
        {
            this.uncommitedevents.Add(@event);
            this.Apply(@event);
        }

        private void Apply(IEvent @event)
        {
            ((dynamic)this).When((dynamic)@event);
        }

        private void When(SeatCountForLicenseChanged @event)
        {
            // Question: Should we guard against applying events with the wrong aggregate identity here?
            this.seats = @event.Seats;
        }

        private void When(CustomerNameHasChanged @event)
        {
            this.name = @event.Name;
        }
    }

    /// <summary>
    /// Clearly defined role.
    /// </summary>
    interface IEvent
    {
    }

    class SeatCountForLicenseChanged : IEvent
    {
        // Question: Should events have a constructor to make setting its  properties mandatory and thus valid?
        public SeatCountForLicenseChanged(Guid identity, int seats)
        {
            this.identity = identity;
            this.Seats = seats;
        }

        public Guid identity
        {
            get;
            private set;
        }

        public int Seats
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format("[SeatCountForLicenseChanged: identity={0}, Seats={1}]", identity, Seats);
        }
    }

    class CustomerNameHasChanged : IEvent
    {
        public CustomerNameHasChanged(Guid identity, string name)
        {
            this.identity = identity;
            this.Name = name;
        }

        public Guid identity
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format("[CustomerNameHasChanged: identity={0}, Name={1}]", identity, Name);
        }
    }
}
