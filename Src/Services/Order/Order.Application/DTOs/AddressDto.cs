namespace Order.Application.DTOs;


public record class AddressDto
    (
        string FirstName, 
        string LastName,
        string EmailAddress,
        string AddressLine,
        string Country, 
        string State, 
        string ZipCode
    );

