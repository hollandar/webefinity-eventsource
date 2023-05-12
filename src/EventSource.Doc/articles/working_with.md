# Working with Entities

## Creating and updating a Person

Creating and updating an entity are both done in the same way.  You provide an id that you want to create or load, its up to you to make sure it refers to an existing person, or is a new never before used id for a new person.

``` c#
// Get the entity service instance
var entitySevice = serviceProvider.GetService<IEntityService>(); // Constructor injection or however

var personId = Guid.Parse("7409057f-4f16-4b59-b2ca-a78bd102ba37");

// If the person does not exists, gives me a new one.  If they do, load and replay events to bring it up to date.
var entity = await entityService.GetEntity<Person, PersonEventBase>(personId); 
await entityService.Apply(entity, new CreatePersonEvent { Name = "Johnny", MobilePhone = "0410 003 430", EmailAddress = "johnny@cash.com" });
await entityService.Apply(entity, new AddAddress { StreetNo = 14, StreetName = "Oak Close", City = "Nunnawadding", State = "Victoria", PostalCode = "3123" });
```

And that is it, the events are saved in the store, applied to the entity and stored in the entity cache for reuse later.

You should not modify the properties of the entity other than through events, doing do will cause the data to be lost because the changes are not replayed on reload of the entity.

## Retrieving a Person

Retrieveing an aggregate root is exactly the same. We just get it by its id, and apply additional events to it.  How you know which id you are interested in, that is a topic for discussion.  Kepping an external search index of people with a subset of their data is a good soltuion, but the events themselves remain the system of record.

If it is still in the cache (if you are using one) it will be returned assuming it hasnt expired.  Otherwise it will be rebuilt for you from the stored events.

``` c#
// Get the entity service instance
var entitySevice = serviceProvider.GetService<IEntityService>(); // Constructor injection or however

var personId = Guid.Parse("7409057f-4f16-4b59-b2ca-a78bd102ba37");

// If the person does not exists, gives me a new one.  If they do, load and replay events to bring it up to date.
var entity = await entityService.GetEntity<Person, PersonEventBase>(personId); 
await entityService.Apply(entity, new AddAddress { StreetNo = 17, StreetName = "Station Way", City = "Booroondara", State = "Victoria", PostalCode = "3029" });
```
