# Entities

Entity classes are configured so they know what keys they have, and how to apply events over their lifecycle, the aggregate root is the root entity in a tree of data that can be changed by events.

Knowing what your entities are, and which are your aggregate roots is a topic for discussion and some domain modelling.

In this example model Person is the aggregate root:

``` c#
public class Person: IAggregateRoot<PersonEventBase>, IAggregateRootWithGuidKey {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = String.Empty;
    public string MobilePhone { get; set; } = String.Empty;
    public string EmailAddress { get; set; } = String.Empty;
    public List<Address> Addresses { get; set; } = new();

    // Serializers for the events that pllay to the person root
    public List<EventSerializer<PersonEventBase>> Serializers
    {
        get
        {
            return new List<EventSerializer<PersonEventBase>>()
            {
                new EventSerializer<PersonEventBase, CreatePersonEvent>("CreatePerson"),
                new EventSerializer<PersonEventBase, AddAddress>("AddAddress"),
            };
        }
    }

    // Handler for the create person event
    public void Apply(CreatePerson @event)
    {
        this.Name = @event.Name;
        this.MobilePhone = @event.MobilePhone;
        this.EmailAddress = @event.EmailAddress;
    }

    // Handler for the add address event
    public void Apply(AddAddress @event)
    {
        if (!this.Addresses.Where(r => r.Id == @event.Id).Any())
        {
            var address = new Address()
            {
                Id = @event.Id,
                StreetNo = @event.StreetNo,
                StreetName = @event.StreetName,
                City = @event.City,
                State = @event.State,
                PostalCode = @event.PostalCode
            };
            this.Addresses.Add(address);
        }
    }
}

// Address, unlike Person, is not an aggregate root, its just a data container.
public class Address
{
    public Guid Id { get; set; }
    public int StreetNo { get; set; } = 0;
    public string StreetName { get; set; } = String.Empty;
    public string PostalCode { get; set; } = String.Empty;
    public string City { get; set; } = String.Empty;
    public string State { get; set; } = String.Empty;
}
```

Now it is important that we define the shape of the events that can happen, and their base class PersonEventBase:

``` c#
public abstract class PersonEventBase { }

public class CreatePersonEvent: PersonEventBase
{
    public string Name { get; set; } = String.Empty;
    public string MobilePhone { get; set; } = String.Empty;
    public string EmailAddress { get; set; } = String.Empty;
}

public class AddAddress: PersonEventBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int StreetNo { get; set; } = 0;
    public string StreetName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}
```

That is all there is to the entity model.  Many questions are left outstanding due to missing events, such has:
1. What if the persons email address changes, can we update it?
2. Is it possible to modify the address, or can we only remove and add them?
3. And many others... which will be driven by your requirements.

The only special thing about events is that they must be serializable using System.Text.Json.JsonSerializer, and they must all derive from a common base class.