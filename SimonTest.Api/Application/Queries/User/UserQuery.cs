﻿namespace Simon_Test.Application.Queries.User;

using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Simon_Test.Application.Constants;
using Simon_Test.Application.DTOs;
using SimonTest.Infrastructure.Persistence;
using TradePlus.ResultData.Abstract.Generics;
using static TradePlus.ResultData.ResultFactory;

public record UserQuery(
    Guid? Id,
    [EmailAddress] string? Email) : IRequest<IResult<UserQuery.QueryResult>>
{
    public class Handler : IRequestHandler<UserQuery, IResult<QueryResult>>
    {
        private readonly MyDbContext _context;

        public Handler(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IResult<QueryResult>> Handle(
            UserQuery request,
            CancellationToken cancellationToken)
        {
            var usersQuery = _context.Users
                .AsNoTracking();

            if (request.Id is not null)
            {
                usersQuery = usersQuery
                    .Where(s => s.Id == request.Id);
            }
            else
            {
                if (request.Email is null)
                    return Failure<QueryResult>(ValidationMessage.UserIdOrEmailShouldBePresent);

                usersQuery = usersQuery
                    .Where(s => s.Email == request.Email);
            }

            var user = await usersQuery
                .Select(
                    u => new QueryResult(
                        u.Id,
                        u.FirstName,
                        u.FirstName,
                        u.Email,
                        u.UserGroups
                            .Select(ug => ug.Group)
                            .Select(
                                g => new GetGroupDto(
                                    g.Id,
                                    g.Name))
                            .ToList()))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
                return Failure<QueryResult>(ValidationMessage.UserNotFound);

            return Success(user);
        }
    }

    public record QueryResult(
        Guid Id,
        string? FirstName,
        string? LastName,
        string Email,
        List<GetGroupDto> Groups);
}