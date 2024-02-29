using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using App.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace App.Requests.Weather;

public class GetWeatherRequest : IRequest<float> { }

public class GetWeatherRequestHandler : IRequestHandler<GetWeatherRequest, float>
{
    private static float? _cache;
    
    private readonly Configuration _configuration;

    public GetWeatherRequestHandler(IOptions<Configuration> configuration)
    {
        _configuration = configuration.Value;
    }

    public async Task<float> Handle(GetWeatherRequest request, CancellationToken cancellationToken)
    {
        if (_cache is not null)
            return (float)_cache;
        
        var start = new ProcessStartInfo
        {
            FileName = $"{_configuration.Model.Path}/dist/model",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        
        Directory.SetCurrentDirectory(_configuration.Model.Path);

        using (Process? process = Process.Start(start))
        {
            if (process is null)
                throw new ApplicationException("Unable to retrieve current weather from python model. The model could not be started.");
            
            using (StreamReader reader = process.StandardOutput)
            {
                var result = await reader.ReadToEndAsync(cancellationToken);
                if (!float.TryParse(result.Trim(), out var prediction))
                    throw new ApplicationException("The model did not return a valid integer weather prediction.");

                _cache = prediction;
                return prediction;
            }
        }
    }
}