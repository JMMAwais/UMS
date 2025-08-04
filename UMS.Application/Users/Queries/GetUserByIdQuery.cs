using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.DTO_s;
using UMS.Domain.Entities;

namespace UMS.Application.Users.Queries
{
    public class GetUserByIdQuery: IRequest<User>
    {
        public string UserId { get; set; }
        public GetUserByIdQuery(string userId)
        {
            UserId = userId;
        }
    }
}
