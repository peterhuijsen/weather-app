using System.Threading.Tasks;
using App.Models.Controllers.Users;
using App.Requests.Weather;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    
    [HttpGet("get")]
    public async Task<float> Get([FromServices] IMediator mediator)
        => await mediator.Send(new GetWeatherRequest());
}