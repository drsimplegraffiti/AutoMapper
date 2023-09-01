using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Techie.Service
{
    public interface IRefreshHandler
    {
        Task<string> GenerateRefreshToken(int id);
    }
}