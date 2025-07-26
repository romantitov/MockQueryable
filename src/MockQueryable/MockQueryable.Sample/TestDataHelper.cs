using System;
using System.Collections.Generic;
using System.Globalization;

namespace MockQueryable.Sample;

public static class TestDataHelper
{
    public static readonly CultureInfo UsCultureInfo = new("en-US");
    public static List<UserEntity> CreateUserList(Guid? userId = null) =>
    [
        new()
        {
            Id = userId ?? Guid.NewGuid(),
            FirstName = "FirstName1", LastName = "LastName1",
            DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)
        },

        new()
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName2", LastName = "LastName2",
            DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)
        },

        new()
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName3", LastName = "LastName4",
            DateOfBirth = DateTime.Parse("01/20/2012", UsCultureInfo.DateTimeFormat)
        },

        new()
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName3", LastName = "LastName4",
            DateOfBirth = DateTime.Parse("03/20/2012", UsCultureInfo.DateTimeFormat)
        },

        new()
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName5", LastName = "LastName4",
            DateOfBirth = DateTime.Parse("01/20/2018", UsCultureInfo.DateTimeFormat)
        }

    ];
}