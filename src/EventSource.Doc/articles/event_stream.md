## What will the EventStore look like

Something like this, for the stream "Person_7409057f-4f16-4b59-b2ca-a78bd102ba37":
``` json
[
    {
        "Type":"CreatePerson",
        "Version":0,
        "Event": {
            "Name":"Johnny", "MobilePhone": "0410 003 430", "EmailAddress": "johnny@cash.com"
        }
    },
    {
        "Type":"AddAddress",
        "Version": 1,
        "Event": {
            "Id": "de933a39-afbd-42ce-84ce-8e0b281cd64a", "StreetNo": 14, 
            "StreetName": "Oak Close", "City": "Nunnawadding", "State": "Victoria", "PostalCode": "3123"
        }
    },
    {
        "Type":"AddAddress",
        "Version": 2,
        "Event": {
            "Id": "105144d0-e546-4e85-9a83-af659f9a57cb", "StreetNo": 17, 
            "StreetName": "Station Way", "City": "Booroondara", "State": "Victoria", "PostalCode": "3029"
        }
    }
]
```