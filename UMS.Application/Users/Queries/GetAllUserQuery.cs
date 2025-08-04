using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.DTO_s;

namespace UMS.Application.Users.Queries
{
    public class GetAllUserQuery: IRequest<List<UserDTO>> { }
}
