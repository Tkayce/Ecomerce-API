using ECommerceAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controller
{
    public interface IAppCustomerController
    {
        Task<IActionResult> Register(AppCustomerDTO request);
    }
}