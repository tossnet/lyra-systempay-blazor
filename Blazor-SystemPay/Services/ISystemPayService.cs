using System;
using System.Threading.Tasks;

namespace Blazor_SystemPay.Services
{
    public interface ISystemPayService
    {
        Task<string> GetFormToken(string JSON_Order);
    }
}
